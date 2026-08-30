namespace Application.DTOs.Categories;

public sealed class CategoryDto {
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Icon { get; init; }
    public int EventsCount { get; init; }
    public DateTime CreatedAt { get; init; }
}
