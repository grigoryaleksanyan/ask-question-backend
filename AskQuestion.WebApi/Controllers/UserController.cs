using AskQuestion.BLL.Repositories.Interfaces;
using AskQuestion.Core.Constants;
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

            UserViewModel userViewModel = new()
            {
                Id = userDto.Id,
                Login = userDto.Login,
                UserRoleId = userDto.UserRoleId,
                Сreated = userDto.Сreated,
                Updated = userDto.Updated,
            };

            return Ok(userViewModel);
        }
    }
}
