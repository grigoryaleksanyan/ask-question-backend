using AskQuestion.BLL.DTO.FaqCategory;
using AskQuestion.BLL.Repositories.Implementations;
using AskQuestion.BLL.Tests.Helpers;
using FluentAssertions;

namespace AskQuestion.BLL.Tests.Repositories;

public class FaqCategoryRepositoryTests : RepositoryTestBase
{
    [Fact]
    public async Task GetAllAsync_ReturnsCategoriesOrderedByOrder()
    {
        await TestDataSeeder.SeedFaqCategoryAsync(DataContext, "Category B", order: 2);
        await TestDataSeeder.SeedFaqCategoryAsync(DataContext, "Category A", order: 1);
        var repo = new FaqCategoryRepository(DataContext, HtmlSanitizer);

        var result = (await repo.GetAllAsync()).ToList();

        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Category A");
        result[1].Name.Should().Be("Category B");
    }

    [Fact]
    public async Task GetAllWithEntriesAsync_ReturnsOnlyCategoriesWithEntries()
    {
        var withEntries = await TestDataSeeder.SeedFaqCategoryAsync(DataContext, "With Entries");
        var empty = await TestDataSeeder.SeedFaqCategoryAsync(DataContext, "Empty");
        await TestDataSeeder.SeedFaqEntryAsync(DataContext, withEntries.Id);
        var repo = new FaqCategoryRepository(DataContext, HtmlSanitizer);

        var result = (await repo.GetAllWithEntriesAsync()).ToList();

        result.Should().ContainSingle(c => c.Name == "With Entries");
        result.Should().NotContain(c => c.Name == "Empty");
    }

    [Fact]
    public async Task GetAllWithEntriesForAdminAsync_ReturnsAllCategories()
    {
        var withEntries = await TestDataSeeder.SeedFaqCategoryAsync(DataContext, "With Entries");
        var empty = await TestDataSeeder.SeedFaqCategoryAsync(DataContext, "Empty");
        await TestDataSeeder.SeedFaqEntryAsync(DataContext, withEntries.Id);
        var repo = new FaqCategoryRepository(DataContext, HtmlSanitizer);

        var result = (await repo.GetAllWithEntriesForAdminAsync()).ToList();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCategoryWithEntries()
    {
        var category = await TestDataSeeder.SeedFaqCategoryAsync(DataContext);
        var entry = await TestDataSeeder.SeedFaqEntryAsync(DataContext, category.Id);
        var repo = new FaqCategoryRepository(DataContext, HtmlSanitizer);

        var result = await repo.GetByIdAsync(category.Id);

        result.Should().NotBeNull();
        result!.Entries.Should().ContainSingle(e => e.Id == entry.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        var repo = new FaqCategoryRepository(DataContext, HtmlSanitizer);

        var result = await repo.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_SavesCategory_AndSanitizesName()
    {
        var repo = new FaqCategoryRepository(DataContext, HtmlSanitizer);

        var result = await repo.CreateAsync(new FaqCategoryCreateDto
        {
            Name = "<script>bad</script>Category",
            Order = 1,
        });

        result.Name.Should().NotContain("<script>");
        var category = await DataContext.FaqCategories.FindAsync(result.Id);
        category.Should().NotBeNull();
        category!.Name.Should().NotContain("<script>");
    }

    [Fact]
    public async Task UpdateAsync_UpdatesCategory_AndSanitizesName()
    {
        var category = await TestDataSeeder.SeedFaqCategoryAsync(DataContext);
        var originalName = category.Name;
        var repo = new FaqCategoryRepository(DataContext, HtmlSanitizer);

        var result = await repo.UpdateAsync(new FaqCategoryUpdateDto
        {
            Id = category.Id,
            Name = "<script>bad</script>Updated",
        });

        result.Name.Should().NotContain("<script>");
        result.Name.Should().NotBe(originalName);
        var updated = await DataContext.FaqCategories.FindAsync(category.Id);
        updated!.Name.Should().NotContain("<script>");
        updated.Name.Should().NotBe(originalName);
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenNotFound()
    {
        var repo = new FaqCategoryRepository(DataContext, HtmlSanitizer);

        Func<Task> act = async () => await repo.UpdateAsync(new FaqCategoryUpdateDto
        {
            Id = Guid.NewGuid(),
            Name = "Updated",
        });

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task DeleteAsync_RemovesCategory()
    {
        var category = await TestDataSeeder.SeedFaqCategoryAsync(DataContext);
        var repo = new FaqCategoryRepository(DataContext, HtmlSanitizer);

        await repo.DeleteAsync(category.Id);

        var deleted = await DataContext.FaqCategories.FindAsync(category.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_Throws_WhenNotFound()
    {
        var repo = new FaqCategoryRepository(DataContext, HtmlSanitizer);

        Func<Task> act = async () => await repo.DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task SetOrderAsync_SetsOrderByIds()
    {
        var c1 = await TestDataSeeder.SeedFaqCategoryAsync(DataContext, order: 0);
        var c2 = await TestDataSeeder.SeedFaqCategoryAsync(DataContext, order: 0);
        var repo = new FaqCategoryRepository(DataContext, HtmlSanitizer);

        await repo.SetOrderAsync(new[] { c2.Id, c1.Id });

        var updated1 = await DataContext.FaqCategories.FindAsync(c1.Id);
        var updated2 = await DataContext.FaqCategories.FindAsync(c2.Id);
        updated1!.Order.Should().Be(1);
        updated2!.Order.Should().Be(0);
    }
}
