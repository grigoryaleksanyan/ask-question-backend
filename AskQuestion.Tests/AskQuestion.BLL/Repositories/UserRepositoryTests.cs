using AskQuestion.BLL.DTO.User;
using AskQuestion.BLL.Repositories.Implementations;
using AskQuestion.BLL.Tests.Helpers;
using AskQuestion.Core.Enums;
using FluentAssertions;

namespace AskQuestion.BLL.Tests.Repositories;

public class UserRepositoryTests : RepositoryTestBase
{
    [Fact]
    public async Task AuthorizeUser_ReturnsDto_WhenCredentialsValid()
    {
        await TestDataSeeder.SeedUserAsync(DataContext, "speaker@test.com", "Password1", UserRoles.Speaker);
        var repo = new UserRepository(DataContext, HtmlSanitizer, EmailSender, SmtpSettings);

        var result = await repo.AuthorizeUser(new UserAuthDto
        {
            Email = "speaker@test.com",
            Password = "Password1",
        });

        result.Should().NotBeNull();
        result!.Email.Should().Be("speaker@test.com");
        result.UserRoleId.Should().Be(UserRoles.Speaker);
    }

    [Fact]
    public async Task AuthorizeUser_ReturnsNull_WhenPasswordInvalid()
    {
        await TestDataSeeder.SeedUserAsync(DataContext, "speaker@test.com", "Password1", UserRoles.Speaker);
        var repo = new UserRepository(DataContext, HtmlSanitizer, EmailSender, SmtpSettings);

        var result = await repo.AuthorizeUser(new UserAuthDto
        {
            Email = "speaker@test.com",
            Password = "wrong",
        });

        result.Should().BeNull();
    }

    [Fact]
    public async Task AuthorizeUser_ReturnsNull_WhenUserNotActive()
    {
        await TestDataSeeder.SeedUserAsync(DataContext, "speaker@test.com", "Password1", UserRoles.Speaker, isActive: false);
        var repo = new UserRepository(DataContext, HtmlSanitizer, EmailSender, SmtpSettings);

        var result = await repo.AuthorizeUser(new UserAuthDto
        {
            Email = "speaker@test.com",
            Password = "Password1",
        });

        result.Should().BeNull();
    }

    [Fact]
    public async Task SetupAdminAsync_CreatesAdmin_WhenNoAdminExists()
    {
        var repo = new UserRepository(DataContext, HtmlSanitizer, EmailSender, SmtpSettings);

        var result = await repo.SetupAdminAsync(new AdminSetupDto
        {
            Email = "admin@test.com",
            Password = "Password1",
            FirstName = "Admin",
            LastName = "Adminov",
        });

        result.Should().NotBeNull();
        result.Email.Should().Be("admin@test.com");
        result.UserRoleId.Should().Be(UserRoles.Administrator);
        result.UserDetails.Should().NotBeNull();
    }

    [Fact]
    public async Task SetupAdminAsync_Throws_WhenAdminAlreadyExists()
    {
        await TestDataSeeder.SeedUserAsync(DataContext, "admin@test.com", "Password1", UserRoles.Administrator);
        var repo = new UserRepository(DataContext, HtmlSanitizer, EmailSender, SmtpSettings);

        Func<Task> act = async () => await repo.SetupAdminAsync(new AdminSetupDto
        {
            Email = "admin2@test.com",
            Password = "Password1",
            FirstName = "Admin",
            LastName = "Adminov",
        });

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Администратор уже существует");
    }

    [Fact]
    public async Task SetupAdminAsync_Throws_WhenEmailNotUnique()
    {
        await TestDataSeeder.SeedUserAsync(DataContext, "existing@test.com", "Password1", UserRoles.Speaker);
        var repo = new UserRepository(DataContext, HtmlSanitizer, EmailSender, SmtpSettings);

        Func<Task> act = async () => await repo.SetupAdminAsync(new AdminSetupDto
        {
            Email = "existing@test.com",
            Password = "Password1",
            FirstName = "Admin",
            LastName = "Adminov",
        });

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Пользователь с таким Email уже существует");
    }

