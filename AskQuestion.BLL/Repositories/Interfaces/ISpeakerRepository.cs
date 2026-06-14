using AskQuestion.BLL.DTO.Speaker;

namespace AskQuestion.BLL.Repositories.Interfaces
{
    public interface ISpeakerRepository
    {
        Task<IEnumerable<SpeakerDto>> GetAllAsync();
        Task<SpeakerDto?> GetByIdAsync(Guid id);
        Task<SpeakerCreatedDto> CreateAsync(SpeakerCreateDto speakerCreateDto);
        Task<SpeakerDto> UpdateAsync(SpeakerUpdateDto speakerUpdateDto);
        Task SetActiveAsync(Guid id, bool isActive);
        Task<IEnumerable<SpeakerDto>> GetAllPublicAsync();
        Task SetOrderAsync(Guid[] ids);
    }
}
