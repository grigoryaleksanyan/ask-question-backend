using AskQuestion.BLL.Repositories.Interfaces;
using AskQuestion.DAL;
using AskQuestion.DAL.Entities;

namespace AskQuestion.BLL.Repositories.Implementations
{
    public class QuestionStatusTransitionRepository(DataContext dataContext) : IQuestionStatusTransitionRepository
    {
        public async Task AddAsync(QuestionStatusTransition transition)
        {
            await dataContext.QuestionStatusTransitions.AddAsync(transition);
            await dataContext.SaveChangesAsync();
        }
    }
}
