namespace AskQuestion.BLL.DTO.User
{
    public class UserDetailsCreateDto
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? AdditionalInfo { get; set; }
    }
}
