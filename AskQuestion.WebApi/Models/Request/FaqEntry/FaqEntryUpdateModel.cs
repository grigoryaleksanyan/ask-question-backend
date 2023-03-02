using System.ComponentModel.DataAnnotations;

namespace AskQuestion.WebApi.Models.Request.FaqEntry
{
    public class FaqEntryUpdateModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Id записи должно быть указано.")]
        public Guid Id { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Вопрос должен быть указан.")]
        public string Question { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Ответ должен быть указан.")]
        public string Answer { get; set; } = string.Empty;
    }
}
