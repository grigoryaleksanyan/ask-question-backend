using AskQuestion.DAL.Entities;

namespace AskQuestion.BLL.Repositories.Interfaces
{
    public interface IQuestionStatusTransitionRepository
    {
        Task AddAsync(QuestionStatusTransition transition);
    }
}
