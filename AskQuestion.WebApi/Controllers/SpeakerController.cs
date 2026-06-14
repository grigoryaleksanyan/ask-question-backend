using AskQuestion.BLL.DTO.Speaker;
using AskQuestion.BLL.Repositories.Interfaces;
using AskQuestion.Core.Constants;
using AskQuestion.WebApi.Models.Request.Speaker;
using AskQuestion.WebApi.Models.Request.User;
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
            var speakerDtos = await _speakerRepository.GetAllPublicAsync();

            IEnumerable<SpeakerPublicViewModel> result = speakerDtos.Select(dto => new SpeakerPublicViewModel
            {
                Id = dto.Id,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Position = dto.Position,
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
                Position = dto.Position,
                Email = dto.Email,
                Order = dto.Order,
                IsActive = dto.IsActive,
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
                Position = speakerDto.Position,
                Email = speakerDto.Email,
                Order = speakerDto.Order,
                IsActive = speakerDto.IsActive,
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
                Position = speakerCreateModel.Position,
                Email = speakerCreateModel.Email,
                Order = speakerCreateModel.Order,
            };

            SpeakerCreatedDto speakerCreatedDto = await _speakerRepository.CreateAsync(speakerCreateDto);

            SpeakerCreatedViewModel result = new()
            {
                Id = speakerCreatedDto.Id,
                FirstName = speakerCreatedDto.FirstName,
                LastName = speakerCreatedDto.LastName,
                Position = speakerCreatedDto.Position,
                Email = speakerCreatedDto.Email,
                Order = speakerCreatedDto.Order,
                GeneratedPassword = speakerCreatedDto.GeneratedPassword,
            };

            return Ok(result);
        }

        [HttpPut("Update")]
        [Authorize(Roles = UserStringRoles.ADMINISTRATORS_ONLY)]
        public async Task<ActionResult<SpeakerViewModel>> Update(SpeakerUpdateModel speakerUpdateModel)
        {
            SpeakerUpdateDto speakerUpdateDto = new()
            {
                Id = speakerUpdateModel.Id,
                FirstName = speakerUpdateModel.FirstName,
                LastName = speakerUpdateModel.LastName,
                Position = speakerUpdateModel.Position,
                Email = speakerUpdateModel.Email,
            };

            var speakerDto = await _speakerRepository.UpdateAsync(speakerUpdateDto);

            SpeakerViewModel result = new()
            {
                Id = speakerDto.Id,
                FirstName = speakerDto.FirstName,
                LastName = speakerDto.LastName,
                Position = speakerDto.Position,
                Email = speakerDto.Email,
                Order = speakerDto.Order,
                IsActive = speakerDto.IsActive,
            };

            return Ok(result);
        }

        [HttpPut("SetActive")]
        [Authorize(Roles = UserStringRoles.ADMINISTRATORS_ONLY)]
        public async Task<IActionResult> SetActive(UserSetActiveModel model)
        {
            try
            {
                await _speakerRepository.SetActiveAsync(model.Id, model.IsActive);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();
        }

        [HttpPut("SetOrder")]
        [Authorize(Roles = UserStringRoles.ADMINISTRATORS_ONLY)]
        public async Task<IActionResult> SetOrder(Guid[] ids)
        {
            if (ids.Length == 0)
            {
                return BadRequest();
            }

            await _speakerRepository.SetOrderAsync(ids);

            return Ok();
        }
    }
}
