using AskQuestion.BLL.DTO.Speaker;
using AskQuestion.BLL.Repositories.Interfaces;
using AskQuestion.Core.Constants;
using AskQuestion.WebApi.Models.Request.Speaker;
using AskQuestion.WebApi.Models.Response.Speaker;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AskQuestion.WebApi.Controllers
{
    [Route("api/Speaker")]
    [ApiController]
    public class SpeakerController : ControllerBase
    {
        private readonly ISpeakerRepository _speakerRepository;

        public SpeakerController(ISpeakerRepository speakerRepository)
        {
            _speakerRepository = speakerRepository ?? throw new ArgumentNullException(nameof(speakerRepository));
        }

        [HttpGet("GetAllPublic")]
        public async Task<ActionResult<IEnumerable<SpeakerPublicViewModel>>> GetAllPublic()
        {
            var speakerDtos = await _speakerRepository.GetAllAsync();

            IEnumerable<SpeakerPublicViewModel> result = speakerDtos.Select(dto => new SpeakerPublicViewModel
            {
                Id = dto.Id,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
            });

            return Ok(result);
        }

        [HttpGet("GetAll")]
        [Authorize(Roles = UserStringRoles.ADMINISTRATORS_ONLY)]
        public async Task<ActionResult<IEnumerable<SpeakerViewModel>>> GetAll()
        {
            var speakerDtos = await _speakerRepository.GetAllAsync();

            IEnumerable<SpeakerViewModel> result = speakerDtos.Select(dto => new SpeakerViewModel
            {
                Id = dto.Id,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Patronymic = dto.Patronymic,
                Position = dto.Position,
                Email = dto.Email,
                AdditionalInfo = dto.AdditionalInfo,

            });

            return Ok(result);
        }

        [HttpGet("GetById/{id}")]
        [Authorize(Roles = UserStringRoles.ADMINISTRATORS_ONLY)]
        public async Task<ActionResult<SpeakerViewModel>> GetById(Guid id)
        {
            SpeakerDto? speakerDto = await _speakerRepository.GetByIdAsync(id);

            if (speakerDto == null)
            {
                return NotFound("Спикер не найден");
            }

            SpeakerViewModel result = new()
            {
                Id = speakerDto.Id,
                FirstName = speakerDto.FirstName,
                LastName = speakerDto.LastName,
                Patronymic = speakerDto.Patronymic,
                Position = speakerDto.Position,
                Email = speakerDto.Email,
                AdditionalInfo = speakerDto.AdditionalInfo,

            };

            return Ok(result);
        }

        [HttpPost("Create")]
        [Authorize(Roles = UserStringRoles.ADMINISTRATORS_ONLY)]
        public async Task<ActionResult<SpeakerCreatedViewModel>> Create(SpeakerCreateModel speakerCreateModel)
        {
            SpeakerCreateDto speakerCreateDto = new()
            {
                FirstName = speakerCreateModel.FirstName,
                LastName = speakerCreateModel.LastName,
                Patronymic = speakerCreateModel.Patronymic,
                Position = speakerCreateModel.Position,
                Email = speakerCreateModel.Email,
            };

            SpeakerCreatedDto speakerCreatedDto = await _speakerRepository.CreateAsync(speakerCreateDto);

            SpeakerCreatedViewModel result = new()
            {
                Id = speakerCreatedDto.Id,
                FirstName = speakerCreatedDto.FirstName,
                LastName = speakerCreatedDto.LastName,
                Patronymic = speakerCreatedDto.Patronymic,
                Position = speakerCreatedDto.Position,
                Email = speakerCreatedDto.Email,
                AdditionalInfo = speakerCreatedDto.AdditionalInfo,

                GeneratedPassword = speakerCreatedDto.GeneratedPassword,
            };

            return Ok(result);
        }

        [HttpPut("Update")]
        [Authorize(Roles = UserStringRoles.ADMINISTRATORS_ONLY)]
        public async Task<IActionResult> Update(SpeakerUpdateModel speakerUpdateModel)
        {
            SpeakerUpdateDto speakerUpdateDto = new()
            {
                Id = speakerUpdateModel.Id,
                FirstName = speakerUpdateModel.FirstName,
                LastName = speakerUpdateModel.LastName,
                Patronymic = speakerUpdateModel.Patronymic,
                Position = speakerUpdateModel.Position,
                Email = speakerUpdateModel.Email,
                AdditionalInfo = speakerUpdateModel.AdditionalInfo,
            };

            await _speakerRepository.UpdateAsync(speakerUpdateDto);

            return Ok();
        }

        [HttpDelete("Delete/{id}")]
        [Authorize(Roles = UserStringRoles.ADMINISTRATORS_ONLY)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _speakerRepository.DeleteAsync(id);

            return Ok();
        }
    }
}
