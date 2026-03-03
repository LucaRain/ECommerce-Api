using ECommerce.Application.DTOs.Dashboard;

namespace ECommerce.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardResponse> GetStatsAsync();
}
