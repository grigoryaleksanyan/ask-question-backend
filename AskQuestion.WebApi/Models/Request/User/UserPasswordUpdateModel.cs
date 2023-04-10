using System.ComponentModel.DataAnnotations;

namespace AskQuestion.WebApi.Models.Request.User
{
    public class UserPasswordUpdateModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "текущий пароль должен быть указан.")]
        public string Password { get; set; } = null!;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Новый пароль должен быть указан.")]
        public string NewPassword { get; set; } = null!;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Пароль подтверждения должен быть указан.")]
        [Compare("NewPassword", ErrorMessage = "Пароли подтверждения не совпадает.")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
