using AskQuestion.BLL.DTO;
using AskQuestion.BLL.DTO.Question;

namespace AskQuestion.BLL.Repositories
{
    public interface IQuestionRepository
    {
        Task<PaginatedResult<QuestionDto>> GetAllAsync(
            int page = 1,
            int pageSize = 10,
            int? status = null,
            string? speaker = null,
            string? area = null,
            string? search = null,
            string sortOrder = "desc");

        Task<IEnumerable<QuestionDto>> GetPopularQuestionsAsync();

        Task<QuestionDto?> GetByIdAsync(Guid id);

        Task<Guid> CreateAsync(QuestionCreateDto questionCreateDto);

        Task UpdateAsync(Guid id, QuestionUpdateDto questionUpdateDto);

        Task DeleteAsync(Guid id);
    }
}
