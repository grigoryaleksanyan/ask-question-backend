using AskQuestion.BLL.DTO.Area;

namespace AskQuestion.BLL.Repositories.Interfaces
{
    public interface IAreaRepository
    {
        Task<IEnumerable<AreaDto>> GetAllAsync();

        Task<Guid> CreateAsync(AreaCreateDto areaCreateDto);

        Task UpdateAsync(AreaUpdateDto areaUpdateDto);

        Task DeleteAsync(Guid id);

        Task SetOrderAsync(Guid[] ids);
    }
}
