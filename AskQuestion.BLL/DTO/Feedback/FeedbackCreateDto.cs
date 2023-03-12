namespace AskQuestion.BLL.DTO.Feedback
{
    public class FeedbackCreateDto
    {
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Theme { get; set; } = null!;
        public string Text { get; set; } = null!;
        public DateTimeOffset Сreated { get; set; }
    }
}
