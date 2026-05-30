using System.ComponentModel.DataAnnotations;

namespace AskQuestion.WebApi.Models.Request.User
{
    public class SpeakerCreateModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Логин должен быть указан.")]
        public string Login { get; set; } = null!;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Пароль должен быть указан.")]
        public string Password { get; set; } = null!;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Имя спикера должно быть указано.")]
        public string FirstName { get; set; } = null!;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Фамилия спикера должна быть указана.")]
        public string LastName { get; set; } = null!;

        public string? Patronymic { get; set; }

        public string? Position { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Почта спикера должна быть указана.")]
        [EmailAddress(ErrorMessage = "Почта должна быть валидна.")]
        public string Email { get; set; } = null!;

        public string? AdditionalInfo { get; set; }
    }
}
