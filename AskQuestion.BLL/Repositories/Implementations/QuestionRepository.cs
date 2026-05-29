using AskQuestion.BLL.DTO;
using AskQuestion.BLL.DTO.Question;
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
    }
}
