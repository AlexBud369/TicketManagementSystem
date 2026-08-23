using Application.DTOs.Reports;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Reports.Queries;

public sealed record ExportReportQuery(
    ReportType Type,
    ReportFormat Format
) : IRequest<FileExportDto>;

public sealed class ExportReportQueryHandler
    : IRequestHandler<ExportReportQuery, FileExportDto> {
    private readonly IReportService _reportService;

    public ExportReportQueryHandler(IReportService reportService) {
        _reportService = reportService;
    }

    public async Task<FileExportDto> Handle(
        ExportReportQuery request,
        CancellationToken cancellationToken) {
        return await _reportService.ExportAsync(
            request.Type, request.Format, cancellationToken);
    }
}
