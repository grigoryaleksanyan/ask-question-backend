using AskQuestion.BLL.DTO.FaqCategory;
using AskQuestion.BLL.DTO.FaqEntry;
using AskQuestion.BLL.Repositories.Interfaces;
using AskQuestion.DAL;
using AskQuestion.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace AskQuestion.BLL.Repositories.Implementations
{
    public class FaqCategoryRepository(DataContext dataContext) : IFaqCategoryRepository
    {
        public async Task<IEnumerable<FaqCategoryDto>> GetAllAsync()
        {
            IEnumerable<FaqCategoryDto> faqCategoryDtos = await dataContext.FaqCategories
                .AsNoTracking()
                .OrderBy(faqCategory => faqCategory.Order)
                .Select(faqCategory => new FaqCategoryDto
                {
                    Id = faqCategory.Id,
                    Name = faqCategory.Name,
                    Order = faqCategory.Order,
                    Сreated = faqCategory.Сreated
                })
                .ToListAsync();

            return faqCategoryDtos;
        }

        public async Task<IEnumerable<FaqCategoryDto>> GetAllWithEntriesAsync()
        {
            IEnumerable<FaqCategoryDto> faqCategoryDtos = await dataContext.FaqCategories
                .AsNoTracking()
                .Include(faqCategory => faqCategory.FaqEntries)
                .Where(faqCategory => faqCategory.FaqEntries.Any())
                .OrderBy(faqCategory => faqCategory.Order)
                .Select(faqCategory => new FaqCategoryDto
                {
                    Id = faqCategory.Id,
                    Name = faqCategory.Name,
                    Order = faqCategory.Order,
                    Сreated = faqCategory.Сreated,
                    Updated = faqCategory.Updated,
                    Entries = faqCategory.FaqEntries.Select(entry => new FaqEntryDto()
                    {
                        Id = entry.Id,
                        Question = entry.Question,
                        Answer = entry.Answer,
                        Order = entry.Order,
                        Сreated = entry.Сreated,
                        Updated = entry.Updated
                    })
                    .OrderBy(entry => entry.Order)
                    .ToList(),
                })
                .ToListAsync();

            return faqCategoryDtos;
        }

        public async Task<FaqCategoryDto?> GetByIdAsync(Guid id)
        {
            FaqCategory? faqCategory = await dataContext.FaqCategories
                .AsNoTracking()
                .Include(faqCategory => faqCategory.FaqEntries.OrderBy(entry => entry.Order))
                .FirstOrDefaultAsync(faqCategory => faqCategory.Id == id);

            if (faqCategory == default)
            {
                return null;
            }

            FaqCategoryDto faqCategoryDto = new()
            {
                Id = faqCategory.Id,
                Name = faqCategory.Name,
                Order = faqCategory.Order,
                Сreated = faqCategory.Сreated,
                Updated = faqCategory.Updated,
                Entries = faqCategory.FaqEntries.Select(entry => new FaqEntryDto()
                {
                    Id = entry.Id,
                    Question = entry.Question,
                    Answer = entry.Answer,
                    Order = entry.Order,
                    Сreated = entry.Сreated,
                    Updated = entry.Updated
                }),
            };

            return faqCategoryDto;
        }

        public async Task<Guid> CreateAsync(FaqCategoryCreateDto faqCategoryCreateDto)
        {
            FaqCategory faqCategory = new()
            {
                Name = faqCategoryCreateDto.Name,
                Order = faqCategoryCreateDto.Order,
                Сreated = DateTimeOffset.UtcNow,
            };

            await dataContext.AddAsync(faqCategory);
            await dataContext.SaveChangesAsync();

            return faqCategory.Id;
        }

        public async Task UpdateAsync(FaqCategoryUpdateDto faqCategoryUpdateDto)
        {
            FaqCategory? faqCategory = await dataContext.FaqCategories
                .FirstOrDefaultAsync(q => q.Id == faqCategoryUpdateDto.Id);

            if (faqCategory == default)
            {
                throw new InvalidOperationException("Объект не найден");
            }

            faqCategory.Name = faqCategoryUpdateDto.Name;
            faqCategory.Updated = DateTimeOffset.UtcNow;

            await dataContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            FaqCategory? faqCategory = await dataContext.FaqCategories
                .FirstOrDefaultAsync(q => q.Id == id);

            if (faqCategory == default)
            {
                throw new InvalidOperationException("Объект не найден");
            }

            dataContext.Remove(faqCategory);
            await dataContext.SaveChangesAsync();
        }

        public async Task SetOrderAsync(Guid[] ids)
        {
            var categories = await dataContext.FaqCategories.ToListAsync();

            for (int i = 0; i < ids.Length; i++)
            {
                var category = categories.Find(c => c.Id == ids[i]);

                if (category == null)
                {
                    continue;
                }

                category.Order = i;
            }

            await dataContext.SaveChangesAsync();
        }
    }
}
