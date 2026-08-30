using Application.Features.Payments.Commands;
using FluentValidation;

namespace Application.Validators.Payments;

public sealed class CreatePaymentSessionCommandValidator
    : AbstractValidator<CreatePaymentSessionCommand> {
    public CreatePaymentSessionCommandValidator() {
        RuleFor(x => x.OrderId)
            .NotEmpty();

        RuleFor(x => x.UserId)
            .NotEmpty();
    }
}
