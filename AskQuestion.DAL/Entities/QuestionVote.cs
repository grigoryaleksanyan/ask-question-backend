using AskQuestion.Core.Enums;

namespace AskQuestion.DAL.Entities
{
    public class QuestionVote
    {
        public Guid QuestionId { get; set; }
        public Guid VisitorId { get; set; }
        public VoteType VoteType { get; set; }

        public Question Question { get; set; } = null!;
    }
}
