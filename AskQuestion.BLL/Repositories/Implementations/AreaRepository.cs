using AskQuestion.BLL.DTO.Area;
using AskQuestion.BLL.Repositories.Interfaces;
using AskQuestion.DAL;
using AskQuestion.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace AskQuestion.BLL.Repositories.Implementations
{
    public class AreaRepository(DataContext dataContext) : IAreaRepository
    {
        public async Task<IEnumerable<AreaDto>> GetAllAsync()
        {
            IEnumerable<AreaDto> areaDtos = await dataContext.Areas
                .AsNoTracking()
                .OrderBy(area => area.Order)
                .Select(area => new AreaDto
                {
                    Id = area.Id,
                    Title = area.Title,
                    Order = area.Order,
                    Сreated = area.Сreated
                })
                .ToListAsync();

            return areaDtos;
        }

        public async Task<Guid> CreateAsync(AreaCreateDto areaCreateDto)
        {
            Area area = new()
            {
                Title = areaCreateDto.Title,
                Order = areaCreateDto.Order,
                Сreated = DateTimeOffset.UtcNow,
            };

            await dataContext.AddAsync(area);
            await dataContext.SaveChangesAsync();

            return area.Id;
        }

        public async Task UpdateAsync(AreaUpdateDto areaUpdateDto)
        {
            Area? area = await dataContext.Areas
                .FirstOrDefaultAsync(q => q.Id == areaUpdateDto.Id);

            if (area == default)
            {
                throw new InvalidOperationException("Объект не найден");
            }

            area.Title = areaUpdateDto.Title;
            area.Updated = DateTimeOffset.UtcNow;

            await dataContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            Area? area = await dataContext.Areas
                .FirstOrDefaultAsync(q => q.Id == id);

            if (area == default)
            {
                throw new InvalidOperationException("Объект не найден");
            }

            dataContext.Remove(area);
            await dataContext.SaveChangesAsync();
        }

        public async Task SetOrderAsync(Guid[] ids)
        {
            var areas = await dataContext.Areas.ToListAsync();

            for (int i = 0; i < ids.Length; i++)
            {
                var area = areas.Find(c => c.Id == ids[i]);

                if (area == null)
                {
                    continue;
                }

                area.Order = i;
            }

            await dataContext.SaveChangesAsync();
        }
    }
}
