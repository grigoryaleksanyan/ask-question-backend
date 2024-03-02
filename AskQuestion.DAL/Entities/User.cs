namespace AskQuestion.DAL.Entities
{
    public class User : BaseEntity
    {
        /// <summary>
		/// Логин пользователя.
		/// </summary>
		public string Login { get; set; } = null!;

        /// <summary>
		/// Зашифрованный пароль пользователя.
		/// </summary>
        public string Password { get; set; } = null!;

        /// <summary>
		/// Идентификатор роли пользователя.
		/// </summary>
		public int UserRoleId { get; set; }

        #region Навигационные свойства

        public UserRole UserRole { get; set; }
        public UserDetails UserDetails { get; set; }

        #endregion
    }
}
