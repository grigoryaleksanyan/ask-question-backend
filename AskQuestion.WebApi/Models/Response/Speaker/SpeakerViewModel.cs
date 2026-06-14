namespace AskQuestion.WebApi.Models.Response.Speaker
{
    public class SpeakerViewModel
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? Position { get; set; }
        public string Email { get; set; } = null!;
        public int Order { get; set; }
        public bool IsActive { get; set; }
    }
}
