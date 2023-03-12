namespace AskQuestion.DAL.Entities
{
    public class Question : BaseEntity
    {
        public string? Author { get; set; }
        public string Speaker { get; set; } = string.Empty;
        public string? Zone { get; set; }
        public string Text { get; set; } = string.Empty;
        public int Views { get; set; }
        public int Likes { get; set; }
        public int Dislikes { get; set; }
        public DateTimeOffset? Answered { get; set; }
    }
}
