using System.ComponentModel.DataAnnotations;

namespace AskQuestion.WebApi.Models.Request.FaqCategory
{
    public class FaqCategoryCreateModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Название категории должно быть указано.")]
        public string Name { get; set; } = string.Empty;

        public int Order { get; set; }
    }
}
