using AskQuestion.Core.Enums;
using AskQuestion.DAL;
using AskQuestion.DAL.Entities;
using System.Security.Cryptography;
using System.Text;

namespace AskQuestion.BLL.Tests.Helpers;

public static class TestDataSeeder
{
    public static async Task<User> SeedUserAsync(
        DataContext context,
        string email,
        string password,
        UserRoles role,
        string firstName = "First",
        string lastName = "Last",
        string? patronymic = null,
        bool isDeleted = false,
        int order = 0)
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = email,
            Password = BCrypt.Net.BCrypt.HashPassword(password, 4),
            UserRoleId = (int)role,
            Created = DateTimeOffset.UtcNow,
            Updated = DateTimeOffset.UtcNow,
        };
        var details = new UserDetails
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FirstName = firstName,
            LastName = lastName,
            Patronymic = patronymic,
            IsDeleted = isDeleted,
            Order = order,
            Created = DateTimeOffset.UtcNow,
            Updated = DateTimeOffset.UtcNow,
        };
        await context.Users.AddAsync(user);
        await context.UserDetails.AddAsync(details);
        await context.SaveChangesAsync();
        return user;
    }

    public static async Task<Area> SeedAreaAsync(DataContext context, string title = "Area")
    {
        var area = new Area
        {
            Id = Guid.NewGuid(),
            Title = title,
            Order = 0,
            Created = DateTimeOffset.UtcNow,
            Updated = DateTimeOffset.UtcNow,
        };
        await context.Areas.AddAsync(area);
        await context.SaveChangesAsync();
        return area;
    }

    public static async Task<Question> SeedQuestionAsync(
        DataContext context,
        string text = "Question text",
        string? author = "Author",
        QuestionStatus status = QuestionStatus.New,
        Guid? speakerId = null,
        Guid? areaId = null,
        int likes = 0,
        int dislikes = 0,
        int views = 0)
    {
        var question = new Question
        {
            Id = Guid.NewGuid(),
            Text = text,
            Author = author,
            AreaId = areaId,
            SpeakerId = speakerId,
            Status = (int)status,
            Likes = likes,
            Dislikes = dislikes,
            Views = views,
            Created = DateTimeOffset.UtcNow,
            Updated = DateTimeOffset.UtcNow,
        };
        await context.Questions.AddAsync(question);
        await context.SaveChangesAsync();
        return question;
    }

    public static async Task<PasswordResetToken> SeedPasswordResetTokenAsync(
        DataContext context,
        Guid userId,
        string rawToken,
        bool isUsed = false,
        DateTimeOffset? expiresAt = null)
    {
        string tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken))).ToLowerInvariant();
        var token = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt ?? DateTimeOffset.UtcNow.AddHours(1),
            IsUsed = isUsed,
            Created = DateTimeOffset.UtcNow,
        };
        await context.PasswordResetTokens.AddAsync(token);
        await context.SaveChangesAsync();
        return token;
    }
}
