using Domain.Common;

namespace Domain.Entities;

public class Category : BaseEntity {
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }

    public ICollection<EventEntity> Events { get; set; } = new List<EventEntity>();
}
