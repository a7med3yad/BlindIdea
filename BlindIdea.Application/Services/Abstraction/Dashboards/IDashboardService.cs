using BlindIdea.Application.Dtos;

namespace BlindIdea.Application.Services.Abstraction.Dashboards
{
    public interface IDashboardService
    {
        Task<DashboardResponseDto> GetDashboardAsync(string userId);
    }
}