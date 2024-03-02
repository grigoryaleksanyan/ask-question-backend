using AskQuestion.BLL.DTO.FaqEntry;

namespace AskQuestion.BLL.DTO.FaqCategory
{
    public class FaqCategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Order { get; set; }
        public DateTimeOffset Сreated { get; set; }
        public DateTimeOffset? Updated { get; set; }
        public IEnumerable<FaqEntryDto> Entries { get; set; } = Enumerable.Empty<FaqEntryDto>();
    }
}
