namespace AskQuestion.BLL.DTO.Question
{
    public class QuestionStatusChangeDto
    {
        public Guid QuestionId { get; set; }
        public int NewStatus { get; set; }
        public Guid? ChangedByUserId { get; set; }
    }
}
