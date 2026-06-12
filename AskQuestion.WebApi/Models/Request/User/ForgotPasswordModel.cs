using System.ComponentModel.DataAnnotations;

namespace AskQuestion.WebApi.Models.Request.User
{
    public class ForgotPasswordModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Email должен быть указан.")]
        [EmailAddress(ErrorMessage = "Введите корректный email.")]
        public string Email { get; set; } = null!;
    }
}
