namespace AskQuestion.DAL.Entities
{
    public class PasswordResetToken : BaseEntity
    {
        public Guid UserId { get; set; }
        public string TokenHash { get; set; } = null!;
        public DateTimeOffset ExpiresAt { get; set; }
        public bool IsUsed { get; set; }

        #region Навигационные свойства

        public User User { get; set; }

        #endregion
    }
}
