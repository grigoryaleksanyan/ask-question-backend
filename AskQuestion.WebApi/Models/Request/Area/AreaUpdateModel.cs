using System.ComponentModel.DataAnnotations;

namespace AskQuestion.WebApi.Models.Request.Area
{
    public class AreaUpdateModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Id области должно быть указано.")]
        public Guid Id { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Заголовок должен быть указан.")]
        public string Title { get; set; } = null!;
    }
}
