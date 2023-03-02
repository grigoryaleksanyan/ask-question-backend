using System.ComponentModel.DataAnnotations;

namespace AskQuestion.WebApi.Models.Request.FaqEntry
{
    public class FaqEntryCreateModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Id категории должен быть указан.")]
        public Guid FaqCategoryId { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Вопрос должен быть указан.")]
        public string Question { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Ответ должен быть указан.")]
        public string Answer { get; set; } = string.Empty;
        public int Order { get; set; }
    }
}
