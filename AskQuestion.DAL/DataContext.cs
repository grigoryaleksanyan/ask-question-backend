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

        /// <summary>
        /// Голоса за вопросы.
        /// </summary>
        public DbSet<QuestionVote> QuestionVotes { get; set; } = null!;

        /// <summary>
        /// История смены статусов вопросов.
        /// </summary>
        public DbSet<QuestionStatusTransition> QuestionStatusTransitions { get; set; } = null!;

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

            modelBuilder.Entity<User>().HasIndex(prop => prop.Email).IsUnique();

            modelBuilder.Entity<User>().HasData
            (
                new List<User>
                {
                    new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000001"), Email = "admin@askquestion.local", UserRoleId = 1, Password = "$2a$11$q385hN2923xQ0sWnC9I84eaC4jNqSx8m9HzZgZYUxBjh9vBX8cr1S", Created = DateTimeOffset.Parse("2023-01-01T00:00:00+00:00") },
                }
            );

            modelBuilder.Entity<UserDetails>().HasData
            (
                new UserDetails
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                    UserId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    FirstName = "Admin",
                    LastName = "Admin",
                    Patronymic = null,
                    Position = null,
                    AdditionalInfo = null,
                    IsDeleted = false,
                    Created = DateTimeOffset.Parse("2023-01-01T00:00:00+00:00"),
                }
            );

            modelBuilder.Entity<QuestionVote>(entity =>
            {
                entity.HasKey(qv => new { qv.QuestionId, qv.VisitorId });
                entity.HasOne(qv => qv.Question)
                    .WithMany()
                    .HasForeignKey(qv => qv.QuestionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Question>(entity =>
            {
                entity.HasOne(q => q.SpeakerUser)
                    .WithMany()
                    .HasForeignKey(q => q.SpeakerId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(q => q.AreaEntity)
                    .WithMany()
                    .HasForeignKey(q => q.AreaId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<QuestionStatusTransition>(entity =>
            {
                entity.HasOne(qst => qst.Question)
                    .WithMany()
                    .HasForeignKey(qst => qst.QuestionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(qst => qst.ChangedByUser)
                    .WithMany()
                    .HasForeignKey(qst => qst.ChangedByUserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}