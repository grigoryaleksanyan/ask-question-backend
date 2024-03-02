using AskQuestion.BLL.DTO.User;
using AskQuestion.BLL.Repositories.Interfaces;
using AskQuestion.Core.Constants;
using AskQuestion.WebApi.Models.Request.User;
using AskQuestion.WebApi.Models.Response.User;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AskQuestion.WebApi.Controllers
{
    /// <summary>
    /// Контроллер авторизации.
    /// </summary>
    [Route("api/Auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public AuthController( IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        /// <summary>
        /// Авторизоваться на портале.
        /// </summary>
        /// <param name="userAuthModel">Модель авторизации пользователя.</param>
        /// <response code='200'>Токен авторизации.</response>
        /// <response code='400'>Ошибка авторизации.</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost("Login")]
        public async Task<ActionResult<UserViewModel>> Login(UserAuthModel userAuthModel)
        {

            UserAuthDto userAuthDto = new()
            {
                Login = userAuthModel.Login,
                Password = userAuthModel.Password,
            };

            var userDto = await _userRepository.AuthorizeUser(userAuthDto);

            if (userDto == default)
            {
                return BadRequest("Неверный логин или пароль.");
            }

            UserViewModel userViewModel = new()
            {
                Id = userDto.Id,
                Login = userDto.Login,
                UserRoleId = userDto.UserRoleId,
                Сreated = userDto.Сreated,
                Updated = userDto.Updated,
            };

            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.Name, userDto.Login),
                new Claim(ClaimTypes.NameIdentifier, userDto.Id.ToString()),
                new Claim(ClaimTypes.Role, userDto.UserRoleId.ToString()),
            };


            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                AllowRefresh = true,
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1),
            };

            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, authProperties);

            HttpContext.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            HttpContext.Response.Headers.Append("X-Xss-Protection", "1");
            HttpContext.Response.Headers.Append("X-Frame-Options", "DENY");

            return Ok(userViewModel);
        }

        /// <summary>
        /// Выйти из портала.
        /// </summary>
        /// <response code='200'>Успешный выход.</response>
        [HttpPut("Logout")]
        [Authorize(Roles = UserStringRoles.ADMINISTRATORS_AND_SPEAKERS)]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return Ok();
        }
    }
}
