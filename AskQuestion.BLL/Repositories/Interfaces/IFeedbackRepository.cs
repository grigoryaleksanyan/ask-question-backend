using AskQuestion.BLL.DTO.Feedback;

namespace AskQuestion.BLL.Repositories.Interfaces
{
    public interface IFeedbackRepository
    {
        Task<IEnumerable<FeedbackDto>> GetAllAsync();

        Task<Guid> CreateAsync(FeedbackCreateDto feedbackCreateDto);

        Task DeleteAsync(Guid id);
    }
}
