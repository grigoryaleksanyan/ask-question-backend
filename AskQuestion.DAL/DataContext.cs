using AskQuestion.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace AskQuestion.DAL
{
    public class DataContext : DbContext
    {
        /// <summary>
        /// Пользователи.
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// Информация о пользователе.
        /// </summary>
        public DbSet<UserDetails> UserDetails { get; set; }

        /// <summary>
        /// Роли пользователей.
        /// </summary>
        public DbSet<UserRole> UserRoles { get; set; }

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

        /// <summary>
        /// Обратная связь.
        /// </summary>
        public DbSet<Feedback> Feedback { get; set; } = null!;

        /// <summary>
        /// Области.
        /// </summary>
        public DbSet<Area> Areas { get; set; } = null!;

        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
            ArgumentNullException.ThrowIfNull(options);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresExtension("uuid-ossp");

            modelBuilder.Entity<UserRole>().HasData
            (
                new List<UserRole>
                {
                    new() { UserRoleId = 1, Name = "Administrator" },
                    new() { UserRoleId = 2, Name = "Speaker" },
                }
            );

            modelBuilder.Entity<User>().HasIndex(prop => prop.Login).IsUnique();

            modelBuilder.Entity<User>().HasData
            (
                new List<User>
                {
                    new() { Id = Guid.NewGuid(), Login = "Admin", UserRoleId = 1, Password = BCrypt.Net.BCrypt.HashPassword("Admin"), Сreated = DateTimeOffset.UtcNow },
                }
            );
        }
    }
}