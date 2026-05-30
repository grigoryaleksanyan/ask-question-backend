namespace AskQuestion.BLL.DTO.Question
{
    public class QuestionUpdateDto
    {
        public Guid Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string? Author { get; set; }
        public Guid? AreaId { get; set; }
        public Guid? SpeakerId { get; set; }
    }
}
