namespace AskQuestion.BLL.DTO.Dashboard;

public class DashboardSummaryDto
{
    public int TotalQuestions { get; set; }
    public int AnsweredQuestions { get; set; }
    public int UnansweredQuestions { get; set; }
    public double AverageResponseTimeHours { get; set; }
    public int TotalFeedback { get; set; }
    public int TotalAreas { get; set; }
    public int QuestionsWithoutSpeaker { get; set; }
    public List<StatusDistributionDto> ByStatus { get; set; } = [];
    public List<TimelinePointDto> Timeline { get; set; } = [];
    public List<AreaDistributionDto> ByArea { get; set; } = [];
    public List<SpeakerProductivityDto> TopSpeakers { get; set; } = [];
    public List<SpeakerAreaDto> SpeakerAreas { get; set; } = [];
    public VotesSummaryDto Votes { get; set; } = new();
}

public class StatusDistributionDto
{
    public int Status { get; set; }
    public int Count { get; set; }
}

public class TimelinePointDto
{
    public string Date { get; set; } = string.Empty;
    public int NewCount { get; set; }
    public int AnsweredCount { get; set; }
}

public class AreaDistributionDto
{
    public string AreaTitle { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class VotesSummaryDto
{
    public int TotalLikes { get; set; }
    public int TotalDislikes { get; set; }
}
