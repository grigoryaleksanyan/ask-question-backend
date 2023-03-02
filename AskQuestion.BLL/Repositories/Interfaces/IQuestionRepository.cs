using AskQuestion.BLL.DTO.Question;

namespace AskQuestion.BLL.Repositories
{
    public interface IQuestionRepository
    {
        Task<IEnumerable<QuestionDto>> GetAllAsync();

        Task<QuestionDto?> GetByIdAsync(Guid id);

        Task<Guid> CreateAsync(QuestionCreateDto questionCreateDto);

        Task UpdateAsync(Guid id, QuestionUpdateDto questionUpdateDto);

        Task DeleteAsync(Guid id);
    }
}
