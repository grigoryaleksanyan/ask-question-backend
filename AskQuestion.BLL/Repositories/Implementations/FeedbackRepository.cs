using AskQuestion.BLL.DTO.Feedback;
using AskQuestion.BLL.Repositories.Interfaces;
using AskQuestion.DAL;
using AskQuestion.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace AskQuestion.BLL.Repositories.Implementations
{
    public class FeedbackRepository(DataContext dataContext) : IFeedbackRepository
    {
        public async Task<IEnumerable<FeedbackDto>> GetAllAsync()
        {
            IEnumerable<FeedbackDto> feedbackDtos = await dataContext.Feedback
               .AsNoTracking()
               .OrderBy(feedback => feedback.Сreated)
               .Select(feedback => new FeedbackDto
               {
                   Id = feedback.Id,
                   Username = feedback.Username,
                   Email = feedback.Email,
                   Theme = feedback.Theme,
                   Text = feedback.Text,
                   Сreated = feedback.Сreated
               })
               .ToListAsync();

            return feedbackDtos;
        }

        public async Task<Guid> CreateAsync(FeedbackCreateDto feedbackCreateDto)
        {
            Feedback feedback = new()
            {
                Username = feedbackCreateDto.Username,
                Email = feedbackCreateDto.Email,
                Theme = feedbackCreateDto.Theme,
                Text = feedbackCreateDto.Text,
                Сreated = DateTimeOffset.UtcNow,
            };

            await dataContext.AddAsync(feedback);
            await dataContext.SaveChangesAsync();

            return feedback.Id;
        }

        public async Task DeleteAsync(Guid id)
        {
            Feedback? feedback = await dataContext.Feedback
                .FirstOrDefaultAsync(q => q.Id == id);

            if (feedback == default)
            {
                throw new InvalidOperationException("Объект не найден");
            }

            dataContext.Remove(feedback);
            await dataContext.SaveChangesAsync();
        }
    }
}
