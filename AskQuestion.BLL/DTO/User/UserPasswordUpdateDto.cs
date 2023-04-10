namespace AskQuestion.BLL.DTO.User
{
    public class UserPasswordUpdateDto
    {
        public Guid Id { get; set; }
        public string Password { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }
}
