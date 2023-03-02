using System.ComponentModel.DataAnnotations;

namespace AskQuestion.WebApi.Models.Request.Question
{
    public class QuestionCreateModel
    {
        public string? Author { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Спикер должен быть указан.")]
        public string Speaker { get; set; } = string.Empty;

        public string? Zone { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Текст вопроса не должен быть пустым.")]
        public string Text { get; set; } = string.Empty;
    }
}
