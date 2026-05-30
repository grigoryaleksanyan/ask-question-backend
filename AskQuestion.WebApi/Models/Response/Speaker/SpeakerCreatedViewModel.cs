namespace AskQuestion.WebApi.Models.Response.Speaker
{
    public class SpeakerCreatedViewModel : SpeakerViewModel
    {
        public string GeneratedPassword { get; set; } = null!;
    }
}
