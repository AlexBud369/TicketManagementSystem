using Application.DTOs.Reports;

namespace Application.Interfaces;

public interface IReportService {
    Task<FileExportDto> ExportAsync(
        ReportType type,
        ReportFormat format,
        CancellationToken cancellationToken = default);
}
