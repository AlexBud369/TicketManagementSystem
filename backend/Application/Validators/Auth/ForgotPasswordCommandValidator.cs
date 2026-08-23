using Application.Features.Auth.Commands;
using FluentValidation;

namespace Application.Features.Auth.Validators;

public sealed class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand> {
    public ForgotPasswordCommandValidator() {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email format is invalid.");
    }
}
