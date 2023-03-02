using System.ComponentModel.DataAnnotations;

namespace AskQuestion.WebApi.Models.Request.FaqCategory
{
    public class FaqCategoryUpdateModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Id категории должно быть указано.")]
        public Guid Id { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Название категории должно быть указано.")]
        public string Name { get; set; } = string.Empty;
    }
}
