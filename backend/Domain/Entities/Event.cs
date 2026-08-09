using Domain.Common;

namespace Domain.Entities;

public class EventEntity : BaseEntity {
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public Guid OrganizerId { get; set; }
    public string Location { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string? BannerImageUrl { get; set; }
    public int MaxCapacity { get; set; }
    public decimal TicketPrice { get; set; }
    public int AvailableTickets { get; set; }
    public bool IsPublished { get; set; }

    public Category? Category { get; set; }
    public User? Organizer { get; set; }
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
