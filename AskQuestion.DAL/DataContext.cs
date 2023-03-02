using AskQuestion.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace AskQuestion.DAL
{
    public class DataContext : DbContext
	{
		/// <summary>
		/// Вопросы.
		/// </summary>
		public DbSet<Question> Questions { get; set; } = null!;

        /// <summary>
        /// Категории FAQ.
        /// </summary>
        public DbSet<FaqCategory> FaqCategories { get; set; } = null!;

        /// <summary>
        /// Записи в FAQ.
        /// </summary>
        public DbSet<FaqEntry> FaqEntries { get; set; } = null!;

        public DataContext(DbContextOptions<DataContext> options) : base(options)
		{
			if (options is null)
			{
				throw new ArgumentNullException(nameof(options));
			}
		}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
			modelBuilder.HasPostgresExtension("uuid-ossp");
		}
	}
}