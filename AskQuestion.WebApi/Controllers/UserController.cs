using AskQuestion.BLL.DTO.User;
using AskQuestion.BLL.Repositories.Interfaces;
using AskQuestion.Core.Constants;
using AskQuestion.WebApi.Models.Request.User;
using AskQuestion.WebApi.Models.Response.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AskQuestion.WebApi.Controllers
{
    /// <summary>
    /// Контроллер пользователя.
    /// </summary>
    [Route("api/User")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        /// <summary>
        /// Получить данные текущего пользователя.
        /// </summary>
        [HttpGet("GetUserData")]
        [Authorize(Roles = UserStringRoles.ADMINISTRATORS_AND_SPEAKERS)]
        public async Task<ActionResult<UserViewModel>> GetUserData()
        {
            var claimsIdentity = HttpContext.User.Identity as ClaimsIdentity;

            if (claimsIdentity == default)
            {
                return BadRequest("Не удалось выполнить идентификацию пользователя");
            }

            string? idString = claimsIdentity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (idString == default)
            {
                return BadRequest("Не удалось выполнить идентификацию пользователя");
            }

            Guid id = Guid.Parse(idString);

            var userDto = await _userRepository.GetById(id);

            if (userDto == default)
            {
                return NotFound("Пользователь не найден");
            }

            UserViewModel userViewModel = new UserViewModel()
            {
                Id = userDto.Id,
                Login = userDto.Login,
                UserRoleId = userDto.UserRoleId,
                UserDetails = userDto.UserDetails is not null ? new UserDetailsViewModel()
                {
                    Id = userDto.UserDetails.Id,
                    FullName = userDto.UserDetails.FullName,
                    Email = userDto.UserDetails.Email,
                    AdditionalInfo = userDto.UserDetails.AdditionalInfo,
                } : null,
                Сreated = userDto.Сreated,
                Updated = userDto.Updated,
            };

            return Ok(userViewModel);
        }

        /// <summary>
        /// Создать спикера.
        /// </summary>
        /// <param name="speakerCreateModel">Модель создания спикера.</param>
        [HttpPost("CreateSpeaker")]
        [Authorize(Roles = UserStringRoles.ADMINISTRATORS_ONLY)]
        public async Task<ActionResult<UserViewModel>> CreateSpeaker(SpeakerCreateModel speakerCreateModel)
        {
            UserCreateDto userCreateDto = new UserCreateDto()
            {
                Login = speakerCreateModel.Login,
                Password = speakerCreateModel.Password,
                UserDetails = new UserDetailsCreateDto()
                {
                    FullName = speakerCreateModel.FullName,
                    Email = speakerCreateModel.Email,
                    AdditionalInfo = speakerCreateModel.AdditionalInfo,
                }
            };

            UserDto userDto = await _userRepository.CreateSpeaker(userCreateDto);

            UserViewModel userViewModel = new()
            {
                Id = userDto.Id,
                Login = userDto.Login,
                UserRoleId = userDto.UserRoleId,
                UserDetails = new UserDetailsViewModel()
                {
                    Id = userDto.UserDetails.Id,
                    FullName = userDto.UserDetails.FullName,
                    Email = userDto.UserDetails.Email,
                    AdditionalInfo = userDto.UserDetails.AdditionalInfo,
                },
                Сreated = userDto.Сreated,
                Updated = userDto.Updated,
            };

            return Ok(userViewModel);
        }

        /// <summary>
        /// Изменить пароль.
        /// </summary>
        /// <param name="userPasswordUpdateModel">Модель изменения пароля.</param>
        [HttpPut("ChangePassword")]
        [Authorize(Roles = UserStringRoles.ADMINISTRATORS_AND_SPEAKERS)]
        public async Task<ActionResult> ChangePassword(UserPasswordUpdateModel userPasswordUpdateModel)
        {
            var claimsIdentity = HttpContext.User.Identity as ClaimsIdentity;

            if (claimsIdentity == default)
            {
                return BadRequest("Не удалось выполнить идентификацию пользователя");
            }

            string? idString = claimsIdentity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (idString == default)
            {
                return BadRequest("Не удалось выполнить идентификацию пользователя");
            }

            Guid id = Guid.Parse(idString);

            UserPasswordUpdateDto userPasswordUpdateDto = new()
            {
                Id = id,
                Password = userPasswordUpdateModel.Password,
                NewPassword = userPasswordUpdateModel.NewPassword,
            };

            await _userRepository.ChangePassword(userPasswordUpdateDto);

            return Ok();
        }
    }
}
