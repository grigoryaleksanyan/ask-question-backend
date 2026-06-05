using AskQuestion.BLL.DTO.Area;
using AskQuestion.BLL.Helpers;
using AskQuestion.BLL.Repositories.Interfaces;
using AskQuestion.DAL;
using AskQuestion.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace AskQuestion.BLL.Repositories.Implementations
{
    public class AreaRepository(
        DataContext dataContext,
        IHtmlSanitizerService htmlSanitizer) : IAreaRepository
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
                    Created = area.Created
                })
                .ToListAsync();

            return areaDtos;
        }

        public async Task<AreaDto> CreateAsync(AreaCreateDto areaCreateDto)
        {
            Area area = new()
            {
                Title = htmlSanitizer.Sanitize(areaCreateDto.Title),
                Order = areaCreateDto.Order,
                Created = DateTimeOffset.UtcNow,
            };

            await dataContext.AddAsync(area);
            await dataContext.SaveChangesAsync();

            return new AreaDto
            {
                Id = area.Id,
                Title = area.Title,
                Order = area.Order,
                Created = area.Created,
                Updated = area.Updated,
            };
        }

        public async Task<AreaDto> UpdateAsync(AreaUpdateDto areaUpdateDto)
        {
            Area? area = await dataContext.Areas
                .FirstOrDefaultAsync(q => q.Id == areaUpdateDto.Id);

            if (area == default)
            {
                throw new InvalidOperationException("Объект не найден");
            }

            area.Title = htmlSanitizer.Sanitize(areaUpdateDto.Title);
            area.Updated = DateTimeOffset.UtcNow;

            await dataContext.SaveChangesAsync();

            return new AreaDto
            {
                Id = area.Id,
                Title = area.Title,
                Order = area.Order,
                Created = area.Created,
                Updated = area.Updated,
            };
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
