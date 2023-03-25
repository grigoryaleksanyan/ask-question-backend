namespace AskQuestion.DAL.Entities
{
    public class Area : BaseEntity
    {
        public string Title { get; set; } = null!;
        public int Order { get; set; }
    }
}
