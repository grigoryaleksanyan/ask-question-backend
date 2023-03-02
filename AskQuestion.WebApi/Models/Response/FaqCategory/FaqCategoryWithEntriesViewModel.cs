using AskQuestion.WebApi.Models.Response.FaqEntry;

namespace AskQuestion.WebApi.Models.Response.FaqCategory
{
    public class FaqCategoryWithEntriesViewModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int Order { get; set; }

        public DateTimeOffset Сreated { get; set; }
        public DateTimeOffset? Updated { get; set; }

        public IEnumerable<FaqEntryViewModel> Entries { get; set; }
    }
}
