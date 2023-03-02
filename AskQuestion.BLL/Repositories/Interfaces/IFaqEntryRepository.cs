using AskQuestion.BLL.DTO.FaqEntry;

namespace AskQuestion.BLL.Repositories.Interfaces
{
    public interface IFaqEntryRepository
    {
        Task<IEnumerable<FaqEntryDto>> GetAllAsync();

        Task<FaqEntryDto?> GetByIdAsync(Guid id);

        Task<Guid> CreateAsync(FaqEntryCreateDto faqEntryCreateDto);

        Task UpdateAsync(FaqEntryUpdateDto faqEntryUpdateDto);

        Task DeleteAsync(Guid id);

        Task SetOrderAsync(Guid[] ids);
    }
}
