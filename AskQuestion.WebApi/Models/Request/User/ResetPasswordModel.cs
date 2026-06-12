using System.ComponentModel.DataAnnotations;

namespace AskQuestion.WebApi.Models.Request.User
{
    public class ResetPasswordModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Токен должен быть указан.")]
        public string Token { get; set; } = null!;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Новый пароль должен быть указан.")]
        [MinLength(6, ErrorMessage = "Пароль должен содержать не менее 6 символов.")]
        public string NewPassword { get; set; } = null!;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Подтверждение пароля должно быть указано.")]
        [Compare("NewPassword", ErrorMessage = "Пароли не совпадают.")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
