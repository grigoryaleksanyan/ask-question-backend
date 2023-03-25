namespace AskQuestion.WebApi.Models.Response.Area
{
    public class AreaViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public int Order { get; set; }
        public DateTimeOffset Сreated { get; set; }
        public DateTimeOffset? Updated { get; set; }
    }
}
