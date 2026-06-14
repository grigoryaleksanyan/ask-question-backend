using System.ComponentModel.DataAnnotations;

namespace AskQuestion.WebApi.Models.Request.User
{
    public class UserSetActiveModel
    {
        [Required]
        public Guid Id { get; set; }

        public bool IsActive { get; set; }
    }
}
