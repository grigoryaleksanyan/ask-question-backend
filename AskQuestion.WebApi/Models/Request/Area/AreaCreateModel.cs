using System.ComponentModel.DataAnnotations;

namespace AskQuestion.WebApi.Models.Request.Area
{
    public class AreaCreateModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Заголовок должен быть указан.")]
        public string Title { get; set; } = null!;

        public int Order { get; set; }
    }
}
