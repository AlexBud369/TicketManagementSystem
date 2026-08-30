using Application.Features.Events.Commands;
using Domain.Scheduling;
using FluentValidation;

namespace Application.Validators.Events;

public sealed class CreateEventCommandValidator
    : AbstractValidator<CreateEventCommand> {
    public CreateEventCommandValidator() {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(5000);

        RuleFor(x => x.CategoryId)
            .NotEmpty();

        RuleFor(x => x.Location)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x)
            .Must(x => !EventSchedule.HasStarted(x.Date, x.StartTime, DateTime.UtcNow))
            .WithMessage("Event start must be in the future.");

        RuleFor(x => x.StartTime)
            .NotEqual(x => x.EndTime)
            .WithMessage("Start time and end time cannot be the same.");

        RuleFor(x => x.MaximumCapacity)
            .GreaterThan(0);

        RuleFor(x => x.TicketPrice)
            .GreaterThan(0);

        RuleFor(x => x.OrganizerId)
            .NotEmpty();

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90)
            .When(x => x.Latitude.HasValue);

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180)
            .When(x => x.Longitude.HasValue);
    }
}
