using Application.Features.Admin.Commands;
using FluentValidation;

namespace Application.Validators.Admin;

public sealed class UpdateUserByAdminCommandValidator
    : AbstractValidator<UpdateUserByAdminCommand> {
    public UpdateUserByAdminCommandValidator() {
        RuleFor(x => x.TargetUserId)
            .NotEmpty();

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Phone)
            .MaximumLength(20)
            .Matches(@"^[\d\+\-\(\)\s]*$")
            .When(x => !string.IsNullOrEmpty(x.Phone));

        RuleFor(x => x.Address)
            .MaximumLength(200);
    }
}
