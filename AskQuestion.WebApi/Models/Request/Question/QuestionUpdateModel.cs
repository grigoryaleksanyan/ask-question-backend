using System.ComponentModel.DataAnnotations;

namespace AskQuestion.WebApi.Models.Request.Question
{
    public class QuestionUpdateModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Текст вопроса не должен быть пустым.")]
        public string Text { get; set; } = string.Empty;

        public string? Author { get; set; }

        public string? Area { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Спикер должен быть указан.")]
        public string Speaker { get; set; } = string.Empty;
    }
}
