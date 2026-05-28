namespace AskQuestion.BLL.DTO.Area
{
    public class AreaDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public int Order { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? Updated { get; set; }
    }
}
