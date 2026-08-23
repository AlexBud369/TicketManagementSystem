using Application.Features.Reports.Queries;
using FluentValidation;

namespace Application.Validators.Reports;

public sealed class ExportReportQueryValidator
    : AbstractValidator<ExportReportQuery> {
    public ExportReportQueryValidator() {
        RuleFor(x => x.Type)
            .IsInEnum();

        RuleFor(x => x.Format)
            .IsInEnum();
    }
}
