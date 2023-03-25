using AskQuestion.BLL.DTO.Area;
using AskQuestion.BLL.Repositories.Interfaces;
using AskQuestion.DAL;
using AskQuestion.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace AskQuestion.BLL.Repositories.Implementations
{
    public class AreaRepository : IAreaRepository
    {
        private readonly DataContext _dataContext;

        public AreaRepository(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<IEnumerable<AreaDto>> GetAllAsync()
        {
            IEnumerable<AreaDto> areaDtos = await _dataContext.Areas
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

            await _dataContext.AddAsync(area);
            await _dataContext.SaveChangesAsync();

            return area.Id;
        }

        public async Task UpdateAsync(AreaUpdateDto areaUpdateDto)
        {
            Area? area = await _dataContext.Areas
                .FirstOrDefaultAsync(q => q.Id == areaUpdateDto.Id);

            if (area == default)
            {
                throw new InvalidOperationException("Объект не найден");
            }

            area.Title = areaUpdateDto.Title;
            area.Updated = DateTimeOffset.UtcNow;

            await _dataContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            Area? area = await _dataContext.Areas
                .FirstOrDefaultAsync(q => q.Id == id);

            if (area == default)
            {
                throw new InvalidOperationException("Объект не найден");
            }

            _dataContext.Remove(area);
            await _dataContext.SaveChangesAsync();
        }

        public async Task SetOrderAsync(Guid[] ids)
        {
            var areas = await _dataContext.Areas.ToListAsync();

            for (int i = 0; i < ids.Length; i++)
            {
                var area = areas.FirstOrDefault(c => c.Id == ids[i]);

                if (area == null)
                {
                    continue;
                }

                area.Order = i;
            }

            await _dataContext.SaveChangesAsync();
        }
    }
}
