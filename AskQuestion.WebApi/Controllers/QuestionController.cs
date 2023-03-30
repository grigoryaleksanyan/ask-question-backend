using AskQuestion.BLL.DTO.Question;
using AskQuestion.BLL.Repositories;
using AskQuestion.Core.Constants;
using AskQuestion.WebApi.Helpers;
using AskQuestion.WebApi.Models.Request.Question;
using AskQuestion.WebApi.Models.Response.Question;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AskQuestion.WebApi.Controllers
{
    /// <summary>
    /// Контроллер вопросов.
    /// </summary>
    [Route("api/Question")]
    [ApiController]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public class QuestionController : ControllerBase
    {
        private readonly IQuestionRepository _questionRepository;

        public QuestionController(IQuestionRepository questionRepository)
        {
            _questionRepository = questionRepository ?? throw new ArgumentNullException(nameof(questionRepository));
        }

        /// <summary>
        /// Получить изображение капчи для подачи вопроса.
        /// </summary>
        /// <response code='200'>Капча в Base64.</response>
        [HttpGet("GetCapctha")]
        public ActionResult<string> GetCapctha()
        {
            string capcthaText = GenerateCaptcha.GetCapcthaText();
            string captcha = GenerateCaptcha.GetCaptchaBase64(capcthaText);

            HttpContext.Session.SetString("capctha", capcthaText);

            return Ok(captcha);
        }

        /// <summary>
        /// Получить список всех вопросов.
        /// </summary>
        /// <response code='200'>Список всех вопросов.</response>
        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<QuestionViewModel>>> GetAll()
        {
            var questions = await _questionRepository.GetAllAsync();

            IEnumerable<QuestionViewModel> result = questions.Select(question => new QuestionViewModel
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
            });

            return Ok(result);
        }

        /// <summary>
        /// Получить вопрос по Id.
        /// </summary>
        /// <param name="id">Id вопроса.</param>
        /// <response code='200'>Модель вопроса.</response>
        /// <response code='404'>Вопрос не найден.</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("GetById/{id:guid}")]
        public async Task<ActionResult<QuestionViewModel>> GetById(Guid id)
        {
            var question = await _questionRepository.GetByIdAsync(id);

            if (question == default)
            {
                return NotFound();
            }

            QuestionViewModel result = new()
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

            return Ok(result);
        }

        /// <summary>
		/// Создать вопрос.
		/// </summary>
		/// <param name="capctha">Проверочный код с капчи.</param>
		/// <param name="questionCreateModel">Модель создания вопроса.</param>
		/// <response code='200'>Id созданного вопроса.</response>
        [HttpPost("Create")]
        public async Task<ActionResult<Guid>> Create(string capctha, QuestionCreateModel questionCreateModel)
        {

            if (capctha != HttpContext.Session.GetString("capctha"))
            {
                return BadRequest("Неверно введена капча");
            }

            QuestionCreateDto questionCreateDto = new()
            {
                Author = questionCreateModel.Author,
                Speaker = questionCreateModel.Speaker,
                Text = questionCreateModel.Text,
            };

            Guid id = await _questionRepository.CreateAsync(questionCreateDto);

            return CreatedAtAction(nameof(Create), new { id });
        }

        /// <summary>
        /// Изменить вопрос.
        /// </summary>
        /// <param name="id">Id вопроса.</param>
        /// <param name="questionUpdateModel">Модель изменения вопроса.</param>
        /// <response code='200'>Статус операции.</response>
        /// <response code='404'>Вопрос не найден.</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("Update/{id:guid}")]
        [Authorize(Roles = UserStringRoles.ADMINISTRATORS_ONLY)]
        public async Task<IActionResult> Update(Guid id, QuestionUpdateModel questionUpdateModel)
        {
            var question = await _questionRepository.GetByIdAsync(id);

            if (question == default)
            {
                return NotFound();
            }

            QuestionUpdateDto questionUpdateDto = new()
            {
                Id = id,
                Text = questionUpdateModel.Text,
                Author = questionUpdateModel.Author,
                Area = questionUpdateModel.Area,
                Speaker = questionUpdateModel.Speaker,
            };

            await _questionRepository.UpdateAsync(id, questionUpdateDto);

            return Ok();
        }

        /// <summary>
		/// Удалить вопрос.
		/// </summary>
		/// <param name="id">Id вопроса.</param>
		/// <response code='200'>Статус операции.</response>
        /// <response code='404'>Вопрос не найден.</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("Delete/{id:guid}")]
        [Authorize(Roles = UserStringRoles.ADMINISTRATORS_ONLY)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var question = await _questionRepository.GetByIdAsync(id);

            if (question == default)
            {
                return NotFound();
            }

            await _questionRepository.DeleteAsync(id);

            return Ok();
        }
    }
}
