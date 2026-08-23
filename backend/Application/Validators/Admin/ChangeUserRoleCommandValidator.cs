using Application.Features.Admin.Commands;
using FluentValidation;

namespace Application.Validators.Admin;

public sealed class ChangeUserRoleCommandValidator
    : AbstractValidator<ChangeUserRoleCommand> {
    private static readonly string[] ValidRoles = ["Admin", "User"];

    public ChangeUserRoleCommandValidator() {
        RuleFor(x => x.AdminId)
            .NotEmpty().WithMessage("Admin ID is required.");

        RuleFor(x => x.TargetUserId)
            .NotEmpty().WithMessage("Target user ID is required.");

        RuleFor(x => x.NewRole)
            .NotEmpty().WithMessage("Role is required.")
            .Must(role => ValidRoles.Contains(role))
            .WithMessage("Role must be either 'Admin' or 'User'.");
    }
}
