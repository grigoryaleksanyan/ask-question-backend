using AskQuestion.BLL.DTO.FaqEntry;
using AskQuestion.BLL.Repositories.Implementations;
using AskQuestion.BLL.Tests.Helpers;
using FluentAssertions;

namespace AskQuestion.BLL.Tests.Repositories;

public class FaqEntryRepositoryTests : RepositoryTestBase
{
    [Fact]
    public async Task GetAllAsync_ReturnsEntriesOrderedByOrder()
    {
        var category = await TestDataSeeder.SeedFaqCategoryAsync(DataContext);
        await TestDataSeeder.SeedFaqEntryAsync(DataContext, category.Id, order: 2);
        await TestDataSeeder.SeedFaqEntryAsync(DataContext, category.Id, order: 1);
        var repo = new FaqEntryRepository(DataContext, HtmlSanitizer);

        var result = (await repo.GetAllAsync()).ToList();

        result.Should().HaveCount(2);
        result[0].Order.Should().Be(1);
        result[1].Order.Should().Be(2);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsEntry()
    {
        var category = await TestDataSeeder.SeedFaqCategoryAsync(DataContext);
        var entry = await TestDataSeeder.SeedFaqEntryAsync(DataContext, category.Id, question: "Specific");
        var repo = new FaqEntryRepository(DataContext, HtmlSanitizer);

        var result = await repo.GetByIdAsync(entry.Id);

        result.Should().NotBeNull();
        result!.Question.Should().Be("Specific");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        var repo = new FaqEntryRepository(DataContext, HtmlSanitizer);

        var result = await repo.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_SavesEntry_AndSanitizesQuestionAndAnswer()
    {
        var category = await TestDataSeeder.SeedFaqCategoryAsync(DataContext);
        var repo = new FaqEntryRepository(DataContext, HtmlSanitizer);

        var result = await repo.CreateAsync(new FaqEntryCreateDto
        {
            FaqCategoryId = category.Id,
            Question = "<script>bad</script>Q",
            Answer = "<script>bad</script>A",
            Order = 1,
        });

        result.Question.Should().NotContain("<script>");
        result.Answer.Should().NotContain("<script>");
        var entry = await DataContext.FaqEntries.FindAsync(result.Id);
        entry.Should().NotBeNull();
        entry!.Question.Should().NotContain("<script>");
        entry.Answer.Should().NotContain("<script>");
    }

    [Fact]
    public async Task UpdateAsync_UpdatesEntry_AndSanitizes()
    {
        var category = await TestDataSeeder.SeedFaqCategoryAsync(DataContext);
        var entry = await TestDataSeeder.SeedFaqEntryAsync(DataContext, category.Id);
        var originalQuestion = entry.Question;
        var repo = new FaqEntryRepository(DataContext, HtmlSanitizer);

        var result = await repo.UpdateAsync(new FaqEntryUpdateDto
        {
            Id = entry.Id,
            Question = "<script>bad</script>Updated Q",
            Answer = "<script>bad</script>Updated A",
        });

        result.Question.Should().NotContain("<script>");
        result.Answer.Should().NotContain("<script>");
        result.Question.Should().NotBe(originalQuestion);
        var updated = await DataContext.FaqEntries.FindAsync(entry.Id);
        updated!.Question.Should().NotContain("<script>");
        updated.Answer.Should().NotContain("<script>");
        updated.Question.Should().NotBe(originalQuestion);
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenNotFound()
    {
        var repo = new FaqEntryRepository(DataContext, HtmlSanitizer);

        Func<Task> act = async () => await repo.UpdateAsync(new FaqEntryUpdateDto
        {
            Id = Guid.NewGuid(),
            Question = "Q",
            Answer = "A",
        });

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task DeleteAsync_RemovesEntry()
    {
        var category = await TestDataSeeder.SeedFaqCategoryAsync(DataContext);
        var entry = await TestDataSeeder.SeedFaqEntryAsync(DataContext, category.Id);
        var repo = new FaqEntryRepository(DataContext, HtmlSanitizer);

        await repo.DeleteAsync(entry.Id);

        var deleted = await DataContext.FaqEntries.FindAsync(entry.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_Throws_WhenNotFound()
    {
        var repo = new FaqEntryRepository(DataContext, HtmlSanitizer);

        Func<Task> act = async () => await repo.DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task SetOrderAsync_SetsOrderByIds()
    {
        var category = await TestDataSeeder.SeedFaqCategoryAsync(DataContext);
        var e1 = await TestDataSeeder.SeedFaqEntryAsync(DataContext, category.Id, order: 0);
        var e2 = await TestDataSeeder.SeedFaqEntryAsync(DataContext, category.Id, order: 0);
        var repo = new FaqEntryRepository(DataContext, HtmlSanitizer);

        await repo.SetOrderAsync(new[] { e2.Id, e1.Id });

        var updated1 = await DataContext.FaqEntries.FindAsync(e1.Id);
        var updated2 = await DataContext.FaqEntries.FindAsync(e2.Id);
        updated1!.Order.Should().Be(1);
        updated2!.Order.Should().Be(0);
    }

    [Fact]
    public async Task SetOrderAsync_IgnoresMissingIds()
    {
        var category = await TestDataSeeder.SeedFaqCategoryAsync(DataContext);
        var e1 = await TestDataSeeder.SeedFaqEntryAsync(DataContext, category.Id, order: 0);
        var repo = new FaqEntryRepository(DataContext, HtmlSanitizer);

        await repo.SetOrderAsync(new[] { Guid.NewGuid(), e1.Id });

        var updated = await DataContext.FaqEntries.FindAsync(e1.Id);
        updated!.Order.Should().Be(1);
    }
}
