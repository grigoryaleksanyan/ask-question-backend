using AskQuestion.BLL.DTO.FaqEntry;
using AskQuestion.BLL.Repositories.Interfaces;
using AskQuestion.WebApi.Models.Request.FaqEntry;
using AskQuestion.WebApi.Models.Response.FaqEntry;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace AskQuestion.WebApi.Controllers
{
    /// <summary>
    /// Контроллер записей в FAQ.
    /// </summary>
    [Route("api/FaqEntry")]
    [ApiController]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    [Authorize(Roles = "Admin")]
    public class FaqEntryController : ControllerBase
    {
        private readonly IFaqEntryRepository _faqEntryRepository;

        public FaqEntryController(IFaqEntryRepository faqEntryRepository)
        {
            _faqEntryRepository = faqEntryRepository ?? throw new ArgumentNullException(nameof(faqEntryRepository));
        }

        /// <summary>
        /// Получить список всех записей.
        /// </summary>
        /// <response code='200'>Список всех записей.</response>
        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<FaqEntryViewModel>>> GetAll()
        {
            var faqEntryDtos = await _faqEntryRepository.GetAllAsync();

            IEnumerable<FaqEntryViewModel> result = faqEntryDtos.Select(faqEntryDtos => new FaqEntryViewModel
            {
                Id = faqEntryDtos.Id,
                Question = faqEntryDtos.Question,
                Answer = faqEntryDtos.Answer,
                Order = faqEntryDtos.Order,
                Сreated = faqEntryDtos.Сreated,
                Updated = faqEntryDtos.Updated,
            });

            return Ok(result);
        }

        /// <summary>
        /// Получить запись по Id.
        /// </summary>
        /// <param name="id">Id записи.</param>
        /// <response code='200'>Данные записи.</response>
        /// <response code='404'>Запись не найдена.</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("GetById")]
        public async Task<ActionResult<FaqEntryViewModel>> GetById(Guid id)
        {
            var faqEntryDto = await _faqEntryRepository.GetByIdAsync(id);

            if (faqEntryDto == default)
            {
                return NotFound();
            }

            FaqEntryViewModel result = new()
            {
                Id = faqEntryDto.Id,
                Question = faqEntryDto.Question,
                Answer = faqEntryDto.Answer,
                Order = faqEntryDto.Order,
                Сreated = faqEntryDto.Сreated,
                Updated = faqEntryDto.Updated,
            };

            return Ok(result);
        }

        /// <summary>
		/// Создать запись.
		/// </summary>
        /// <param name="faqEntryCreateModel">Модель создания записи</param>
		/// <response code='200'>Id созданной записи.</response>
        [HttpPost("Create")]
        public async Task<ActionResult<Guid>> Create(FaqEntryCreateModel faqEntryCreateModel)
        {
            FaqEntryCreateDto faqEntryCreateDto = new()
            {
                FaqCategoryId = faqEntryCreateModel.FaqCategoryId,
                Question = faqEntryCreateModel.Question,
                Answer = faqEntryCreateModel.Answer,
                Order = faqEntryCreateModel.Order,
            };

            Guid id = await _faqEntryRepository.CreateAsync(faqEntryCreateDto);

            return CreatedAtAction(nameof(Create), id);
        }

        /// <summary>
        /// Изменить запись.
        /// </summary>
        /// <param name="faqEntryUpdateModel">Модель изменения записи</param>
        /// <response code='200'>Статус операции.</response>
        /// <response code='404'>Запись не найдена.</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("Update")]
        public async Task<IActionResult> Update(FaqEntryUpdateModel faqEntryUpdateModel)
        {
            var faqEntryUpdateDto = new FaqEntryUpdateDto()
            {
                Id = faqEntryUpdateModel.Id,
                Question = faqEntryUpdateModel.Question,
                Answer = faqEntryUpdateModel.Answer,
            };

            await _faqEntryRepository.UpdateAsync(faqEntryUpdateDto);

            return Ok();
        }

        /// <summary>
		/// Удалить запись.
		/// </summary>
		/// <param name="id">Id записи.</param>
		/// <response code='200'>Статус операции.</response>
        /// <response code='404'>Запись не найдена.</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("Delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _faqEntryRepository.DeleteAsync(id);

            return Ok();
        }

        /// <summary>
		/// Изменить сортировку записей.
		/// </summary>
		/// <param name="ids">Id записей.</param>
		/// <response code='200'>Статус операции.</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("SetOrder")]
        public async Task<IActionResult> SetOrder(Guid[] ids)
        {
            if (ids.Length == 0)
            {
                return BadRequest();
            }

            await _faqEntryRepository.SetOrderAsync(ids);

            return Ok();
        }
    }
}
