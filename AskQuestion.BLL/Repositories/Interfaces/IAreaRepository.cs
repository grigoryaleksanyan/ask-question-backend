using AskQuestion.BLL.DTO.Area;

namespace AskQuestion.BLL.Repositories.Interfaces
{
    public interface IAreaRepository
    {
        Task<IEnumerable<AreaDto>> GetAllAsync();

        Task<AreaDto> CreateAsync(AreaCreateDto areaCreateDto);

        Task<AreaDto> UpdateAsync(AreaUpdateDto areaUpdateDto);

        Task DeleteAsync(Guid id);

        Task SetOrderAsync(Guid[] ids);
    }
}
