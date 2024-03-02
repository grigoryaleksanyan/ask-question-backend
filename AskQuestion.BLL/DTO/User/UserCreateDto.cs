namespace AskQuestion.BLL.DTO.User
{
    public class UserCreateDto
    {
        public string Login { get; set; } = null!;
        public string Password { get; set; } = null!;
        public UserDetailsCreateDto UserDetails { get; set; }
    }
}
