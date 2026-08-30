using Application.Features.Events.Queries;
using FluentValidation;

namespace Application.Validators.Events;

public sealed class GetAllEventsQueryValidator
    : AbstractValidator<GetAllEventsQuery> {
    private static readonly string[] AllowedSortFields =
        ["date", "price", "title"];

    public GetAllEventsQueryValidator() {
        RuleFor(x => x.MinPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinPrice.HasValue);

        RuleFor(x => x.MaxPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MaxPrice.HasValue);

        RuleFor(x => x)
            .Must(x =>
                !x.MinPrice.HasValue ||
                !x.MaxPrice.HasValue ||
                x.MinPrice <= x.MaxPrice)
            .WithMessage("Minimum price cannot be greater than maximum price.");

        RuleFor(x => x)
            .Must(x =>
                !x.DateFrom.HasValue ||
                !x.DateTo.HasValue ||
                x.DateFrom <= x.DateTo)
            .WithMessage("Start date cannot be after end date.");

        RuleFor(x => x.SortBy)
            .Must(sortBy =>
                string.IsNullOrWhiteSpace(sortBy) ||
                AllowedSortFields.Contains(sortBy.ToLowerInvariant()))
            .WithMessage("Sort field must be date, price, or title.");
    }
}
