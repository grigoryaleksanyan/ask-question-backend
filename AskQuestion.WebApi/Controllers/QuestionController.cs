using AskQuestion.BLL.DTO;
using AskQuestion.BLL.DTO.Question;
using AskQuestion.BLL.Repositories;
using AskQuestion.BLL.Repositories.Interfaces;
using AskQuestion.Core.Constants;
using AskQuestion.Core.Enums;
using AskQuestion.WebApi.Extensions;
using AskQuestion.WebApi.Helpers;
using AskQuestion.WebApi.Models.Request.Question;
using AskQuestion.WebApi.Models.Response.Question;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AskQuestion.WebApi.Controllers
{
    /// <summary>
    /// Контроллер вопросов.
    /// </summary>
    [Route("api/Question")]
    [ApiController]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    [ServiceFilter(typeof(EnsureVisitorIdAttribute))]
    public class QuestionController : ControllerBase
    {
        private readonly IQuestionRepository _questionRepository;

        public QuestionController(IQuestionRepository questionRepository)
        {
            _questionRepository = questionRepository ?? throw new ArgumentNullException(nameof(questionRepository));
        }

        private Guid GetVisitorId()
        {
            var visitorIdStr = HttpContext.Items["VisitorId"]?.ToString()
                ?? HttpContext.Request.Cookies["VisitorId"]
                ?? throw new InvalidOperationException("VisitorId не найден");

            return Guid.Parse(visitorIdStr);
        }

        private Guid? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return userIdClaim != null ? Guid.Parse(userIdClaim) : null;
        }

        private bool IsAdmin()
        {
            return User.IsInRole(UserStringRoles.ADMINISTRATORS_ONLY);
        }

        /// <summary>
        /// Получить изображение капчи для подачи вопроса.
        /// </summary>
        /// <response code='200'>Капча в Base64.</response>
        [HttpGet("GetCaptcha")]
        public ActionResult<string> GetCaptcha()
        {
            string captchaText = GenerateCaptcha.GetCaptchaText();
            string captcha = GenerateCaptcha.GetCaptchaBase64(captchaText);

            HttpContext.Session.SetString("captcha", captchaText);

            return Ok(captcha);
        }

        /// <summary>
        /// Получить список всех вопросов.
        /// </summary>
        /// <response code='200'>Список всех вопросов.</response>
        [HttpGet("GetAll")]
        public async Task<ActionResult<PaginatedResult<QuestionViewModel>>> GetAll(
            int page = 1,
            int pageSize = 10,
            int? status = null,
            Guid? speakerId = null,
            Guid? areaId = null,
            string? search = null,
            string sortOrder = "desc")
        {
            var result = await _questionRepository.GetAllAsync(page, pageSize, status, speakerId, areaId, search, sortOrder);

            PaginatedResult<QuestionViewModel> response = new()
            {
                Items = result.Items.Select(question => new QuestionViewModel
                {
                    Id = question.Id,
                    Text = question.Text,
                    Author = question.Author,
                    AreaId = question.AreaId,
                    AreaTitle = question.AreaTitle,
                    SpeakerId = question.SpeakerId,
                    SpeakerName = question.SpeakerName,
                    Likes = question.Likes,
                    Dislikes = question.Dislikes,
                    Views = question.Views,
                    Status = question.Status,
                    Comment = question.Comment,
                    Created = question.Created,
                    Answered = question.Answered
                }).ToList(),
                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize,
            };

            return Ok(response);
        }

        /// <summary>
        /// Получить список популярных вопросов.
        /// </summary>
        /// <response code='200'>Список популярных вопросов.</response>
        [HttpGet("GetPopularQuestions")]
        public async Task<ActionResult<IEnumerable<QuestionViewModel>>> GetPopularQuestions()
        {
            var questions = await _questionRepository.GetPopularQuestionsAsync();

            IEnumerable<QuestionViewModel> result = questions.Select(question => new QuestionViewModel
            {
                Id = question.Id,
                Text = question.Text,
                Author = question.Author,
                AreaId = question.AreaId,
                AreaTitle = question.AreaTitle,
                SpeakerId = question.SpeakerId,
                SpeakerName = question.SpeakerName,
                Likes = question.Likes,
                Dislikes = question.Dislikes,
                Views = question.Views,
                Status = question.Status,
                Comment = question.Comment,
                Created = question.Created,
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
            QuestionDto? question = await _questionRepository.GetByIdAsync(id);

            if (question == null)
            {
                return NotFound();
            }

            await _questionRepository.IncrementViewsAsync(id);

            Guid visitorId = GetVisitorId();
            VoteType? userVote = await _questionRepository.GetUserVoteAsync(id, visitorId);

            QuestionViewModel result = new()
            {
                Id = question.Id,
                Text = question.Text,
                Author = question.Author,
                AreaId = question.AreaId,
                AreaTitle = question.AreaTitle,
                SpeakerId = question.SpeakerId,
                SpeakerName = question.SpeakerName,
                Likes = question.Likes,
                Dislikes = question.Dislikes,
                Views = question.Views + 1,
                Status = question.Status,
                Comment = question.Comment,
                Created = question.Created,
                Answered = question.Answered,
                UserVote = userVote,
            };

            return Ok(result);
        }

        [HttpPost("{id:guid}/like")]
        public async Task<ActionResult<VoteResultViewModel>> Like(Guid id)
        {
            Guid visitorId = GetVisitorId();

            VoteResultDto result = await _questionRepository.ToggleLikeAsync(id, visitorId);

            VoteResultViewModel response = new()
            {
                Likes = result.Likes,
                Dislikes = result.Dislikes,
                UserVote = result.UserVote,
            };

            return Ok(response);
        }

        [HttpPost("{id:guid}/dislike")]
        public async Task<ActionResult<VoteResultViewModel>> Dislike(Guid id)
        {
            Guid visitorId = GetVisitorId();

            VoteResultDto result = await _questionRepository.ToggleDislikeAsync(id, visitorId);

            VoteResultViewModel response = new()
            {
                Likes = result.Likes,
                Dislikes = result.Dislikes,
                UserVote = result.UserVote,
            };

            return Ok(response);
        }

        /// <summary>
		/// Создать вопрос.
		/// </summary>
		/// <param name="captcha">Проверочный код с капчи.</param>
		/// <param name="questionCreateModel">Модель создания вопроса.</param>
		/// <response code='200'>Id созданного вопроса.</response>
        [HttpPost("Create")]
        public async Task<ActionResult<Guid>> Create(string captcha, QuestionCreateModel questionCreateModel)
        {

            if (captcha != HttpContext.Session.GetString("captcha"))
            {
                return BadRequest("Неверно введена капча");
            }

            QuestionCreateDto questionCreateDto = new()
            {
                Author = questionCreateModel.Author,
                SpeakerId = questionCreateModel.SpeakerId,
                Text = questionCreateModel.Text,
                AreaId = questionCreateModel.AreaId,
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
        public async Task<ActionResult<QuestionViewModel>> Update(Guid id, QuestionUpdateModel questionUpdateModel)
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
                AreaId = questionUpdateModel.AreaId,
                SpeakerId = questionUpdateModel.SpeakerId,
            };

            var questionDto = await _questionRepository.UpdateAsync(id, questionUpdateDto);

            QuestionViewModel result = new()
            {
                Id = questionDto.Id,
                Text = questionDto.Text,
                Author = questionDto.Author,
                AreaId = questionDto.AreaId,
                AreaTitle = questionDto.AreaTitle,
                SpeakerId = questionDto.SpeakerId,
                SpeakerName = questionDto.SpeakerName,
                Likes = questionDto.Likes,
                Dislikes = questionDto.Dislikes,
                Views = questionDto.Views,
                Status = questionDto.Status,
                Comment = questionDto.Comment,
                Created = questionDto.Created,
                Answered = questionDto.Answered,
            };

            return Ok(result);
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

        /// <summary>
        /// Изменить статус вопроса.
        /// </summary>
        /// <param name="id">Id вопроса.</param>
        /// <param name="model">Модель смены статуса.</param>
        /// <response code='200'>Статус изменён.</response>
        /// <response code='400'>Недопустимый переход.</response>
        /// <response code='403'>Нет прав.</response>
        /// <response code='404'>Вопрос не найден.</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("{id:guid}/status")]
        [Authorize(Roles = $"{UserStringRoles.ADMINISTRATORS_ONLY},{UserStringRoles.SPEAKERS_ONLY}")]
        public async Task<IActionResult> ChangeStatus(Guid id, QuestionStatusChangeModel model)
        {
            QuestionDto? question = await _questionRepository.GetByIdAsync(id);

            if (question == null)
            {
                return NotFound();
            }

            if (!IsAdmin())
            {
                var speakerId = GetCurrentUserId();
                if (question.SpeakerId != speakerId)
                {
                    return Forbid();
                }
            }

            var dto = new QuestionStatusChangeDto
            {
                QuestionId = id,
                NewStatus = model.Status,
                ChangedByUserId = GetCurrentUserId(),
            };

            try
            {
                await _questionRepository.ChangeStatusAsync(dto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();
        }

        /// <summary>
        /// Установить комментарий к вопросу.
        /// </summary>
        /// <param name="id">Id вопроса.</param>
        /// <param name="model">Модель комментария.</param>
        /// <response code='200'>Комментарий установлен.</response>
        /// <response code='403'>Нет прав.</response>
        /// <response code='404'>Вопрос не найден.</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("{id:guid}/comment")]
        [Authorize(Roles = $"{UserStringRoles.ADMINISTRATORS_ONLY},{UserStringRoles.SPEAKERS_ONLY}")]
        public async Task<IActionResult> SetComment(Guid id, QuestionCommentModel model)
        {
            QuestionDto? question = await _questionRepository.GetByIdAsync(id);

            if (question == null)
            {
                return NotFound();
            }

            if (!IsAdmin())
            {
                var speakerId = GetCurrentUserId();
                if (question.SpeakerId != speakerId)
                {
                    return Forbid();
                }
            }

            var dto = new QuestionCommentDto
            {
                QuestionId = id,
                Comment = model.Comment,
            };

            try
            {
                await _questionRepository.SetCommentAsync(dto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();
        }
    }
}
