using Application.Features.Tickets.Commands;
using FluentValidation;

namespace Application.Validators.Tickets;

public sealed class ValidateTicketCommandValidator
    : AbstractValidator<ValidateTicketCommand> {
    public ValidateTicketCommandValidator() {
        RuleFor(x => x.QrCode)
            .NotEmpty();
    }
}
