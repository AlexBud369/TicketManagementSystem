using Application.Common;
using Application.DTOs.Categories;
using Application.Interfaces;
using Application.Interfaces.Categories.Models;
using MediatR;

namespace Application.Features.Categories.Commands;

public sealed record CreateCategoryCommand(
    string Name,
    string Description,
    string Icon
) : IRequest<CategoryDto>;

public sealed class CreateCategoryCommandHandler
    : IRequestHandler<CreateCategoryCommand, CategoryDto> {
    private readonly ICategoryService _categoryService;

    public CreateCategoryCommandHandler(ICategoryService categoryService) {
        _categoryService = categoryService;
    }

    public async Task<CategoryDto> Handle(
        CreateCategoryCommand request,
        CancellationToken cancellationToken) {
        var exists = await _categoryService.NameExistsAsync(
            request.Name,
            null,
            cancellationToken);

        Guard.AgainstDuplicate(
            exists,
            "A category with this name already exists.");

        var createRequest = new CreateCategoryRequest {
            Name = request.Name,
            Description = request.Description,
            Icon = request.Icon
        };

        return await _categoryService.CreateAsync(
            createRequest, cancellationToken);
    }
}
