using AskQuestion.BLL.DTO.FaqCategory;
using AskQuestion.BLL.Repositories.Interfaces;
using AskQuestion.WebApi.Models.Request.FaqCategory;
using AskQuestion.WebApi.Models.Response.FaqCategory;
using AskQuestion.WebApi.Models.Response.FaqEntry;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AskQuestion.WebApi.Controllers
{
    /// <summary>
    /// Контроллер категорий FAQ.
    /// </summary>
    [Route("api/FaqCategory")]
    [ApiController]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public class FaqCategoryController : ControllerBase
    {
        private readonly IFaqCategoryRepository _faqCategoryRepository;

        public FaqCategoryController(IFaqCategoryRepository faqCategoryRepository)
        {
            _faqCategoryRepository = faqCategoryRepository ?? throw new ArgumentNullException(nameof(faqCategoryRepository));
        }

        /// <summary>
        /// Получить список всех категорий.
        /// </summary>
        /// <response code='200'>Список всех категорий.</response>
        [HttpGet("GetAll"), Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<FaqCategoryViewModel>>> GetAll()
        {
            var faqCategoryDtos = await _faqCategoryRepository.GetAllAsync();

            IEnumerable<FaqCategoryViewModel> result = faqCategoryDtos.Select(faqCategoryDto => new FaqCategoryViewModel
            {
                Id = faqCategoryDto.Id,
                Name = faqCategoryDto.Name,
                Order = faqCategoryDto.Order,
                Сreated = faqCategoryDto.Сreated,
                Updated = faqCategoryDto.Updated,
            });

            return Ok(result);
        }

        /// <summary>
        /// Получить список всех категорий c записями.
        /// </summary>
        /// <response code='200'>Список всех категорий.</response>
        [HttpGet("GetAllWithEntries")]
        public async Task<ActionResult<IEnumerable<FaqCategoryWithEntriesViewModel>>> GetAllWithEntries()
        {
            var faqCategoryDtos = await _faqCategoryRepository.GetAllWithEntriesAsync();

            IEnumerable<FaqCategoryWithEntriesViewModel> result = faqCategoryDtos.Select(faqCategoryDto => new FaqCategoryWithEntriesViewModel
            {
                Id = faqCategoryDto.Id,
                Name = faqCategoryDto.Name,
                Order = faqCategoryDto.Order,
                Сreated = faqCategoryDto.Сreated,
                Updated = faqCategoryDto.Updated,
                Entries = faqCategoryDto.Entries.Select(entry => new FaqEntryViewModel()
                {
                    Id = entry.Id,
                    Question = entry.Question,
                    Answer = entry.Answer,
                    Сreated = entry.Сreated,
                    Updated = entry.Updated
                }).ToList()
            });

            return Ok(result);
        }

        /// <summary>
        /// Получить категорию по Id.
        /// </summary>
        /// <param name="id">Id категории.</param>
        /// <response code='200'>Данные категории.</response>
        /// <response code='404'>Категория не найдена.</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("GetById")]
        public async Task<ActionResult<FaqCategoryWithEntriesViewModel>> GetById(Guid id)
        {
            var faqCategoryDto = await _faqCategoryRepository.GetByIdAsync(id);

            if (faqCategoryDto == default)
            {
                return NotFound();
            }

            FaqCategoryWithEntriesViewModel result = new()
            {
                Id = faqCategoryDto.Id,
                Name = faqCategoryDto.Name,
                Order = faqCategoryDto.Order,
                Сreated = faqCategoryDto.Сreated,
                Updated = faqCategoryDto.Updated,
                Entries = faqCategoryDto.Entries.Select(entry => new FaqEntryViewModel()
                {
                    Id = entry.Id,
                    Question = entry.Question,
                    Answer = entry.Answer,
                    Сreated = entry.Сreated,
                    Updated = entry.Updated
                }).ToList()
            };

            return Ok(result);
        }

        /// <summary>
		/// Создать категорию.
		/// </summary>
        /// <param name="faqCategoryCreateModel">Модель создания категории</param>
		/// <response code='200'>Id созданной категории.</response>
        [HttpPost("Create"), Authorize(Roles = "Admin")]
        public async Task<ActionResult<Guid>> Create(FaqCategoryCreateModel faqCategoryCreateModel)
        {
            FaqCategoryCreateDto faqCategoryCreateDto = new()
            {
                Name = faqCategoryCreateModel.Name,
                Order = faqCategoryCreateModel.Order,
            };

            Guid id = await _faqCategoryRepository.CreateAsync(faqCategoryCreateDto);

            return CreatedAtAction(nameof(Create), id);
        }

        /// <summary>
        /// Изменить категорию.
        /// </summary>
        /// <param name="faqCategoryUpdateModel">Модель изменения категории</param>
        /// <response code='200'>Статус операции.</response>
        /// <response code='404'>Категория не найдена.</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("Update"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(FaqCategoryUpdateModel faqCategoryUpdateModel)
        {
            var faqCategoryUpdateDto = new FaqCategoryUpdateDto()
            {
                Id = faqCategoryUpdateModel.Id,
                Name = faqCategoryUpdateModel.Name
            };

            await _faqCategoryRepository.UpdateAsync(faqCategoryUpdateDto);

            return Ok();
        }

        /// <summary>
		/// Удалить категорию.
		/// </summary>
		/// <param name="id">Id категории.</param>
		/// <response code='200'>Статус операции.</response>
        /// <response code='404'>Категория не найдена.</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("Delete"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _faqCategoryRepository.DeleteAsync(id);

            return Ok();
        }

        /// <summary>
		/// Изменить сортировку категорий.
		/// </summary>
		/// <param name="ids">Id категорий.</param>
		/// <response code='200'>Статус операции.</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("SetOrder"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> SetOrder(Guid[] ids)
        {
            if (ids.Length == 0)
            {
                return BadRequest();
            }

            await _faqCategoryRepository.SetOrderAsync(ids);

            return Ok();
        }
    }
}
