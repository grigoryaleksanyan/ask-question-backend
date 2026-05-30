namespace AskQuestion.BLL.DTO.Dashboard;

public class SpeakerAreaDto
{
    public Guid SpeakerId { get; set; }
    public string SpeakerName { get; set; } = string.Empty;
    public string AreaTitle { get; set; } = string.Empty;
    public int QuestionCount { get; set; }
}
