using AskQuestion.BLL.DTO.Feedback;
using AskQuestion.BLL.Repositories.Implementations;
using AskQuestion.BLL.Tests.Helpers;
using AskQuestion.DAL.Entities;
using FluentAssertions;

namespace AskQuestion.BLL.Tests.Repositories;

public class FeedbackRepositoryTests : RepositoryTestBase
{
    [Fact]
    public async Task GetAllAsync_ReturnsFeedbackOrderedByCreated()
    {
        var f1 = new Feedback
        {
            Id = Guid.NewGuid(),
            Username = "User1",
            Email = "u1@test.com",
            Theme = "Theme1",
            Text = "Text1",
            Created = DateTimeOffset.UtcNow.AddMinutes(-1),
        };
        var f2 = new Feedback
        {
            Id = Guid.NewGuid(),
            Username = "User2",
            Email = "u2@test.com",
            Theme = "Theme2",
            Text = "Text2",
            Created = DateTimeOffset.UtcNow,
        };
        await DataContext.Feedback.AddRangeAsync(f1, f2);
        await DataContext.SaveChangesAsync();
        var repo = new FeedbackRepository(DataContext, HtmlSanitizer);

        var result = (await repo.GetAllAsync()).ToList();

        result.Should().HaveCount(2);
        result[0].Id.Should().Be(f1.Id);
        result[1].Id.Should().Be(f2.Id);
    }

    [Fact]
    public async Task CreateAsync_SavesFeedback_AndSanitizes()
    {
        var repo = new FeedbackRepository(DataContext, HtmlSanitizer);

        var id = await repo.CreateAsync(new FeedbackCreateDto
        {
            Username = "<script>bad</script>User",
            Email = "user@test.com",
            Theme = "<script>bad</script>Theme",
            Text = "<script>bad</script>Text",
        });

        var feedback = await DataContext.Feedback.FindAsync(id);
        feedback.Should().NotBeNull();
        feedback!.Username.Should().NotContain("<script>");
        feedback.Theme.Should().NotContain("<script>");
        feedback.Text.Should().NotContain("<script>");
        feedback.Email.Should().Be("user@test.com");
    }

    [Fact]
    public async Task CreateAsync_ReturnsId()
    {
        var repo = new FeedbackRepository(DataContext, HtmlSanitizer);

        var id = await repo.CreateAsync(new FeedbackCreateDto
        {
            Username = "User",
            Email = "user@test.com",
            Theme = "Theme",
            Text = "Text",
        });

        var feedback = await DataContext.Feedback.FindAsync(id);
        feedback.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteAsync_RemovesFeedback()
    {
        var feedback = await TestDataSeeder.SeedFeedbackAsync(DataContext);
        var repo = new FeedbackRepository(DataContext, HtmlSanitizer);

        await repo.DeleteAsync(feedback.Id);

        var deleted = await DataContext.Feedback.FindAsync(feedback.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_Throws_WhenNotFound()
    {
        var repo = new FeedbackRepository(DataContext, HtmlSanitizer);

        Func<Task> act = async () => await repo.DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
