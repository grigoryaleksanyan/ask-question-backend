using AskQuestion.BLL.DTO.Dashboard;

namespace AskQuestion.BLL.Repositories.Interfaces;

public interface IDashboardRepository
{
    Task<DashboardSummaryDto> GetSummaryAsync(int periodDays);
}
