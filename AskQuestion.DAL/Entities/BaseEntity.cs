using System.ComponentModel.DataAnnotations.Schema;

namespace AskQuestion.DAL.Entities
{
    public class BaseEntity
    {
        public Guid Id { get; set; }
        [Column("Сreated")]
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? Updated { get; set; }
    }

}
