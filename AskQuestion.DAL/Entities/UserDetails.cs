namespace AskQuestion.DAL.Entities
{
    public class UserDetails : BaseEntity
    {
        public Guid UserId { get; set; }

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string? Position { get; set; }

        public int Order { get; set; }

        #region Навигационные свойства

        public User User { get; set; }

        #endregion

        public string GetFullName()
        {
            return string.Join(" ", new[] { LastName, FirstName }.Where(s => !string.IsNullOrEmpty(s)));
        }
    }
}
