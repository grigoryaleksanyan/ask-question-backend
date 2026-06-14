namespace AskQuestion.BLL.DTO.Speaker
{
    public class SpeakerUpdateDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? Position { get; set; }
        public string Email { get; set; } = null!;
    }
}
