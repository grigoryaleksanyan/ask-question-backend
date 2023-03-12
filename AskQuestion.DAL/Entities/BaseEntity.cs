namespace AskQuestion.DAL.Entities
{
    public class BaseEntity
    {
        public Guid Id { get; set; }
        public DateTimeOffset Сreated { get; set; }
        public DateTimeOffset? Updated { get; set; }
    }

}