    [Fact]
    public async Task ChangePassword_UpdatesPassword_WhenCurrentPasswordValid()
    {
        var user = await TestDataSeeder.SeedUserAsync(DataContext, "user@test.com", "Password1", UserRoles.Speaker);
        var repo = new UserRepository(DataContext, HtmlSanitizer, EmailSender, SmtpSettings);

        await repo.ChangePassword(new UserPasswordUpdateDto
        {
            Id = user.Id,
            Password = "Password1",
            NewPassword = "NewPassword1",
        });

        var authorized = await repo.AuthorizeUser(new UserAuthDto
        {
            Email = "user@test.com",
            Password = "NewPassword1",
        });
        authorized.Should().NotBeNull();
    }

    [Fact]
    public async Task ChangePassword_Throws_WhenCurrentPasswordInvalid()
    {
        var user = await TestDataSeeder.SeedUserAsync(DataContext, "user@test.com", "Password1", UserRoles.Speaker);
        var repo = new UserRepository(DataContext, HtmlSanitizer, EmailSender, SmtpSettings);

        Func<Task> act = async () => await repo.ChangePassword(new UserPasswordUpdateDto
        {
            Id = user.Id,
            Password = "wrong",
            NewPassword = "NewPassword1",
        });

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ForgotPasswordAsync_EnqueuesEmail_WhenUserExists()
    {
        await TestDataSeeder.SeedUserAsync(DataContext, "user@test.com", "Password1", UserRoles.Speaker);
        var repo = new UserRepository(DataContext, HtmlSanitizer, EmailSender, SmtpSettings);

        await repo.ForgotPasswordAsync(new ForgotPasswordDto { Email = "user@test.com" });

        EmailSender.Messages.Should().ContainSingle();
        EmailSender.Messages[0].ToEmail.Should().Be("user@test.com");
        DataContext.PasswordResetTokens.Should().ContainSingle(t => !t.IsUsed);
    }

    [Fact]
    public async Task ForgotPasswordAsync_DoesNothing_WhenUserNotActive()
    {
        await TestDataSeeder.SeedUserAsync(DataContext, "user@test.com", "Password1", UserRoles.Speaker, isActive: false);
        var repo = new UserRepository(DataContext, HtmlSanitizer, EmailSender, SmtpSettings);

        await repo.ForgotPasswordAsync(new ForgotPasswordDto { Email = "user@test.com" });

        EmailSender.Messages.Should().BeEmpty();
        DataContext.PasswordResetTokens.Should().BeEmpty();
    }

    [Fact]
    public async Task ForgotPasswordAsync_MarksPreviousTokensAsUsed()
    {
        var user = await TestDataSeeder.SeedUserAsync(DataContext, "user@test.com", "Password1", UserRoles.Speaker);
        await TestDataSeeder.SeedPasswordResetTokenAsync(DataContext, user.Id, "old-token");
        var repo = new UserRepository(DataContext, HtmlSanitizer, EmailSender, SmtpSettings);

        await repo.ForgotPasswordAsync(new ForgotPasswordDto { Email = "user@test.com" });

        DataContext.PasswordResetTokens.Count(t => t.IsUsed).Should().Be(1);
        DataContext.PasswordResetTokens.Count(t => !t.IsUsed).Should().Be(1);
    }

    [Fact]
    public async Task ResetPasswordAsync_UpdatesPassword_WhenTokenValid()
    {
        var user = await TestDataSeeder.SeedUserAsync(DataContext, "user@test.com", "Password1", UserRoles.Speaker);
        await TestDataSeeder.SeedPasswordResetTokenAsync(DataContext, user.Id, "valid-token");
        var repo = new UserRepository(DataContext, HtmlSanitizer, EmailSender, SmtpSettings);

        await repo.ResetPasswordAsync(new ResetPasswordDto
        {
            Token = "valid-token",
            NewPassword = "NewPassword1",
        });

        var authorized = await repo.AuthorizeUser(new UserAuthDto
        {
            Email = "user@test.com",
            Password = "NewPassword1",
        });
        authorized.Should().NotBeNull();
    }

    [Fact]
    public async Task ResetPasswordAsync_Throws_WhenTokenAlreadyUsed()
    {
        var user = await TestDataSeeder.SeedUserAsync(DataContext, "user@test.com", "Password1", UserRoles.Speaker);
        await TestDataSeeder.SeedPasswordResetTokenAsync(DataContext, user.Id, "used-token", isUsed: true);
        var repo = new UserRepository(DataContext, HtmlSanitizer, EmailSender, SmtpSettings);

        Func<Task> act = async () => await repo.ResetPasswordAsync(new ResetPasswordDto
        {
            Token = "used-token",
            NewPassword = "NewPassword1",
        });

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ResetPasswordAsync_Throws_WhenTokenExpired()
    {
        var user = await TestDataSeeder.SeedUserAsync(DataContext, "user@test.com", "Password1", UserRoles.Speaker);
        await TestDataSeeder.SeedPasswordResetTokenAsync(DataContext, user.Id, "expired-token", expiresAt: DateTimeOffset.UtcNow.AddHours(-1));
        var repo = new UserRepository(DataContext, HtmlSanitizer, EmailSender, SmtpSettings);

        Func<Task> act = async () => await repo.ResetPasswordAsync(new ResetPasswordDto
        {
            Token = "expired-token",
            NewPassword = "NewPassword1",
        });

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsDto_WhenExists()
    {
        var user = await TestDataSeeder.SeedUserAsync(DataContext, "user@test.com", "Password1", UserRoles.Speaker);
        var repo = new UserRepository(DataContext, HtmlSanitizer, EmailSender, SmtpSettings);

        var result = await repo.GetById(user.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Email.Should().Be("user@test.com");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
    {
        var repo = new UserRepository(DataContext, HtmlSanitizer, EmailSender, SmtpSettings);

        var result = await repo.GetById(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task IsAdminExistsAsync_ReturnsTrue_WhenAdminExists()
    {
        await TestDataSeeder.SeedUserAsync(DataContext, "admin@test.com", "Password1", UserRoles.Administrator);
        var repo = new UserRepository(DataContext, HtmlSanitizer, EmailSender, SmtpSettings);

        var result = await repo.IsAdminExistsAsync();

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsAdminExistsAsync_ReturnsFalse_WhenNoAdmin()
    {
        var repo = new UserRepository(DataContext, HtmlSanitizer, EmailSender, SmtpSettings);

        var result = await repo.IsAdminExistsAsync();

        result.Should().BeFalse();
    }

    [Fact]
    public async Task AuthorizeUser_ReturnsNull_WhenUserNotFound()
    {
        var repo = new UserRepository(DataContext, HtmlSanitizer, EmailSender, SmtpSettings);

        var result = await repo.AuthorizeUser(new UserAuthDto
        {
            Email = "missing@test.com",
            Password = "Password1",
        });

        result.Should().BeNull();
    }

    [Fact]
    public async Task ChangePassword_Throws_WhenUserNotFound()
    {
        var repo = new UserRepository(DataContext, HtmlSanitizer, EmailSender, SmtpSettings);

        Func<Task> act = async () => await repo.ChangePassword(new UserPasswordUpdateDto
        {
            Id = Guid.NewGuid(),
            Password = "Password1",
            NewPassword = "NewPassword1",
        });

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ForgotPasswordAsync_DoesNothing_WhenUserNotFound()
    {
        var repo = new UserRepository(DataContext, HtmlSanitizer, EmailSender, SmtpSettings);

        await repo.ForgotPasswordAsync(new ForgotPasswordDto { Email = "missing@test.com" });

        EmailSender.Messages.Should().BeEmpty();
        DataContext.PasswordResetTokens.Should().BeEmpty();
    }

    [Fact]
    public async Task ResetPasswordAsync_Throws_WhenUserNotFound()
    {
        await TestDataSeeder.SeedPasswordResetTokenAsync(DataContext, Guid.NewGuid(), "valid-token");
        var repo = new UserRepository(DataContext, HtmlSanitizer, EmailSender, SmtpSettings);

        Func<Task> act = async () => await repo.ResetPasswordAsync(new ResetPasswordDto
        {
            Token = "valid-token",
            NewPassword = "NewPassword1",
        });

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task SetupAdminAsync_SanitizesInput()
    {
        var repo = new UserRepository(DataContext, HtmlSanitizer, EmailSender, SmtpSettings);

        var result = await repo.SetupAdminAsync(new AdminSetupDto
        {
            Email = "admin@test.com",
            Password = "Password1",
            FirstName = "<script>bad</script>Admin",
            LastName = "<script>bad</script>Adminov",
        });

        result.UserDetails!.FirstName.Should().NotContain("<script>");
        result.UserDetails.LastName.Should().NotContain("<script>");
    }
}
