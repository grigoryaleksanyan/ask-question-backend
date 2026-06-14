using AskQuestion.BLL.DTO.Question;
using AskQuestion.BLL.Repositories.Implementations;
using AskQuestion.BLL.Tests.Helpers;
using AskQuestion.Core.Enums;
using FluentAssertions;

namespace AskQuestion.BLL.Tests.Repositories;

public class QuestionRepositoryTests : RepositoryTestBase
{
    [Fact]
    public async Task CreateAsync_SavesQuestion_AndSanitizesText()
    {
        var repo = new QuestionRepository(DataContext, EmailSender, SmtpSettings, HtmlSanitizer);

        var id = await repo.CreateAsync(new QuestionCreateDto
        {
            Text = "<script>alert('xss')</script>Hello",
            Author = "<script>bad</script>Author",
        });

        var question = await DataContext.Questions.FindAsync(id);
        question.Should().NotBeNull();
        question!.Text.Should().NotContain("<script>");
        question.Author.Should().NotContain("<script>");
    }

    [Fact]
    public async Task CreateAsync_EnqueuesEmail_WhenSpeakerSpecified()
    {
        var speaker = await TestDataSeeder.SeedUserAsync(DataContext, "speaker@test.com", "Password1", UserRoles.Speaker);
        var repo = new QuestionRepository(DataContext, EmailSender, SmtpSettings, HtmlSanitizer);

        await repo.CreateAsync(new QuestionCreateDto
        {
            Text = "Question",
            SpeakerId = speaker.Id,
        });

        EmailSender.Messages.Should().ContainSingle();
        EmailSender.Messages[0].ToEmail.Should().Be("speaker@test.com");
    }

    [Fact]
    public async Task GetAllAsync_AppliesPagination()
    {
        var repo = new QuestionRepository(DataContext, EmailSender, SmtpSettings, HtmlSanitizer);
        for (int i = 0; i < 5; i++)
        {
            await TestDataSeeder.SeedQuestionAsync(DataContext, $"Question {i}");
        }

        var result = await repo.GetAllAsync(page: 1, pageSize: 2);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(5);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(2);
    }

    [Fact]
    public async Task GetAllAsync_ClampsPageSize_ToMaximum50()
    {
        var repo = new QuestionRepository(DataContext, EmailSender, SmtpSettings, HtmlSanitizer);
        await TestDataSeeder.SeedQuestionAsync(DataContext);

        var result = await repo.GetAllAsync(pageSize: 100);

        result.PageSize.Should().Be(50);
    }

    [Fact]
    public async Task GetAllAsync_ClampsPage_ToMinimum1()
    {
        var repo = new QuestionRepository(DataContext, EmailSender, SmtpSettings, HtmlSanitizer);
        await TestDataSeeder.SeedQuestionAsync(DataContext);

        var result = await repo.GetAllAsync(page: -1);

        result.Page.Should().Be(1);
    }

    [Fact]
    public async Task GetAllAsync_FiltersByStatus()
    {
        var repo = new QuestionRepository(DataContext, EmailSender, SmtpSettings, HtmlSanitizer);
        await TestDataSeeder.SeedQuestionAsync(DataContext, status: QuestionStatus.New);
        await TestDataSeeder.SeedQuestionAsync(DataContext, status: QuestionStatus.Answered);

        var result = await repo.GetAllAsync(status: (int)QuestionStatus.New);

        result.Items.Should().ContainSingle(q => q.Status == (int)QuestionStatus.New);
    }

    [Fact]
    public async Task GetAllAsync_FiltersBySpeakerId()
    {
        var speaker = await TestDataSeeder.SeedUserAsync(DataContext, "speaker@test.com", "Password1", UserRoles.Speaker);
        var repo = new QuestionRepository(DataContext, EmailSender, SmtpSettings, HtmlSanitizer);
        await TestDataSeeder.SeedQuestionAsync(DataContext, speakerId: speaker.Id);
        await TestDataSeeder.SeedQuestionAsync(DataContext);

        var result = await repo.GetAllAsync(speakerId: speaker.Id);

        result.Items.Should().ContainSingle(q => q.SpeakerId == speaker.Id);
    }

    [Fact]
    public async Task GetAllAsync_FiltersBySearch()
    {
        var repo = new QuestionRepository(DataContext, EmailSender, SmtpSettings, HtmlSanitizer);
        await TestDataSeeder.SeedQuestionAsync(DataContext, text: "Unique search term");
        await TestDataSeeder.SeedQuestionAsync(DataContext, text: "Another");

        var result = await repo.GetAllAsync(search: "unique");

        result.Items.Should().ContainSingle(q => q.Text.Contains("Unique"));
    }

