using Application.DTOs.Dashboard;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Dashboard.Queries;

public sealed record GetDashboardStatsQuery
    : IRequest<DashboardStatsDto>;

public sealed class GetDashboardStatsQueryHandler
    : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto> {
    private readonly IDashboardService _dashboardService;

    public GetDashboardStatsQueryHandler(
        IDashboardService dashboardService) {
        _dashboardService = dashboardService;
    }

    public async Task<DashboardStatsDto> Handle(
        GetDashboardStatsQuery request,
        CancellationToken cancellationToken) {
        return await _dashboardService.GetStatsAsync(
            cancellationToken);
    }
}
