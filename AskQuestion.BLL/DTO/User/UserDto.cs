using AskQuestion.Core.Enums;

namespace AskQuestion.BLL.DTO.User
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Login { get; set; } = null!;
        public string Password { get; set; } = null!;
        public UserRoles UserRoleId { get; set; }
        public UserDetailsDto? UserDetails { get; set; }
        public DateTimeOffset Сreated { get; set; }
        public DateTimeOffset? Updated { get; set; }
    }
}
