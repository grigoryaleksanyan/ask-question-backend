namespace AskQuestion.BLL.DTO.User
{
    public class UserDetailsCreateDto
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? Position { get; set; }
    }
}
