using AskQuestion.WebApi.Models;
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
        public static User user = new User();

        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("Register")]
        public ActionResult<User> Register(UserDto request)
        {
            string passwordHash
                = BCrypt.Net.BCrypt.HashPassword(request.Password);

            user.UserName = request.UserName;
            user.PasswordHash = passwordHash;

            return Ok(user);
        }

        [HttpPost("Login")]
        public ActionResult<User> Login(UserDto request)
        {
            if (user.UserName != request.UserName)
            {
                return BadRequest("Неверный логин или пароль.");
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return BadRequest("Неверный логин или пароль.");
            }

            string token = CreateToken(user);

            if (token != null)
                HttpContext.Response.Cookies.Append(".WebApi", token,
                new CookieOptions
                {
                    MaxAge = TimeSpan.FromMinutes(60)
                });

            HttpContext.Response.Headers.Add("X-Content-Type-Options", "nosniff");
            HttpContext.Response.Headers.Add("X-Xss-Protection", "1");
            HttpContext.Response.Headers.Add("X-Frame-Options", "DENY");

            return Ok(token);
        }

        [HttpPut("Logout")]
        public IActionResult Logout()
        {
            HttpContext.Response.Cookies.Delete(".WebApi");

            return Ok();
        }

        private string CreateToken(User user)
        {
            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, "Admin"),
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
