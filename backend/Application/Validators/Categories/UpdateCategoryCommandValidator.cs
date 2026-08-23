using Application.Features.Categories.Commands;
using FluentValidation;

namespace Application.Validators.Categories;

public sealed class UpdateCategoryCommandValidator
    : AbstractValidator<UpdateCategoryCommand> {
    public UpdateCategoryCommandValidator() {
        RuleFor(x => x.CategoryId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .MaximumLength(1000);

        RuleFor(x => x.Icon)
            .MaximumLength(500);
    }
}
