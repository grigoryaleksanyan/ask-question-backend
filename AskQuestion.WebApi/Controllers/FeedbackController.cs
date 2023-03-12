﻿using AskQuestion.BLL.DTO.Feedback;
using AskQuestion.BLL.Repositories.Interfaces;
using AskQuestion.WebApi.Models.Request.Feedback;
using AskQuestion.WebApi.Models.Response.Feedback;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AskQuestion.WebApi.Controllers
{
    /// <summary>
    /// Контроллер записей по обратной связи.
    /// </summary>
    [Route("api/Feedback")]
    [ApiController]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackRepository _feedbackRepository;

        public FeedbackController(IFeedbackRepository feedbackRepository)
        {
            _feedbackRepository = feedbackRepository ?? throw new ArgumentNullException(nameof(feedbackRepository));
        }

        /// <summary>
        /// Получить список всех записей.
        /// </summary>
        /// <response code='200'>Список всех записей.</response>
        [HttpGet("GetAll"), Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<FeedbackViewModel>>> GetAll()
        {
            var feedbackDtos = await _feedbackRepository.GetAllAsync();

            IEnumerable<FeedbackViewModel> result = feedbackDtos.Select(feedbackDto => new FeedbackViewModel
            {
                Id = feedbackDto.Id,
                Username = feedbackDto.Username,
                Email = feedbackDto.Email,
                Theme = feedbackDto.Theme,
                Text = feedbackDto.Text,
                Сreated = feedbackDto.Сreated,
                Updated = feedbackDto.Updated,
            });

            return Ok(result);
        }

        /// <summary>
		/// Создать запись.
		/// </summary>
        /// <param name="feedbackCreateModel">Модель создания обратной связи.</param>
		/// <response code='200'>Id созданной записи.</response>
        [HttpPost("Create")]
        public async Task<ActionResult<Guid>> Create(FeedbackCreateModel feedbackCreateModel)
        {
            FeedbackCreateDto feedbackCreateDto = new()
            {
                Username = feedbackCreateModel.Username,
                Email = feedbackCreateModel.Email,
                Theme = feedbackCreateModel.Theme,
                Text = feedbackCreateModel.Text,
            };

            Guid id = await _feedbackRepository.CreateAsync(feedbackCreateDto);

            return CreatedAtAction(nameof(Create), id);
        }

        /// <summary>
		/// Удалить запись.
		/// </summary>
		/// <param name="id">Id записи.</param>
		/// <response code='200'>Статус операции.</response>
        /// <response code='404'>Запись не найдена.</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("Delete"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _feedbackRepository.DeleteAsync(id);

            return Ok();
        }
    }
}
