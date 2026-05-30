using AskQuestion.Core.Enums;

namespace AskQuestion.BLL.DTO.Question
{
    public class QuestionDto
    {
        public Guid Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string? Author { get; set; }
        public Guid? AreaId { get; set; }
        public string? AreaTitle { get; set; }
        public Guid? SpeakerId { get; set; }
        public string? SpeakerName { get; set; }
        public int Views { get; set; }
        public int Likes { get; set; }
        public int Dislikes { get; set; }
        public VoteType? UserVote { get; set; }
        public int Status { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? Answered { get; set; }
    }
}
