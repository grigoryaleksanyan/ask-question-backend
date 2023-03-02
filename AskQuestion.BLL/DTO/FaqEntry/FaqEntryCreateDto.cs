namespace AskQuestion.BLL.DTO.FaqEntry
{
    public class FaqEntryCreateDto
    {
        public Guid FaqCategoryId { get; set; }
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public int Order { get; set; }
    }
}
