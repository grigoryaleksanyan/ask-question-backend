using AskQuestion.BLL.DTO.FaqEntry;
using AskQuestion.BLL.Repositories.Interfaces;
using AskQuestion.DAL;
using AskQuestion.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace AskQuestion.BLL.Repositories.Implementations
{
    public class FaqEntryRepository : IFaqEntryRepository
    {
        private readonly DataContext _dataContext;

        public FaqEntryRepository(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<IEnumerable<FaqEntryDto>> GetAllAsync()
        {
            IEnumerable<FaqEntryDto> faqEntryDtos = await _dataContext.FaqEntries
                .AsNoTracking()
                .OrderBy(c => c.Order)
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
            FaqEntry? faqEntry = await _dataContext.FaqEntries
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.Id == id);

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

            await _dataContext.AddAsync(faqEntry);
            await _dataContext.SaveChangesAsync();

            return faqEntry.Id;
        }

        public async Task UpdateAsync(FaqEntryUpdateDto faqEntryUpdateDto)
        {
            FaqEntry? faqEntry = await _dataContext.FaqEntries
                .FirstOrDefaultAsync(q => q.Id == faqEntryUpdateDto.Id);

            if (faqEntry == default)
            {
                throw new InvalidOperationException("Объект не найден");
            }

            faqEntry.Question = faqEntryUpdateDto.Question;
            faqEntry.Answer = faqEntryUpdateDto.Answer;
            faqEntry.Updated = DateTimeOffset.UtcNow;

            await _dataContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            FaqEntry? faqEntry = await _dataContext.FaqEntries
                .FirstOrDefaultAsync(q => q.Id == id);

            if (faqEntry == default)
            {
                throw new InvalidOperationException("Объект не найден");
            }

            _dataContext.Remove(faqEntry);
            await _dataContext.SaveChangesAsync();
        }

        public async Task SetOrderAsync(Guid[] ids)
        {
            var entries = await _dataContext.FaqEntries.ToListAsync();

            for (int i = 0; i < ids.Length; i++)
            {
                var entry = entries.FirstOrDefault(c => c.Id == ids[i]);

                if (entry == null)
                {
                    continue;
                }

                entry.Order = i;
            }

            await _dataContext.SaveChangesAsync();
        }
    }
}
