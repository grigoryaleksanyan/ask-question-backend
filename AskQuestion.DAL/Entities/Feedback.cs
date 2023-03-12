namespace AskQuestion.DAL.Entities
{
    public class Feedback: BaseEntity
    {
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Theme { get; set; } = null!;
        public string Text { get; set; } = null!;
    }
}
