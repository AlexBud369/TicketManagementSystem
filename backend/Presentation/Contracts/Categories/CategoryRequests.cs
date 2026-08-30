namespace Presentation.Contracts.Categories;

public sealed record CreateCategoryRequest(
    string Name,
    string Description,
    string Icon);

public sealed record UpdateCategoryRequest(
    string Name,
    string Description,
    string Icon);
