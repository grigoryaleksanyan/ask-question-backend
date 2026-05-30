namespace AskQuestion.BLL.DTO.Speaker
{
    public class SpeakerCreateDto
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? Patronymic { get; set; }
        public string? Position { get; set; }
        public string Email { get; set; } = null!;
    }
}
