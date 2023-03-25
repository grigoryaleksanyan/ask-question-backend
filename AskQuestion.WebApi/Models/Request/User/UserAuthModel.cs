using System.ComponentModel.DataAnnotations;

namespace AskQuestion.WebApi.Models.Request.User
{
    public class UserAuthModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Логин должен быть указан.")]
        public string Login { get; set; } = null!;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Пароль должен быть указан.")]
        public string Password { get; set; } = null!;
    }
}
