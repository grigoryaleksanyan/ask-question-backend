namespace AskQuestion.BLL.DTO.Dashboard;

public class SpeakerProductivityDto
{
    public Guid SpeakerId { get; set; }
    public string SpeakerName { get; set; } = string.Empty;
    public int AssignedQuestions { get; set; }
    public int AnsweredQuestions { get; set; }
    public double AnswerRate { get; set; }
    public double AverageResponseHours { get; set; }
}
