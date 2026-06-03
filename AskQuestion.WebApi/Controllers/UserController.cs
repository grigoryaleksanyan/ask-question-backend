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
    [Route("api/User")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

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
                Email = userDto.Email,
                UserRoleId = userDto.UserRoleId,
                UserDetails = userDto.UserDetails is not null ? new UserDetailsViewModel()
                {
                    Id = userDto.UserDetails.Id,
                    FirstName = userDto.UserDetails.FirstName,
                    LastName = userDto.UserDetails.LastName,
                    Patronymic = userDto.UserDetails.Patronymic,
                    Position = userDto.UserDetails.Position,

                    AdditionalInfo = userDto.UserDetails.AdditionalInfo,
                    IsDeleted = userDto.UserDetails.IsDeleted,
                } : null,
                Created = userDto.Created,
                Updated = userDto.Updated,
            };

            return Ok(userViewModel);
        }

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
