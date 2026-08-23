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
    public Guid? BannerImageId { get; set; }
    public int MaxCapacity { get; set; }
    public decimal TicketPrice { get; set; }
    public int AvailableTickets { get; set; }
    public bool IsPublished { get; set; }
    public bool AllowCancellation { get; set; } = true;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public Category? Category { get; set; }
    public User? Organizer { get; set; }
    public Image? BannerImage { get; set; }
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
