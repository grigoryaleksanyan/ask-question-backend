using AskQuestion.Core.Enums;

namespace AskQuestion.WebApi.Models.Response.User
{
    public class UserViewModel
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public UserRoles UserRoleId { get; set; }
        public bool IsActive { get; set; }
        public UserDetailsViewModel? UserDetails { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? Updated { get; set; }
    }
}
