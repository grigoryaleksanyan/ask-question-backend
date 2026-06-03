using System.ComponentModel.DataAnnotations;

namespace AskQuestion.WebApi.Models.Request.User
{
    public class UserAuthModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Email должен быть указан.")]
        [EmailAddress(ErrorMessage = "Введите корректный email.")]
        public string Email { get; set; } = null!;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Пароль должен быть указан.")]
        public string Password { get; set; } = null!;
    }
}
