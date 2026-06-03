namespace AskQuestion.BLL.DTO.User
{
    public class AdminSetupDto
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? Patronymic { get; set; }
    }
}
