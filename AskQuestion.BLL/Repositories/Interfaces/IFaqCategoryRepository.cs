using AskQuestion.BLL.DTO.FaqCategory;

namespace AskQuestion.BLL.Repositories.Interfaces
{
    public interface IFaqCategoryRepository
    {
        Task<IEnumerable<FaqCategoryDto>> GetAllAsync();
        Task<IEnumerable<FaqCategoryDto>> GetAllWithEntriesAsync();

        Task<FaqCategoryDto?> GetByIdAsync(Guid id);

        Task<Guid> CreateAsync(FaqCategoryCreateDto faqCategoryCreateDto);

        Task UpdateAsync(FaqCategoryUpdateDto faqCategoryUpdateDto);

        Task DeleteAsync(Guid id);

        Task SetOrderAsync(Guid[] ids);
    }
}
