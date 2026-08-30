using Application.Features.Orders.Commands;
using FluentValidation;

namespace Application.Validators.Orders;

public sealed class CreateOrderCommandValidator
    : AbstractValidator<CreateOrderCommand> {
    public CreateOrderCommandValidator() {
        RuleFor(x => x.EventId)
            .NotEmpty();

        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be at least 1.")
            .LessThanOrEqualTo(10)
            .WithMessage("Maximum 10 tickets per order.");
    }
}
