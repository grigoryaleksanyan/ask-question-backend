namespace AskQuestion.DAL.Entities
{
    public class BaseEntity
    {
        public Guid Id { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? Updated { get; set; }
    }

}
