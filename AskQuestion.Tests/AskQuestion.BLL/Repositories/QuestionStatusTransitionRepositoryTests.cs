using AskQuestion.BLL.Repositories.Implementations;
using AskQuestion.BLL.Tests.Helpers;
using AskQuestion.Core.Enums;
using AskQuestion.DAL.Entities;
using FluentAssertions;

namespace AskQuestion.BLL.Tests.Repositories;

public class QuestionStatusTransitionRepositoryTests : RepositoryTestBase
{
    [Fact]
    public async Task AddAsync_SavesTransition()
    {
        var question = await TestDataSeeder.SeedQuestionAsync(DataContext);
        var repo = new QuestionStatusTransitionRepository(DataContext);

        await repo.AddAsync(new QuestionStatusTransition
        {
            QuestionId = question.Id,
            FromStatus = (int)QuestionStatus.New,
            ToStatus = (int)QuestionStatus.InFocus,
        });

        DataContext.QuestionStatusTransitions.Should().ContainSingle(t =>
            t.QuestionId == question.Id &&
            t.FromStatus == (int)QuestionStatus.New &&
            t.ToStatus == (int)QuestionStatus.InFocus);
    }
}
