using AskQuestion.BLL.DTO.User;
using AskQuestion.BLL.Repositories.Interfaces;
using AskQuestion.Core.Constants;
using AskQuestion.Core.Enums;
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
        /// Проверить, требуется ли первичная настройка администратора.
        /// </summary>
        /// <response code='200'>Флаг необходимости настройки.</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("SetupRequired")]
        public async Task<ActionResult<SetupRequiredResponse>> SetupRequired()
        {
            bool setupRequired = !await _userRepository.IsAdminExistsAsync();

            SetupRequiredResponse response = new() { SetupRequired = setupRequired };

            return Ok(response);
        }

        /// <summary>
        /// Создать администратора при первичной настройке.
        /// </summary>
        /// <param name="adminSetupModel">Модель создания администратора.</param>
        /// <response code='200'>Данные созданного администратора.</response>
        /// <response code='400'>Ошибка создания администратора.</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost("Setup")]
        public async Task<ActionResult<UserViewModel>> Setup(AdminSetupModel adminSetupModel)
        {
            bool adminExists = await _userRepository.IsAdminExistsAsync();

            if (adminExists)
            {
                return BadRequest("Администратор уже существует.");
            }

            AdminSetupDto adminSetupDto = new()
            {
                Email = adminSetupModel.Email,
                Password = adminSetupModel.Password,
                FirstName = adminSetupModel.FirstName,
                LastName = adminSetupModel.LastName,
                Patronymic = adminSetupModel.Patronymic,
            };

            UserDto userDto;

            try
            {
                userDto = await _userRepository.SetupAdminAsync(adminSetupDto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }

            UserViewModel userViewModel = new()
            {
                Id = userDto.Id,
                Email = userDto.Email,
                UserRoleId = userDto.UserRoleId,
                IsActive = userDto.IsActive,
                UserDetails = userDto.UserDetails is not null ? new UserDetailsViewModel()
                {
                    Id = userDto.UserDetails.Id,
                    FirstName = userDto.UserDetails.FirstName,
                    LastName = userDto.UserDetails.LastName,
                    Patronymic = userDto.UserDetails.Patronymic,
                    Position = userDto.UserDetails.Position,
                    AdditionalInfo = userDto.UserDetails.AdditionalInfo,
                    Created = userDto.UserDetails.Created,
                    Updated = userDto.UserDetails.Updated,
                } : null,
                Created = userDto.Created,
                Updated = userDto.Updated,
            };

            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.Name, userDto.Email),
                new Claim(ClaimTypes.NameIdentifier, userDto.Id.ToString()),
                new Claim(ClaimTypes.Role, ((UserRoles)userDto.UserRoleId).ToString()),
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
                Email = userAuthModel.Email,
                Password = userAuthModel.Password,
            };

            var userDto = await _userRepository.AuthorizeUser(userAuthDto);

            if (userDto == default)
            {
                return BadRequest("Неверный email или пароль.");
            }

            UserViewModel userViewModel = new()
            {
                Id = userDto.Id,
                Email = userDto.Email,
                UserRoleId = userDto.UserRoleId,
                IsActive = userDto.IsActive,
                UserDetails = userDto.UserDetails is not null ? new UserDetailsViewModel()
                {
                    Id = userDto.UserDetails.Id,
                    FirstName = userDto.UserDetails.FirstName,
                    LastName = userDto.UserDetails.LastName,
                    Patronymic = userDto.UserDetails.Patronymic,
                    Position = userDto.UserDetails.Position,
                    AdditionalInfo = userDto.UserDetails.AdditionalInfo,
                    Created = userDto.UserDetails.Created,
                    Updated = userDto.UserDetails.Updated,
                } : null,
                Created = userDto.Created,
                Updated = userDto.Updated,
            };

            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.Name, userDto.Email),
                new Claim(ClaimTypes.NameIdentifier, userDto.Id.ToString()),
                new Claim(ClaimTypes.Role, ((UserRoles)userDto.UserRoleId).ToString()),
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

        /// <summary>
        /// Запросить ссылку для сброса пароля.
        /// </summary>
        /// <param name="forgotPasswordModel">Модель запроса сброса пароля.</param>
        /// <response code='200'>Запрос обработан.</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel forgotPasswordModel)
        {
            ForgotPasswordDto forgotPasswordDto = new()
            {
                Email = forgotPasswordModel.Email,
            };

            await _userRepository.ForgotPasswordAsync(forgotPasswordDto);

            return Ok();
        }

        /// <summary>
        /// Сбросить пароль по токену из email.
        /// </summary>
        /// <param name="resetPasswordModel">Модель сброса пароля.</param>
        /// <response code='200'>Пароль успешно изменён.</response>
        /// <response code='400'>Недействительный или истёкший токен.</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel resetPasswordModel)
        {
            ResetPasswordDto resetPasswordDto = new()
            {
                Token = resetPasswordModel.Token,
                NewPassword = resetPasswordModel.NewPassword,
            };

            try
            {
                await _userRepository.ResetPasswordAsync(resetPasswordDto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();
        }
    }
}
