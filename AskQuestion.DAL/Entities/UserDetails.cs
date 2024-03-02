namespace AskQuestion.DAL.Entities
{
    public class UserDetails : BaseEntity
    {
        /// <summary>
		/// Id пользователя.
		/// </summary>
        public Guid UserId { get; set; }

        /// <summary>
		/// ФИО пользователя.
		/// </summary>
        public string FullName { get; set; } = null!;

        /// <summary>
		/// Email пользователя.
		/// </summary>
        public string Email { get; set; } = null!;

        /// <summary>
		/// Любая дополнительная информация.
		/// </summary>
        public string? AdditionalInfo { get; set; }

        #region Навигационные свойства

        public User User { get; set; }

        #endregion
    }
}
