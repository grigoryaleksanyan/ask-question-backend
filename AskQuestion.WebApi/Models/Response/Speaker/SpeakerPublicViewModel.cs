namespace AskQuestion.WebApi.Models.Response.Speaker
{
    public class SpeakerPublicViewModel
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? Position { get; set; }
    }
}
