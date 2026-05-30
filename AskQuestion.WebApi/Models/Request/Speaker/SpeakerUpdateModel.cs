using System.ComponentModel.DataAnnotations;

namespace AskQuestion.WebApi.Models.Request.Speaker
{
    public class SpeakerUpdateModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Id спикера должен быть указан.")]
        public Guid Id { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Имя должно быть указано.")]
        public string FirstName { get; set; } = null!;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Фамилия должна быть указана.")]
        public string LastName { get; set; } = null!;

        public string? Patronymic { get; set; }

        public string? Position { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Почта спикера должна быть указана.")]
        [EmailAddress(ErrorMessage = "Почта должна быть валидна.")]
        public string Email { get; set; } = null!;

        public string? AdditionalInfo { get; set; }
    }
}
