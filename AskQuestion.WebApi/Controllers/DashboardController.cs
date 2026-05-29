using AskQuestion.BLL.DTO.Dashboard;
using AskQuestion.BLL.Repositories.Interfaces;
using AskQuestion.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AskQuestion.WebApi.Controllers;

[Route("api/Dashboard")]
[ApiController]
public class DashboardController : ControllerBase
{
    private readonly IDashboardRepository _dashboardRepository;

    public DashboardController(IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository ?? throw new ArgumentNullException(nameof(dashboardRepository));
    }

    [HttpGet("Summary")]
    [Authorize(Roles = UserStringRoles.ADMINISTRATORS_ONLY)]
    public async Task<ActionResult<DashboardSummaryDto>> Summary([FromQuery] int periodDays = 30)
    {
        if (periodDays < 1)
        {
            periodDays = 30;
        }

        var result = await _dashboardRepository.GetSummaryAsync(periodDays);

        return Ok(result);
    }
}
