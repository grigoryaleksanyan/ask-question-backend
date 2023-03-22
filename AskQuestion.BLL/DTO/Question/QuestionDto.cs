namespace AskQuestion.BLL.DTO.Question
{
    public class QuestionDto
    {
        public Guid Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string? Author { get; set; }
        public string? Area { get; set; }
        public string Speaker { get; set; } = string.Empty;
        public int Views { get; set; }
        public int Likes { get; set; }
        public int Dislikes { get; set; }
        public DateTimeOffset Сreated { get; set; }
        public DateTimeOffset? Answered { get; set; }
    }
}
