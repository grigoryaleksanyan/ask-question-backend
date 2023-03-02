namespace AskQuestion.BLL.DTO.FaqEntry
{
    public class FaqEntryUpdateDto
    {
        public Guid Id { get; set; }
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
    }
}
