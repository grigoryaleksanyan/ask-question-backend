using AskQuestion.BLL.DTO.Question;
using AskQuestion.DAL;
using AskQuestion.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace AskQuestion.BLL.Repositories.Implementations
{
    public class QuestionRepository : IQuestionRepository
    {
        private readonly DataContext _dataContext;

        public QuestionRepository(DataContext dataContext)
        {
            _dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
        }

        public async Task<IEnumerable<QuestionDto>> GetAllAsync()
        {
            IEnumerable<QuestionDto> questions = await _dataContext.Questions
                .AsNoTracking()
                .Select(question => new QuestionDto
                {
                    Id = question.Id,
                    Author = question.Author,
                    Speaker = question.Speaker,
                    Text = question.Text,
                    Likes = question.Likes,
                    Dislikes = question.Dislikes,
                    Views = question.Views,
                    Zone = question.Zone,
                    Сreated = question.Сreated,
                    Answered = question.Answered
                })
                .ToListAsync();

            return questions;
        }

        public async Task<QuestionDto?> GetByIdAsync(Guid id)
        {
            Question? question = await _dataContext.Questions
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == default)
            {
                return null;
            }

            QuestionDto questionDTO = new()
            {
                Id = question.Id,
                Author = question.Author,
                Speaker = question.Speaker,
                Text = question.Text,
                Likes = question.Likes,
                Dislikes = question.Dislikes,
                Views = question.Views,
                Zone = question.Zone,
                Сreated = question.Сreated,
                Answered = question.Answered
            };

            return questionDTO;
        }

        public async Task<Guid> CreateAsync(QuestionCreateDto questionCreateDto)
        {
            Question question = new()
            {
                Author = questionCreateDto.Author,
                Speaker = questionCreateDto.Speaker,
                Zone = questionCreateDto.Zone,
                Text = questionCreateDto.Text,
                Сreated = DateTimeOffset.UtcNow,
            };

            await _dataContext.AddAsync(question);
            await _dataContext.SaveChangesAsync();

            return question.Id;
        }

        public async Task UpdateAsync(Guid id, QuestionUpdateDto questionUpdateDto)
        {
            Question? question = await _dataContext.Questions
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == default)
            {
                throw new InvalidOperationException("Объект не найден");
            }

            question.Author = questionUpdateDto.Author;
            question.Speaker = questionUpdateDto.Speaker;
            question.Zone = questionUpdateDto.Zone;
            question.Text = questionUpdateDto.Text;

            await _dataContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            Question? question = await _dataContext.Questions
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == default)
            {
                throw new InvalidOperationException("Объект не найден");
            }

            _dataContext.Remove(question);
            await _dataContext.SaveChangesAsync();
        }
    }
}
