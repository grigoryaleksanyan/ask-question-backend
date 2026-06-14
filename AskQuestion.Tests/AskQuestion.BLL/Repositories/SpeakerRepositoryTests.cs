using AskQuestion.BLL.DTO.Speaker;
using AskQuestion.BLL.Repositories.Implementations;
using AskQuestion.BLL.Tests.Helpers;
using AskQuestion.Core.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AskQuestion.BLL.Tests.Repositories;

public class SpeakerRepositoryTests : RepositoryTestBase
{
    [Fact]
    public async Task GetAllAsync_ReturnsOnlyActiveSpeakers_OrderedByOrder()
    {
        var s1 = await TestDataSeeder.SeedUserAsync(DataContext, "s1@test.com", "p", UserRoles.Speaker, order: 2);
        var s2 = await TestDataSeeder.SeedUserAsync(DataContext, "s2@test.com", "p", UserRoles.Speaker, order: 1);
        await TestDataSeeder.SeedUserAsync(DataContext, "del@test.com", "p", UserRoles.Speaker, isDeleted: true);
        var repo = new SpeakerRepository(DataContext, EmailSender, SmtpSettings, HtmlSanitizer);

        var result = (await repo.GetAllAsync()).ToList();

        result.Should().HaveCount(2);
        result[0].Email.Should().Be("s2@test.com");
        result[1].Email.Should().Be("s1@test.com");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsSpeaker_WhenActive()
    {
        var speaker = await TestDataSeeder.SeedUserAsync(DataContext, "speaker@test.com", "p", UserRoles.Speaker);
        var repo = new SpeakerRepository(DataContext, EmailSender, SmtpSettings, HtmlSanitizer);

        var result = await repo.GetByIdAsync(speaker.Id);

        result.Should().NotBeNull();
        result!.Email.Should().Be("speaker@test.com");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenDeleted()
    {
        var speaker = await TestDataSeeder.SeedUserAsync(DataContext, "speaker@test.com", "p", UserRoles.Speaker, isDeleted: true);
        var repo = new SpeakerRepository(DataContext, EmailSender, SmtpSettings, HtmlSanitizer);

        var result = await repo.GetByIdAsync(speaker.Id);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_CreatesSpeaker_AndEnqueuesEmail()
    {
        var repo = new SpeakerRepository(DataContext, EmailSender, SmtpSettings, HtmlSanitizer);

        var result = await repo.CreateAsync(new SpeakerCreateDto
        {
            Email = "newspeaker@test.com",
            FirstName = "New",
            LastName = "Speaker",
            Order = 1,
        });

        result.Should().NotBeNull();
        result.Email.Should().Be("newspeaker@test.com");
        result.GeneratedPassword.Should().NotBeNullOrEmpty();
        result.GeneratedPassword.Length.Should().Be(8);
        EmailSender.Messages.Should().ContainSingle();
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenEmailAlreadyExists()
    {
        await TestDataSeeder.SeedUserAsync(DataContext, "existing@test.com", "p", UserRoles.Speaker);
        var repo = new SpeakerRepository(DataContext, EmailSender, SmtpSettings, HtmlSanitizer);

        Func<Task> act = async () => await repo.CreateAsync(new SpeakerCreateDto
        {
            Email = "existing@test.com",
            FirstName = "New",
            LastName = "Speaker",
            Order = 1,
        });

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Пользователь с таким Email уже существует");
    }

    [Fact]
    public async Task UpdateAsync_UpdatesSpeaker_AndEmail()
    {
        var speaker = await TestDataSeeder.SeedUserAsync(DataContext, "speaker@test.com", "p", UserRoles.Speaker);
        var repo = new SpeakerRepository(DataContext, EmailSender, SmtpSettings, HtmlSanitizer);

        var result = await repo.UpdateAsync(new SpeakerUpdateDto
        {
            Id = speaker.Id,
            Email = "updated@test.com",
            FirstName = "Updated",
            LastName = "Speaker",
        });

        result.Email.Should().Be("updated@test.com");
        result.FirstName.Should().Be("Updated");
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenEmailNotUnique()
    {
        var speaker = await TestDataSeeder.SeedUserAsync(DataContext, "speaker@test.com", "p", UserRoles.Speaker);
        await TestDataSeeder.SeedUserAsync(DataContext, "other@test.com", "p", UserRoles.Speaker);
        var repo = new SpeakerRepository(DataContext, EmailSender, SmtpSettings, HtmlSanitizer);

        Func<Task> act = async () => await repo.UpdateAsync(new SpeakerUpdateDto
        {
            Id = speaker.Id,
            Email = "other@test.com",
            FirstName = "Updated",
            LastName = "Speaker",
        });

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Пользователь с таким Email уже существует");
    }

    [Fact]
    public async Task DeleteAsync_SetsIsDeleted()
    {
        var speaker = await TestDataSeeder.SeedUserAsync(DataContext, "speaker@test.com", "p", UserRoles.Speaker);
        var repo = new SpeakerRepository(DataContext, EmailSender, SmtpSettings, HtmlSanitizer);

        await repo.DeleteAsync(speaker.Id);

        var details = await DataContext.UserDetails.SingleAsync(ud => ud.UserId == speaker.Id);
        details.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_Throws_WhenSpeakerNotFound()
    {
        var repo = new SpeakerRepository(DataContext, EmailSender, SmtpSettings, HtmlSanitizer);

        Func<Task> act = async () => await repo.DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task SetOrderAsync_SetsOrderByIds()
    {
        var s1 = await TestDataSeeder.SeedUserAsync(DataContext, "s1@test.com", "p", UserRoles.Speaker, order: 0);
        var s2 = await TestDataSeeder.SeedUserAsync(DataContext, "s2@test.com", "p", UserRoles.Speaker, order: 0);
        var repo = new SpeakerRepository(DataContext, EmailSender, SmtpSettings, HtmlSanitizer);

        await repo.SetOrderAsync(new[] { s2.Id, s1.Id });

        var d1 = await DataContext.UserDetails.SingleAsync(ud => ud.UserId == s1.Id);
        var d2 = await DataContext.UserDetails.SingleAsync(ud => ud.UserId == s2.Id);
        d1.Order.Should().Be(1);
        d2.Order.Should().Be(0);
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenSpeakerNotFound()
    {
        var repo = new SpeakerRepository(DataContext, EmailSender, SmtpSettings, HtmlSanitizer);

        Func<Task> act = async () => await repo.UpdateAsync(new SpeakerUpdateDto
        {
            Id = Guid.NewGuid(),
            Email = "new@test.com",
            FirstName = "First",
            LastName = "Last",
        });

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenUserIsNotSpeaker()
    {
        var admin = await TestDataSeeder.SeedUserAsync(DataContext, "admin@test.com", "p", UserRoles.Administrator);
        var repo = new SpeakerRepository(DataContext, EmailSender, SmtpSettings, HtmlSanitizer);

        var result = await repo.GetByIdAsync(admin.Id);

        result.Should().BeNull();
    }

    [Fact]
    public async Task SetOrderAsync_IgnoresMissingIds()
    {
        var s1 = await TestDataSeeder.SeedUserAsync(DataContext, "s1@test.com", "p", UserRoles.Speaker, order: 0);
        var repo = new SpeakerRepository(DataContext, EmailSender, SmtpSettings, HtmlSanitizer);

        await repo.SetOrderAsync(new[] { Guid.NewGuid(), s1.Id });

        var details = await DataContext.UserDetails.SingleAsync(ud => ud.UserId == s1.Id);
        details.Order.Should().Be(1);
    }

    [Fact]
    public async Task CreateAsync_SanitizesInput()
    {
        var repo = new SpeakerRepository(DataContext, EmailSender, SmtpSettings, HtmlSanitizer);

        var result = await repo.CreateAsync(new SpeakerCreateDto
        {
            Email = "speaker@test.com",
            FirstName = "<script>bad</script>First",
            LastName = "<script>bad</script>Last",
            Patronymic = "<script>bad</script>Patronymic",
            Position = "<script>bad</script>Position",
            Order = 1,
        });

        result.FirstName.Should().NotContain("<script>");
        result.LastName.Should().NotContain("<script>");
        result.Patronymic.Should().NotContain("<script>");
        result.Position.Should().NotContain("<script>");
    }

    [Fact]
    public async Task UpdateAsync_SanitizesInput()
    {
        var speaker = await TestDataSeeder.SeedUserAsync(DataContext, "speaker@test.com", "p", UserRoles.Speaker);
        var repo = new SpeakerRepository(DataContext, EmailSender, SmtpSettings, HtmlSanitizer);

        var result = await repo.UpdateAsync(new SpeakerUpdateDto
        {
            Id = speaker.Id,
            Email = "updated@test.com",
            FirstName = "<script>bad</script>Updated",
            LastName = "Speaker",
            Patronymic = "<script>bad</script>Patronymic",
            Position = "<script>bad</script>Position",
            AdditionalInfo = "<script>bad</script>Info",
        });

        result.FirstName.Should().NotContain("<script>");
        result.Patronymic.Should().NotContain("<script>");
        result.Position.Should().NotContain("<script>");
        result.AdditionalInfo.Should().NotContain("<script>");
    }
}
