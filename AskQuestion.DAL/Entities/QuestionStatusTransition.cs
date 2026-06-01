namespace AskQuestion.DAL.Entities
{
    public class QuestionStatusTransition : BaseEntity
    {
        public Guid QuestionId { get; set; }
        public int FromStatus { get; set; }
        public int ToStatus { get; set; }
        public Guid? ChangedByUserId { get; set; }

        public Question? Question { get; set; }
        public User? ChangedByUser { get; set; }
    }
}
