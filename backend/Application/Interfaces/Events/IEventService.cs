using Application.Common.Models;
using Application.DTOs.Events;
using Application.Interfaces.Events.Models;

namespace Application.Interfaces;

public interface IEventService {
    Task<EventDto> CreateAsync(
        CreateEventRequest request,
        CancellationToken cancellationToken = default);

    Task<EventDto> UpdateAsync(
        Guid eventId,
        UpdateEventRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid eventId,
        CancellationToken cancellationToken = default);

    Task PublishAsync(
        Guid eventId,
        CancellationToken cancellationToken = default);

    Task UnpublishAsync(
        Guid eventId,
        CancellationToken cancellationToken = default);

    Task<EventDto> GetByIdAsync(
        Guid eventId,
        CancellationToken cancellationToken = default);

    Task<PagedResult<EventListDto>> GetAllAsync(
        EventFilterRequest filter,
        CancellationToken cancellationToken = default);

    Task UpdateBannerAsync(
        Guid eventId,
        Guid imageId,
        CancellationToken cancellationToken = default);

    Task<bool> HasPaidOrdersAsync(
        Guid eventId,
        CancellationToken cancellationToken = default);
}
