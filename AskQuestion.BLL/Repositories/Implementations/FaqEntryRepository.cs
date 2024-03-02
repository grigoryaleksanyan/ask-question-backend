using AskQuestion.BLL.DTO.FaqEntry;
using AskQuestion.BLL.Repositories.Interfaces;
using AskQuestion.DAL;
using AskQuestion.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace AskQuestion.BLL.Repositories.Implementations
{
    public class FaqEntryRepository(DataContext dataContext) : IFaqEntryRepository
    {
        public async Task<IEnumerable<FaqEntryDto>> GetAllAsync()
        {
            IEnumerable<FaqEntryDto> faqEntryDtos = await dataContext.FaqEntries
                .AsNoTracking()
                .OrderBy(entry => entry.Order)
                .Select(faqEntry => new FaqEntryDto
                {
                    Id = faqEntry.Id,
                    Question = faqEntry.Question,
                    Answer = faqEntry.Answer,
                    Order = faqEntry.Order,
                    Сreated = faqEntry.Сreated
                })
                .ToListAsync();

            return faqEntryDtos;
        }

        public async Task<FaqEntryDto?> GetByIdAsync(Guid id)
        {
            FaqEntry? faqEntry = await dataContext.FaqEntries
                .AsNoTracking()
                .FirstOrDefaultAsync(entry => entry.Id == id);

            if (faqEntry == default)
            {
                return null;
            }

            FaqEntryDto faqEntryDto = new()
            {
                Id = faqEntry.Id,
                Question = faqEntry.Question,
                Answer = faqEntry.Answer,
                Сreated = faqEntry.Сreated
            };

            return faqEntryDto;
        }

        public async Task<Guid> CreateAsync(FaqEntryCreateDto faqEntryCreateDto)
        {
            FaqEntry faqEntry = new()
            {
                FaqCategoryId = faqEntryCreateDto.FaqCategoryId,
                Question = faqEntryCreateDto.Question,
                Answer = faqEntryCreateDto.Answer,
                Order = faqEntryCreateDto.Order,
                Сreated = DateTimeOffset.UtcNow,
            };

            await dataContext.AddAsync(faqEntry);
            await dataContext.SaveChangesAsync();

            return faqEntry.Id;
        }

        public async Task UpdateAsync(FaqEntryUpdateDto faqEntryUpdateDto)
        {
            FaqEntry? faqEntry = await dataContext.FaqEntries
                .FirstOrDefaultAsync(entry => entry.Id == faqEntryUpdateDto.Id);

            if (faqEntry == default)
            {
                throw new InvalidOperationException("Объект не найден");
            }

            faqEntry.Question = faqEntryUpdateDto.Question;
            faqEntry.Answer = faqEntryUpdateDto.Answer;
            faqEntry.Updated = DateTimeOffset.UtcNow;

            await dataContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            FaqEntry? faqEntry = await dataContext.FaqEntries
                .FirstOrDefaultAsync(entry => entry.Id == id);

            if (faqEntry == default)
            {
                throw new InvalidOperationException("Объект не найден");
            }

            dataContext.Remove(faqEntry);
            await dataContext.SaveChangesAsync();
        }

        public async Task SetOrderAsync(Guid[] ids)
        {
            var entries = await dataContext.FaqEntries.ToListAsync();

            for (int i = 0; i < ids.Length; i++)
            {
                var entry = entries.Find(entry => entry.Id == ids[i]);

                if (entry == null)
                {
                    continue;
                }

                entry.Order = i;
            }

            await dataContext.SaveChangesAsync();
        }
    }
}
