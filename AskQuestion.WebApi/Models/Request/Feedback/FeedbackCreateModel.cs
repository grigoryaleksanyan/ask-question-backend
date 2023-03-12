using System.ComponentModel.DataAnnotations;

namespace AskQuestion.WebApi.Models.Request.Feedback
{
    public class FeedbackCreateModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Имя пользователя должно быть указано.")]
        public string Username { get; set; } = null!;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Почта должна быть указана.")]
        [EmailAddress(ErrorMessage = "Почта должна быть валидна.")]
        public string Email { get; set; } = null!;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Тема должна быть указана.")]
        public string Theme { get; set; } = null!;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Текст обращения должен быть указан.")]
        public string Text { get; set; } = null!;
    }
}
