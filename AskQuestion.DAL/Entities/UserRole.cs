namespace AskQuestion.DAL.Entities
{
    public class UserRole
    {
        /// <summary>
		/// Идентификатор роли.
		/// </summary>
		public int UserRoleId { get; set; }

        /// <summary>
        /// Название роли.
        /// </summary>
        public string Name { get; set; } = null!;

        #region Навигационные свойства

        public IEnumerable<User> Users { get; set; }

        #endregion
    }
}
