using AskQuestion.BLL.DTO.Question;
using AskQuestion.DAL;
using AskQuestion.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace AskQuestion.BLL.Repositories.Implementations
{
    public class QuestionRepository(DataContext dataContext) : IQuestionRepository
    {
        public async Task<IEnumerable<QuestionDto>> GetAllAsync()
        {
            IEnumerable<QuestionDto> questions = await dataContext.Questions
                .AsNoTracking()
                .Select(question => new QuestionDto
                {
                    Id = question.Id,
                    Text = question.Text,
                    Author = question.Author,
                    Area = question.Area,
                    Speaker = question.Speaker,
                    Likes = question.Likes,
                    Dislikes = question.Dislikes,
                    Views = question.Views,
                    Сreated = question.Сreated,
                    Answered = question.Answered
                })
                .ToListAsync();

            return questions;
        }

        public async Task<QuestionDto?> GetByIdAsync(Guid id)
        {
            Question? question = await dataContext.Questions
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == default)
            {
                return null;
            }

            QuestionDto questionDTO = new()
            {
                Id = question.Id,
                Text = question.Text,
                Author = question.Author,
                Area = question.Area,
                Speaker = question.Speaker,
                Likes = question.Likes,
                Dislikes = question.Dislikes,
                Views = question.Views,
                Сreated = question.Сreated,
                Answered = question.Answered
            };

            return questionDTO;
        }

        public async Task<Guid> CreateAsync(QuestionCreateDto questionCreateDto)
        {
            Question question = new()
            {
                Text = questionCreateDto.Text,
                Author = questionCreateDto.Author,
                Area = questionCreateDto.Area,
                Speaker = questionCreateDto.Speaker,
                Сreated = DateTimeOffset.UtcNow,
            };

            await dataContext.AddAsync(question);
            await dataContext.SaveChangesAsync();

            return question.Id;
        }

        public async Task UpdateAsync(Guid id, QuestionUpdateDto questionUpdateDto)
        {
            Question? question = await dataContext.Questions
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == default)
            {
                throw new InvalidOperationException("Объект не найден");
            }

            question.Text = questionUpdateDto.Text;
            question.Author = questionUpdateDto.Author;
            question.Area = questionUpdateDto.Area;
            question.Speaker = questionUpdateDto.Speaker;

            await dataContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            Question? question = await dataContext.Questions
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == default)
            {
                throw new InvalidOperationException("Объект не найден");
            }

            dataContext.Remove(question);
            await dataContext.SaveChangesAsync();
        }
    }
}
