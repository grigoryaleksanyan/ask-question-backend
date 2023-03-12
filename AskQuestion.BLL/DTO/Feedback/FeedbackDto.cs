namespace AskQuestion.BLL.DTO.Feedback
{
    public class FeedbackDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Theme { get; set; } = null!;
        public string Text { get; set; } = null!;
        public DateTimeOffset Сreated { get; set; }
        public DateTimeOffset? Updated { get; set; }
    }
}
