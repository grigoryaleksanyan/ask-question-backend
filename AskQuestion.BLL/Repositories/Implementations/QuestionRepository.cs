using AskQuestion.BLL.DTO;
using AskQuestion.BLL.DTO.Question;
using AskQuestion.BLL.Email;
using AskQuestion.BLL.Helpers;
using AskQuestion.Core.Enums;
using AskQuestion.DAL;
using AskQuestion.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AskQuestion.BLL.Repositories.Implementations
{
    public class QuestionRepository(
        DataContext dataContext,
        IEmailSender emailSender,
        IOptions<SmtpSettings> smtpSettings,
        IHtmlSanitizerService htmlSanitizer) : IQuestionRepository
    {
        private readonly SmtpSettings _smtpSettings = smtpSettings.Value;
        public async Task<PaginatedResult<QuestionDto>> GetAllAsync(
            int page = 1,
            int pageSize = 10,
            int? status = null,
            Guid? speakerId = null,
            Guid? areaId = null,
            string? search = null,
            string sortOrder = "desc")
        {
            pageSize = Math.Clamp(pageSize, 1, 50);
            page = Math.Max(page, 1);

            IQueryable<Question> query = dataContext.Questions
                .AsNoTracking()
                .Include(q => q.SpeakerUser)
                    .ThenInclude(u => u!.UserDetails)
                .Include(q => q.AreaEntity);

            if (status.HasValue)
            {
                query = query.Where(q => q.Status == status.Value);
            }

            if (speakerId.HasValue)
            {
                query = query.Where(q => q.SpeakerId == speakerId.Value);
            }

            if (areaId.HasValue)
            {
                query = query.Where(q => q.AreaId == areaId.Value);
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

            List<Question> questions = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            List<QuestionDto> items = questions.Select(q => new QuestionDto
            {
                Id = q.Id,
                Text = q.Text,
                Author = q.Author,
                AreaId = q.AreaId,
                AreaTitle = q.AreaEntity?.Title,
                SpeakerId = q.SpeakerId,
                SpeakerName = q.SpeakerUser?.UserDetails?.GetFullName(),
                Likes = q.Likes,
                Dislikes = q.Dislikes,
                Views = q.Views,
                Status = q.Status,
                Comment = q.Comment,
                Created = q.Created,
                Answered = q.Answered,
            }).ToList();

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
            List<Question> questions = await dataContext.Questions
                .AsNoTracking()
                .Include(q => q.SpeakerUser)
                    .ThenInclude(u => u!.UserDetails)
                .Include(q => q.AreaEntity)
                .Where(q => q.Status == (int)QuestionStatus.New || q.Status == (int)QuestionStatus.InFocus)
                .OrderByDescending(question => question.Likes)
                    .ThenByDescending(question => question.Created)
                .Take(5)
                .ToListAsync();

            IEnumerable<QuestionDto> result = questions.Select(q => new QuestionDto
            {
                Id = q.Id,
                Text = q.Text,
                Author = q.Author,
                AreaId = q.AreaId,
                AreaTitle = q.AreaEntity?.Title,
                SpeakerId = q.SpeakerId,
                SpeakerName = q.SpeakerUser?.UserDetails?.GetFullName(),
                Likes = q.Likes,
                Dislikes = q.Dislikes,
                Views = q.Views,
                Status = q.Status,
                Comment = q.Comment,
                Created = q.Created,
                Answered = q.Answered
            });

            return result;
        }

        public async Task<QuestionDto?> GetByIdAsync(Guid id)
        {
            Question? question = await dataContext.Questions
                .AsNoTracking()
                .Include(q => q.SpeakerUser)
                    .ThenInclude(u => u!.UserDetails)
                .Include(q => q.AreaEntity)
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
                AreaId = question.AreaId,
                AreaTitle = question.AreaEntity?.Title,
                SpeakerId = question.SpeakerId,
                SpeakerName = question.SpeakerUser?.UserDetails?.GetFullName(),
                Likes = question.Likes,
                Dislikes = question.Dislikes,
                Views = question.Views,
                Status = question.Status,
                Comment = question.Comment,
                Created = question.Created,
                Answered = question.Answered
            };

            return questionDTO;
        }

        public async Task<Guid> CreateAsync(QuestionCreateDto questionCreateDto)
        {
            Question question = new()
            {
                Text = htmlSanitizer.Sanitize(questionCreateDto.Text),
                Author = htmlSanitizer.Sanitize(questionCreateDto.Author),
                AreaId = questionCreateDto.AreaId,
                SpeakerId = questionCreateDto.SpeakerId,
                Created = DateTimeOffset.UtcNow,
            };

            await dataContext.AddAsync(question);
            await dataContext.SaveChangesAsync();

            if (questionCreateDto.SpeakerId.HasValue)
            {
                var speaker = await dataContext.Users
                    .AsNoTracking()
                    .Include(u => u.UserDetails)
                    .FirstOrDefaultAsync(u => u.Id == questionCreateDto.SpeakerId.Value);

                if (speaker?.UserDetails != null)
                {
                    var emailMessage = EmailTemplateBuilder.BuildNewQuestionNotification(
                        toEmail: speaker.Email,
                        toName: speaker.UserDetails.GetFullName(),
                        questionText: question.Text,
                        questionUrl: $"{_smtpSettings.BaseUrl}/admin-questions/{question.Id}");

                    await emailSender.EnqueueAsync(emailMessage);
                }
            }

            return question.Id;
        }

        public async Task<QuestionDto> UpdateAsync(Guid id, QuestionUpdateDto questionUpdateDto)
        {
            Question? question = await dataContext.Questions
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == default)
            {
                throw new InvalidOperationException("Объект не найден");
            }

            question.Text = htmlSanitizer.Sanitize(questionUpdateDto.Text);
            question.Author = htmlSanitizer.Sanitize(questionUpdateDto.Author);
            question.AreaId = questionUpdateDto.AreaId;
            question.SpeakerId = questionUpdateDto.SpeakerId;

            await dataContext.SaveChangesAsync();

            return new QuestionDto
            {
                Id = question.Id,
                Text = question.Text,
                Author = question.Author,
                AreaId = question.AreaId,
                SpeakerId = question.SpeakerId,
                Views = question.Views,
                Likes = question.Likes,
                Dislikes = question.Dislikes,
                Status = question.Status,
                Comment = question.Comment,
                Created = question.Created,
                Answered = question.Answered,
            };
        }

        public async Task DeleteAsync(Guid id)
        {
            Question? question = await dataContext.Questions
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

        public async Task ChangeStatusAsync(QuestionStatusChangeDto statusChangeDto)
        {
            Question? question = await dataContext.Questions
                .FirstOrDefaultAsync(q => q.Id == statusChangeDto.QuestionId)
                ?? throw new InvalidOperationException("Вопрос не найден");

            int currentStatus = question.Status;
            int newStatus = statusChangeDto.NewStatus;

            if (currentStatus == newStatus)
            {
                throw new InvalidOperationException("Вопрос уже в этом статусе");
            }

            if (!IsValidTransition(currentStatus, newStatus))
            {
                throw new InvalidOperationException("Недопустимый переход статуса");
            }

            var transition = new QuestionStatusTransition
            {
                QuestionId = question.Id,
                FromStatus = currentStatus,
                ToStatus = newStatus,
                ChangedByUserId = statusChangeDto.ChangedByUserId,
                Created = DateTimeOffset.UtcNow,
            };

            await dataContext.QuestionStatusTransitions.AddAsync(transition);

            question.Status = newStatus;
            question.Updated = DateTimeOffset.UtcNow;

            if (newStatus == (int)QuestionStatus.Answered)
            {
                question.Answered = DateTimeOffset.UtcNow;
            }
            else if (currentStatus == (int)QuestionStatus.Answered)
            {
                question.Answered = null;
            }

            await dataContext.SaveChangesAsync();
        }

        private static bool IsValidTransition(int from, int to)
        {
            return Math.Abs(from - to) == 1;
        }

        public async Task SetCommentAsync(QuestionCommentDto commentDto)
        {
            Question? question = await dataContext.Questions
                .FirstOrDefaultAsync(q => q.Id == commentDto.QuestionId)
                ?? throw new InvalidOperationException("Вопрос не найден");

            question.Comment = htmlSanitizer.Sanitize(commentDto.Comment);
            question.Updated = DateTimeOffset.UtcNow;

            await dataContext.SaveChangesAsync();
        }
    }
}
