using AskQuestion.BLL.DTO.Speaker;

namespace AskQuestion.BLL.Repositories.Interfaces
{
    public interface ISpeakerRepository
    {
        Task<IEnumerable<SpeakerDto>> GetAllAsync();
        Task<SpeakerDto?> GetByIdAsync(Guid id);
        Task<SpeakerCreatedDto> CreateAsync(SpeakerCreateDto speakerCreateDto);
        Task UpdateAsync(SpeakerUpdateDto speakerUpdateDto);
        Task DeleteAsync(Guid id);
    }
}
