using AskQuestion.BLL.DTO.Area;
using AskQuestion.BLL.Repositories.Implementations;
using AskQuestion.BLL.Tests.Helpers;
using FluentAssertions;

namespace AskQuestion.BLL.Tests.Repositories;

public class AreaRepositoryTests : RepositoryTestBase
{
    [Fact]
    public async Task GetAllAsync_ReturnsAreasOrderedByOrder()
    {
        await TestDataSeeder.SeedAreaAsync(DataContext, "Area B", order: 2);
        await TestDataSeeder.SeedAreaAsync(DataContext, "Area A", order: 1);
        var repo = new AreaRepository(DataContext, HtmlSanitizer);

        var result = (await repo.GetAllAsync()).ToList();

        result.Should().HaveCount(2);
        result[0].Title.Should().Be("Area A");
        result[1].Title.Should().Be("Area B");
    }

    [Fact]
    public async Task CreateAsync_SavesArea_AndSanitizesTitle()
    {
        var repo = new AreaRepository(DataContext, HtmlSanitizer);

        var result = await repo.CreateAsync(new AreaCreateDto
        {
            Title = "<script>bad</script>Area",
            Order = 1,
        });

        result.Title.Should().NotContain("<script>");
        var area = await DataContext.Areas.FindAsync(result.Id);
        area.Should().NotBeNull();
        area!.Title.Should().NotContain("<script>");
    }

    [Fact]
    public async Task UpdateAsync_UpdatesArea_AndSanitizesTitle()
    {
        var area = await TestDataSeeder.SeedAreaAsync(DataContext);
        var originalTitle = area.Title;
        var repo = new AreaRepository(DataContext, HtmlSanitizer);

        var result = await repo.UpdateAsync(new AreaUpdateDto
        {
            Id = area.Id,
            Title = "<script>bad</script>Updated",
        });

        result.Title.Should().NotContain("<script>");
        result.Title.Should().NotBe(originalTitle);
        var updated = await DataContext.Areas.FindAsync(area.Id);
        updated!.Title.Should().NotContain("<script>");
        updated.Title.Should().NotBe(originalTitle);
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenNotFound()
    {
        var repo = new AreaRepository(DataContext, HtmlSanitizer);

        Func<Task> act = async () => await repo.UpdateAsync(new AreaUpdateDto
        {
            Id = Guid.NewGuid(),
            Title = "Updated",
        });

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task DeleteAsync_RemovesArea()
    {
        var area = await TestDataSeeder.SeedAreaAsync(DataContext);
        var repo = new AreaRepository(DataContext, HtmlSanitizer);

        await repo.DeleteAsync(area.Id);

        var deleted = await DataContext.Areas.FindAsync(area.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_Throws_WhenNotFound()
    {
        var repo = new AreaRepository(DataContext, HtmlSanitizer);

        Func<Task> act = async () => await repo.DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task SetOrderAsync_SetsOrderByIds()
    {
        var a1 = await TestDataSeeder.SeedAreaAsync(DataContext, order: 0);
        var a2 = await TestDataSeeder.SeedAreaAsync(DataContext, order: 0);
        var repo = new AreaRepository(DataContext, HtmlSanitizer);

        await repo.SetOrderAsync(new[] { a2.Id, a1.Id });

        var updated1 = await DataContext.Areas.FindAsync(a1.Id);
        var updated2 = await DataContext.Areas.FindAsync(a2.Id);
        updated1!.Order.Should().Be(1);
        updated2!.Order.Should().Be(0);
    }

    [Fact]
    public async Task SetOrderAsync_IgnoresMissingIds()
    {
        var a1 = await TestDataSeeder.SeedAreaAsync(DataContext, order: 0);
        var repo = new AreaRepository(DataContext, HtmlSanitizer);

        await repo.SetOrderAsync(new[] { Guid.NewGuid(), a1.Id });

        var updated = await DataContext.Areas.FindAsync(a1.Id);
        updated!.Order.Should().Be(1);
    }
}
