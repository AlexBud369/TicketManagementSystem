using Application.Common;
using Application.DTOs.Categories;
using Application.Interfaces;
using Application.Interfaces.Categories.Models;
using MediatR;

namespace Application.Features.Categories.Commands;

public sealed record UpdateCategoryCommand(
    Guid CategoryId,
    string Name,
    string Description,
    string Icon
) : IRequest<CategoryDto>;

public sealed class UpdateCategoryCommandHandler
    : IRequestHandler<UpdateCategoryCommand, CategoryDto> {
    private readonly ICategoryService _categoryService;

    public UpdateCategoryCommandHandler(ICategoryService categoryService) {
        _categoryService = categoryService;
    }

    public async Task<CategoryDto> Handle(
        UpdateCategoryCommand request,
        CancellationToken cancellationToken) {
        var nameExists = await _categoryService.NameExistsAsync(
            request.Name,
            request.CategoryId,
            cancellationToken);

        Guard.AgainstDuplicate(
            nameExists,
            "A category with this name already exists.");

        var updateRequest = new UpdateCategoryRequest {
            Name = request.Name,
            Description = request.Description,
            Icon = request.Icon
        };

        return await _categoryService.UpdateAsync(
            request.CategoryId, updateRequest, cancellationToken);
    }
}
