using AskQuestion.BLL.DTO;
using AskQuestion.BLL.DTO.Question;
using AskQuestion.Core.Enums;

namespace AskQuestion.BLL.Repositories
{
    public interface IQuestionRepository
    {
        Task<PaginatedResult<QuestionDto>> GetAllAsync(
            int page = 1,
            int pageSize = 10,
            int? status = null,
            Guid? speakerId = null,
            string? area = null,
            string? search = null,
            string sortOrder = "desc");

        Task<IEnumerable<QuestionDto>> GetPopularQuestionsAsync();

        Task<QuestionDto?> GetByIdAsync(Guid id);

        Task<Guid> CreateAsync(QuestionCreateDto questionCreateDto);

        Task UpdateAsync(Guid id, QuestionUpdateDto questionUpdateDto);

        Task DeleteAsync(Guid id);

        Task<VoteResultDto> ToggleLikeAsync(Guid questionId, Guid visitorId);

        Task<VoteResultDto> ToggleDislikeAsync(Guid questionId, Guid visitorId);

        Task IncrementViewsAsync(Guid questionId);

        Task<VoteType?> GetUserVoteAsync(Guid questionId, Guid visitorId);
    }
}
