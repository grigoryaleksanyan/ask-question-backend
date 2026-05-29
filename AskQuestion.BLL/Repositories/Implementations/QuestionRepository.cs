using AskQuestion.BLL.DTO;
using AskQuestion.BLL.DTO.Question;
using AskQuestion.Core.Enums;
using AskQuestion.DAL;
using AskQuestion.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace AskQuestion.BLL.Repositories.Implementations
{
    public class QuestionRepository(DataContext dataContext) : IQuestionRepository
    {
        public async Task<PaginatedResult<QuestionDto>> GetAllAsync(
            int page = 1,
            int pageSize = 10,
            int? status = null,
            string? speaker = null,
            string? area = null,
            string? search = null,
            string sortOrder = "desc")
        {
            pageSize = Math.Clamp(pageSize, 1, 50);
            page = Math.Max(page, 1);

            IQueryable<Question> query = dataContext.Questions.AsNoTracking();

            if (status.HasValue)
            {
                query = query.Where(q => q.Status == status.Value);
            }

            if (!string.IsNullOrWhiteSpace(speaker))
            {
                query = query.Where(q => q.Speaker == speaker);
            }

            if (!string.IsNullOrWhiteSpace(area))
            {
                query = query.Where(q => q.Area == area);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(q => q.Text.ToLower().Contains(searchLower));
            }

            int totalCount = await query.CountAsync();

            query = sortOrder == "asc"
                ? query.OrderBy(q => q.Created)
                : query.OrderByDescending(q => q.Created);

            List<QuestionDto> items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(q => new QuestionDto
                {
                    Id = q.Id,
                    Text = q.Text,
                    Author = q.Author,
                    Area = q.Area,
                    Speaker = q.Speaker,
                    Likes = q.Likes,
                    Dislikes = q.Dislikes,
                    Views = q.Views,
                    Status = q.Status,
                    Created = q.Created,
                    Answered = q.Answered,
                })
                .ToListAsync();

            return new PaginatedResult<QuestionDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
            };
        }

        public async Task<IEnumerable<QuestionDto>> GetPopularQuestionsAsync()
        {
            IEnumerable<QuestionDto> questions = await dataContext.Questions
                .AsNoTracking()
                .OrderByDescending(question => question.Likes)
                    .ThenByDescending(question => question.Created)
                .Take(5)
                .Select(question => new QuestionDto
                {
                    Id = question.Id,
                    Text = question.Text,
                    Author = question.Author,
                    Area = question.Area,
                    Speaker = question.Speaker,
                    Likes = question.Likes,
                    Dislikes = question.Dislikes,
                    Views = question.Views,
                    Status = question.Status,
                    Created = question.Created,
                    Answered = question.Answered
                })
                .ToListAsync();

            return questions;
        }

        public async Task<QuestionDto?> GetByIdAsync(Guid id)
        {
            Question? question = await dataContext.Questions
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == default)
            {
                return null;
            }

            QuestionDto questionDTO = new()
            {
                Id = question.Id,
                Text = question.Text,
                Author = question.Author,
                Area = question.Area,
                Speaker = question.Speaker,
                Likes = question.Likes,
                Dislikes = question.Dislikes,
                Views = question.Views,
                Status = question.Status,
                Created = question.Created,
                Answered = question.Answered
            };

            return questionDTO;
        }

        public async Task<Guid> CreateAsync(QuestionCreateDto questionCreateDto)
        {
            Question question = new()
            {
                Text = questionCreateDto.Text,
                Author = questionCreateDto.Author,
                Area = questionCreateDto.Area,
                Speaker = questionCreateDto.Speaker,
                Created = DateTimeOffset.UtcNow,
            };

            await dataContext.AddAsync(question);
            await dataContext.SaveChangesAsync();

            return question.Id;
        }

        public async Task UpdateAsync(Guid id, QuestionUpdateDto questionUpdateDto)
        {
            Question? question = await dataContext.Questions
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == default)
            {
                throw new InvalidOperationException("Объект не найден");
            }

            question.Text = questionUpdateDto.Text;
            question.Author = questionUpdateDto.Author;
            question.Area = questionUpdateDto.Area;
            question.Speaker = questionUpdateDto.Speaker;

            await dataContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            Question? question = await dataContext.Questions
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == default)
            {
                throw new InvalidOperationException("Объект не найден");
            }

            dataContext.Remove(question);
            await dataContext.SaveChangesAsync();
        }

        public async Task<VoteResultDto> ToggleLikeAsync(Guid questionId, Guid visitorId)
        {
            Question? question = await dataContext.Questions.FindAsync(questionId)
                ?? throw new InvalidOperationException("Вопрос не найден");

            QuestionVote? existingVote = await dataContext.QuestionVotes.FindAsync(questionId, visitorId);

            if (existingVote != null)
            {
                if (existingVote.VoteType == VoteType.Like)
                {
                    dataContext.QuestionVotes.Remove(existingVote);
                    question.Likes--;
                    await dataContext.SaveChangesAsync();
                    return new VoteResultDto { Likes = question.Likes, Dislikes = question.Dislikes, UserVote = null };
                }

                existingVote.VoteType = VoteType.Like;
                question.Dislikes--;
                question.Likes++;
                await dataContext.SaveChangesAsync();
                return new VoteResultDto { Likes = question.Likes, Dislikes = question.Dislikes, UserVote = VoteType.Like };
            }

            dataContext.QuestionVotes.Add(new QuestionVote { QuestionId = questionId, VisitorId = visitorId, VoteType = VoteType.Like });
            question.Likes++;
            await dataContext.SaveChangesAsync();
            return new VoteResultDto { Likes = question.Likes, Dislikes = question.Dislikes, UserVote = VoteType.Like };
        }

        public async Task<VoteResultDto> ToggleDislikeAsync(Guid questionId, Guid visitorId)
        {
            Question? question = await dataContext.Questions.FindAsync(questionId)
                ?? throw new InvalidOperationException("Вопрос не найден");

            QuestionVote? existingVote = await dataContext.QuestionVotes.FindAsync(questionId, visitorId);

            if (existingVote != null)
            {
                if (existingVote.VoteType == VoteType.Dislike)
                {
                    dataContext.QuestionVotes.Remove(existingVote);
                    question.Dislikes--;
                    await dataContext.SaveChangesAsync();
                    return new VoteResultDto { Likes = question.Likes, Dislikes = question.Dislikes, UserVote = null };
                }

                existingVote.VoteType = VoteType.Dislike;
                question.Likes--;
                question.Dislikes++;
                await dataContext.SaveChangesAsync();
                return new VoteResultDto { Likes = question.Likes, Dislikes = question.Dislikes, UserVote = VoteType.Dislike };
            }

            dataContext.QuestionVotes.Add(new QuestionVote { QuestionId = questionId, VisitorId = visitorId, VoteType = VoteType.Dislike });
            question.Dislikes++;
            await dataContext.SaveChangesAsync();
            return new VoteResultDto { Likes = question.Likes, Dislikes = question.Dislikes, UserVote = VoteType.Dislike };
        }

        public async Task IncrementViewsAsync(Guid questionId)
        {
            Question? question = await dataContext.Questions.FindAsync(questionId);
            if (question == null) return;

            question.Views++;
            await dataContext.SaveChangesAsync();
        }

        public async Task<VoteType?> GetUserVoteAsync(Guid questionId, Guid visitorId)
        {
            QuestionVote? vote = await dataContext.QuestionVotes.FindAsync(questionId, visitorId);
            return vote?.VoteType;
        }
    }
}
