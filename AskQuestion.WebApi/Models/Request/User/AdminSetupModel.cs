using System.ComponentModel.DataAnnotations;

namespace AskQuestion.WebApi.Models.Request.User
{
    public class AdminSetupModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Email должен быть указан.")]
        [EmailAddress(ErrorMessage = "Введите корректный email.")]
        public string Email { get; set; } = null!;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Пароль должен быть указан.")]
        [MinLength(6, ErrorMessage = "Пароль должен содержать не менее 6 символов.")]
        public string Password { get; set; } = null!;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Подтверждение пароля должно быть указано.")]
        [Compare("Password", ErrorMessage = "Пароли не совпадают.")]
        public string ConfirmPassword { get; set; } = null!;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Имя должно быть указано.")]
        public string FirstName { get; set; } = null!;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Фамилия должна быть указана.")]
        public string LastName { get; set; } = null!;
    }
}
