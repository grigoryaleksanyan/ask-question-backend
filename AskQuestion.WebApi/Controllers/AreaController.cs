using AskQuestion.BLL.DTO.Area;
using AskQuestion.BLL.Repositories.Interfaces;
using AskQuestion.WebApi.Models.Request.Area;
using AskQuestion.WebApi.Models.Response.Area;
using AskQuestion.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace AskQuestion.WebApi.Controllers
{
    /// <summary>
    /// Контроллер областей.
    /// </summary>
    [Route("api/Area")]
    [ApiController]
    public class AreaController : ControllerBase
    {
        private readonly IAreaRepository _areaRepository;

        public AreaController(IAreaRepository areaRepository)
        {
            _areaRepository = areaRepository ?? throw new ArgumentNullException(nameof(areaRepository));
        }

        /// <summary>
        /// Получить список всех областей.
        /// </summary>
        /// <response code='200'>Список всех областей.</response>
        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<AreaViewModel>>> GetAll()
        {
            var areaDtos = await _areaRepository.GetAllAsync();

            IEnumerable<AreaViewModel> result = areaDtos.Select(areaDto => new AreaViewModel
            {
                Id = areaDto.Id,
                Title = areaDto.Title,
                Order = areaDto.Order,
                Сreated = areaDto.Сreated,
                Updated = areaDto.Updated,
            });

            return Ok(result);
        }

        /// <summary>
		/// Создать область.
		/// </summary>
        /// <param name="areaCreateModel">Модель создания области</param>
		/// <response code='200'>Id созданной категории.</response>
        [HttpPost("Create")]
        [Authorize(Roles = UserStringRoles.ADMINISTRATORS_ONLY)]
        public async Task<ActionResult<Guid>> Create(AreaCreateModel areaCreateModel)
        {
            AreaCreateDto areaCreateDto = new()
            {
                Title = areaCreateModel.Title,
                Order = areaCreateModel.Order,
            };

            Guid id = await _areaRepository.CreateAsync(areaCreateDto);

            return CreatedAtAction(nameof(Create), id);
        }

        /// <summary>
        /// Изменить область.
        /// </summary>
        /// <param name="areaUpdateModel">Модель изменения области</param>
        /// <response code='200'>Статус операции.</response>
        /// <response code='404'>Область не найдена.</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("Update")]
        [Authorize(Roles = UserStringRoles.ADMINISTRATORS_ONLY)]
        public async Task<IActionResult> Update(AreaUpdateModel areaUpdateModel)
        {
            var areaUpdateDto = new AreaUpdateDto()
            {
                Id = areaUpdateModel.Id,
                Title = areaUpdateModel.Title
            };

            await _areaRepository.UpdateAsync(areaUpdateDto);

            return Ok();
        }

        /// <summary>
		/// Удалить область.
		/// </summary>
		/// <param name="id">Id области.</param>
		/// <response code='200'>Статус операции.</response>
        /// <response code='404'>Область не найдена.</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("Delete")]
        [Authorize(Roles = UserStringRoles.ADMINISTRATORS_ONLY)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _areaRepository.DeleteAsync(id);

            return Ok();
        }

        /// <summary>
		/// Изменить сортировку областей.
		/// </summary>
		/// <param name="ids">Id областей.</param>
		/// <response code='200'>Статус операции.</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("SetOrder")]
        [Authorize(Roles = UserStringRoles.ADMINISTRATORS_ONLY)]
        public async Task<IActionResult> SetOrder(Guid[] ids)
        {
            if (ids.Length == 0)
            {
                return BadRequest();
            }

            await _areaRepository.SetOrderAsync(ids);

            return Ok();
        }
    }
}