    [Fact]
    public async Task GetPopularQuestionsAsync_ReturnsTop5_ByLikes()
    {
        var repo = new QuestionRepository(DataContext, EmailSender, SmtpSettings, HtmlSanitizer);
        await TestDataSeeder.SeedQuestionAsync(DataContext, likes: 10);
        await TestDataSeeder.SeedQuestionAsync(DataContext, likes: 20);
        await TestDataSeeder.SeedQuestionAsync(DataContext, likes: 30);
        await TestDataSeeder.SeedQuestionAsync(DataContext, likes: 40);
        await TestDataSeeder.SeedQuestionAsync(DataContext, likes: 50);
        await TestDataSeeder.SeedQuestionAsync(DataContext, likes: 60);

        var result = await repo.GetPopularQuestionsAsync();

        result.Should().HaveCount(5);
        result.First().Likes.Should().Be(60);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsQuestion_WhenExists()
    {
        var question = await TestDataSeeder.SeedQuestionAsync(DataContext, text: "Specific");
        var repo = new QuestionRepository(DataContext, EmailSender, SmtpSettings, HtmlSanitizer);

        var result = await repo.GetByIdAsync(question.Id);

        result.Should().NotBeNull();
        result!.Text.Should().Be("Specific");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
    {
        var repo = new QuestionRepository(DataContext, EmailSender, SmtpSettings, HtmlSanitizer);

        var result = await repo.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task IncrementViewsAsync_IncrementsViews()
    {
        var question = await TestDataSeeder.SeedQuestionAsync(DataContext, views: 5);
        var repo = new QuestionRepository(DataContext, EmailSender, SmtpSettings, HtmlSanitizer);

        await repo.IncrementViewsAsync(question.Id);

        var updated = await DataContext.Questions.FindAsync(question.Id);
        updated!.Views.Should().Be(6);
    }

    [Fact]
    public async Task ToggleLikeAsync_AddsLike_WhenNoVote()
    {
        var question = await TestDataSeeder.SeedQuestionAsync(DataContext);
        var visitor = Guid.NewGuid();
        var repo = new QuestionRepository(DataContext, EmailSender, SmtpSettings, HtmlSanitizer);

        var result = await repo.ToggleLikeAsync(question.Id, visitor);

        result.Likes.Should().Be(1);
        result.Dislikes.Should().Be(0);
        result.UserVote.Should().Be(VoteType.Like);
    }

    [Fact]
    public async Task ToggleLikeAsync_RemovesLike_WhenAlreadyLiked()
    {
        var question = await TestDataSeeder.SeedQuestionAsync(DataContext);
        var visitor = Guid.NewGuid();
        var repo = new QuestionRepository(DataContext, EmailSender, SmtpSettings, HtmlSanitizer);
        await repo.ToggleLikeAsync(question.Id, visitor);

        var result = await repo.ToggleLikeAsync(question.Id, visitor);

        result.Likes.Should().Be(0);
        result.UserVote.Should().BeNull();
    }

    [Fact]
    public async Task ToggleDislikeAsync_ChangesLikeToDislike()
    {
        var question = await TestDataSeeder.SeedQuestionAsync(DataContext);
        var visitor = Guid.NewGuid();
        var repo = new QuestionRepository(DataContext, EmailSender, SmtpSettings, HtmlSanitizer);
        await repo.ToggleLikeAsync(question.Id, visitor);

        var result = await repo.ToggleDislikeAsync(question.Id, visitor);

        result.Likes.Should().Be(0);
        result.Dislikes.Should().Be(1);
        result.UserVote.Should().Be(VoteType.Dislike);
    }

    [Fact]
    public async Task ChangeStatusAsync_ChangesStatus_AndLogsTransition()
    {
        var question = await TestDataSeeder.SeedQuestionAsync(DataContext, status: QuestionStatus.New);
        var repo = new QuestionRepository(DataContext, EmailSender, SmtpSettings, HtmlSanitizer);

        await repo.ChangeStatusAsync(new QuestionStatusChangeDto
        {
            QuestionId = question.Id,
            NewStatus = (int)QuestionStatus.InFocus,
        });

        var updated = await DataContext.Questions.FindAsync(question.Id);
        updated!.Status.Should().Be((int)QuestionStatus.InFocus);
        updated.Answered.Should().BeNull();
        DataContext.QuestionStatusTransitions.Should().ContainSingle(t => t.ToStatus == (int)QuestionStatus.InFocus);
    }

    [Fact]
    public async Task ChangeStatusAsync_SetsAnswered_WhenStatusAnswered()
    {
        var question = await TestDataSeeder.SeedQuestionAsync(DataContext, status: QuestionStatus.InFocus);
        var repo = new QuestionRepository(DataContext, EmailSender, SmtpSettings, HtmlSanitizer);

        await repo.ChangeStatusAsync(new QuestionStatusChangeDto
        {
            QuestionId = question.Id,
            NewStatus = (int)QuestionStatus.Answered,
        });

        var updated = await DataContext.Questions.FindAsync(question.Id);
        updated!.Status.Should().Be((int)QuestionStatus.Answered);
        updated.Answered.Should().NotBeNull();
    }

    [Fact]
    public async Task ChangeStatusAsync_ResetsAnswered_WhenLeavingAnswered()
    {
        var question = await TestDataSeeder.SeedQuestionAsync(DataContext, status: QuestionStatus.Answered);
        question.Answered = DateTimeOffset.UtcNow;
        await DataContext.SaveChangesAsync();
        var repo = new QuestionRepository(DataContext, EmailSender, SmtpSettings, HtmlSanitizer);

        await repo.ChangeStatusAsync(new QuestionStatusChangeDto
        {
            QuestionId = question.Id,
            NewStatus = (int)QuestionStatus.InFocus,
        });

        var updated = await DataContext.Questions.FindAsync(question.Id);
        updated!.Status.Should().Be((int)QuestionStatus.InFocus);
        updated.Answered.Should().BeNull();
    }

    [Fact]
    public async Task ChangeStatusAsync_Throws_WhenInvalidTransition()
    {
        var question = await TestDataSeeder.SeedQuestionAsync(DataContext, status: QuestionStatus.New);
        var repo = new QuestionRepository(DataContext, EmailSender, SmtpSettings, HtmlSanitizer);

        Func<Task> act = async () => await repo.ChangeStatusAsync(new QuestionStatusChangeDto
        {
            QuestionId = question.Id,
            NewStatus = (int)QuestionStatus.Answered,
        });

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ChangeStatusAsync_Throws_WhenSameStatus()
    {
        var question = await TestDataSeeder.SeedQuestionAsync(DataContext, status: QuestionStatus.New);
        var repo = new QuestionRepository(DataContext, EmailSender, SmtpSettings, HtmlSanitizer);

        Func<Task> act = async () => await repo.ChangeStatusAsync(new QuestionStatusChangeDto
        {
            QuestionId = question.Id,
            NewStatus = (int)QuestionStatus.New,
        });

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task SetCommentAsync_SanitizesComment()
    {
        var question = await TestDataSeeder.SeedQuestionAsync(DataContext);
        var repo = new QuestionRepository(DataContext, EmailSender, SmtpSettings, HtmlSanitizer);

        await repo.SetCommentAsync(new QuestionCommentDto
        {
            QuestionId = question.Id,
            Comment = "<script>bad</script>Ok",
        });

        var updated = await DataContext.Questions.FindAsync(question.Id);
        updated!.Comment.Should().NotContain("<script>");
    }

    [Fact]
    public async Task UpdateAsync_UpdatesQuestion_AndSanitizesText()
    {
        var question = await TestDataSeeder.SeedQuestionAsync(DataContext);
        var area = await TestDataSeeder.SeedAreaAsync(DataContext, "Area");
        var repo = new QuestionRepository(DataContext, EmailSender, SmtpSettings, HtmlSanitizer);

        var result = await repo.UpdateAsync(question.Id, new QuestionUpdateDto
        {
            Id = question.Id,
            Text = "<script>x</script>Updated",
            Author = "Updated Author",
            AreaId = area.Id,
        });

        result.Text.Should().NotContain("<script>");
        result.AreaId.Should().Be(area.Id);
    }

    [Fact]
    public async Task DeleteAsync_RemovesQuestion()
    {
        var question = await TestDataSeeder.SeedQuestionAsync(DataContext);
        var repo = new QuestionRepository(DataContext, EmailSender, SmtpSettings, HtmlSanitizer);

        await repo.DeleteAsync(question.Id);

        var deleted = await DataContext.Questions.FindAsync(question.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task GetUserVoteAsync_ReturnsVote_WhenExists()
    {
        var question = await TestDataSeeder.SeedQuestionAsync(DataContext);
        var visitor = Guid.NewGuid();
        var repo = new QuestionRepository(DataContext, EmailSender, SmtpSettings, HtmlSanitizer);
        await repo.ToggleLikeAsync(question.Id, visitor);

        var result = await repo.GetUserVoteAsync(question.Id, visitor);

        result.Should().Be(VoteType.Like);
    }

    [Fact]
    public async Task GetUserVoteAsync_ReturnsNull_WhenNotExists()
    {
        var question = await TestDataSeeder.SeedQuestionAsync(DataContext);
        var visitor = Guid.NewGuid();
        var repo = new QuestionRepository(DataContext, EmailSender, SmtpSettings, HtmlSanitizer);

        var result = await repo.GetUserVoteAsync(question.Id, visitor);

        result.Should().BeNull();
    }

    [Fact]
    public async Task ToggleLikeAsync_ChangesDislikeToLike()
    {
        var question = await TestDataSeeder.SeedQuestionAsync(DataContext);
        var visitor = Guid.NewGuid();
        var repo = new QuestionRepository(DataContext, EmailSender, SmtpSettings, HtmlSanitizer);
        await repo.ToggleDislikeAsync(question.Id, visitor);

        var result = await repo.ToggleLikeAsync(question.Id, visitor);

        result.Likes.Should().Be(1);
        result.Dislikes.Should().Be(0);
        result.UserVote.Should().Be(VoteType.Like);
    }

    [Fact]
    public async Task ToggleDislikeAsync_AddsDislike_WhenNoVote()
    {
        var question = await TestDataSeeder.SeedQuestionAsync(DataContext);
        var visitor = Guid.NewGuid();
        var repo = new QuestionRepository(DataContext, EmailSender, SmtpSettings, HtmlSanitizer);

        var result = await repo.ToggleDislikeAsync(question.Id, visitor);

        result.Likes.Should().Be(0);
        result.Dislikes.Should().Be(1);
        result.UserVote.Should().Be(VoteType.Dislike);
    }

    [Fact]
    public async Task ToggleDislikeAsync_RemovesDislike_WhenAlreadyDisliked()
    {
        var question = await TestDataSeeder.SeedQuestionAsync(DataContext);
        var visitor = Guid.NewGuid();
        var repo = new QuestionRepository(DataContext, EmailSender, SmtpSettings, HtmlSanitizer);
        await repo.ToggleDislikeAsync(question.Id, visitor);

        var result = await repo.ToggleDislikeAsync(question.Id, visitor);

        result.Likes.Should().Be(0);
        result.Dislikes.Should().Be(0);
        result.UserVote.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_FiltersByAreaId()
    {
        var area = await TestDataSeeder.SeedAreaAsync(DataContext, "Area");
        var repo = new QuestionRepository(DataContext, EmailSender, SmtpSettings, HtmlSanitizer);
        await TestDataSeeder.SeedQuestionAsync(DataContext, areaId: area.Id);
        await TestDataSeeder.SeedQuestionAsync(DataContext);

        var result = await repo.GetAllAsync(areaId: area.Id);

        result.Items.Should().ContainSingle(q => q.AreaId == area.Id);
    }

    [Fact]
    public async Task GetAllAsync_SortsAscending()
    {
        var repo = new QuestionRepository(DataContext, EmailSender, SmtpSettings, HtmlSanitizer);
        var q1 = await TestDataSeeder.SeedQuestionAsync(DataContext, text: "First");
        var q2 = await TestDataSeeder.SeedQuestionAsync(DataContext, text: "Second");
        q1.Created = DateTimeOffset.UtcNow.AddMinutes(-1);
        q2.Created = DateTimeOffset.UtcNow;
        await DataContext.SaveChangesAsync();

        var result = await repo.GetAllAsync(sortOrder: "asc");

        result.Items.First().Text.Should().Be("First");
        result.Items.Last().Text.Should().Be("Second");
    }

    [Fact]
    public async Task CreateAsync_DoesNotEnqueueEmail_WhenNoSpeaker()
    {
        var repo = new QuestionRepository(DataContext, EmailSender, SmtpSettings, HtmlSanitizer);

        await repo.CreateAsync(new QuestionCreateDto
        {
            Text = "Question",
            Author = "Author",
        });

        EmailSender.Messages.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenNotFound()
    {
        var repo = new QuestionRepository(DataContext, EmailSender, SmtpSettings, HtmlSanitizer);
        var area = await TestDataSeeder.SeedAreaAsync(DataContext);

        Func<Task> act = async () => await repo.UpdateAsync(Guid.NewGuid(), new QuestionUpdateDto
        {
            Id = Guid.NewGuid(),
            Text = "Updated",
            Author = "Author",
            AreaId = area.Id,
        });

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task DeleteAsync_Throws_WhenNotFound()
    {
        var repo = new QuestionRepository(DataContext, EmailSender, SmtpSettings, HtmlSanitizer);

        Func<Task> act = async () => await repo.DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task IncrementViewsAsync_DoesNothing_WhenNotFound()
    {
        var repo = new QuestionRepository(DataContext, EmailSender, SmtpSettings, HtmlSanitizer);

        await repo.IncrementViewsAsync(Guid.NewGuid());

        DataContext.Questions.Should().BeEmpty();
    }

    [Fact]
    public async Task SetCommentAsync_Throws_WhenNotFound()
    {
        var repo = new QuestionRepository(DataContext, EmailSender, SmtpSettings, HtmlSanitizer);

        Func<Task> act = async () => await repo.SetCommentAsync(new QuestionCommentDto
        {
            QuestionId = Guid.NewGuid(),
            Comment = "Comment",
        });

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
