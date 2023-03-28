using AskQuestion.BLL.DTO.User;
using AskQuestion.BLL.Repositories.Interfaces;
using AskQuestion.Core.Constants;
using AskQuestion.WebApi.Models.Request.User;
using AskQuestion.WebApi.Models.Response.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration, IUserRepository userRepository)
        {
            _configuration = configuration;
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

            string token = CreateToken(userDto);

            if (token != null)
            {
                HttpContext.Response.Cookies.Append(".WebApi", token,
                new CookieOptions
                {
                    MaxAge = TimeSpan.FromMinutes(60)
                });
            }

            HttpContext.Response.Headers.Add("X-Content-Type-Options", "nosniff");
            HttpContext.Response.Headers.Add("X-Xss-Protection", "1");
            HttpContext.Response.Headers.Add("X-Frame-Options", "DENY");

            return Ok(userViewModel);
        }

        /// <summary>
        /// Выйти из портала.
        /// </summary>
        /// <response code='200'>Успешный выход.</response>
        [HttpPut("Logout")]
        [Authorize(Roles = UserStringRoles.ADMINISTRATORS_AND_SPEAKERS)]
        public IActionResult Logout()
        {
            HttpContext.Response.Cookies.Delete(".WebApi");

            return Ok();
        }

        private string CreateToken(UserDto userDto)
        {
            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.Name, userDto.Login),
                new Claim(ClaimTypes.NameIdentifier, userDto.Id.ToString()),
                new Claim(ClaimTypes.Role, userDto.UserRoleId.ToString()),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("Jwt:Token").Value!));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddHours(1),
                    signingCredentials: credentials
                );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
    }
}
