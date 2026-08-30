using Application.DTOs.Categories;
using Application.Interfaces.Categories.Models;

namespace Application.Interfaces;

public interface ICategoryService {
    Task<CategoryDto> CreateAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken = default);

    Task<CategoryDto> UpdateAsync(
        Guid categoryId,
        UpdateCategoryRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default);

    Task<CategoryDto> GetByIdAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<CategoryDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default);

    Task<bool> NameExistsAsync(
        string name,
        Guid? excludeCategoryId = null,
        CancellationToken cancellationToken = default);
}
