namespace AskQuestion.BLL.DTO.Question
{
    public class QuestionCreateDto
    {
        public string Text { get; set; } = string.Empty;
        public string? Author { get; set; }
        public string? Area { get; set; }
        public string Speaker { get; set; } = string.Empty;
    }
}
