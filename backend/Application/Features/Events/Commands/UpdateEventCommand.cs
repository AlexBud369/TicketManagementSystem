using Application.Common;
using Application.DTOs.Events;
using Application.Interfaces;
using Application.Interfaces.Events.Models;
using MediatR;

namespace Application.Features.Events.Commands;

public sealed record UpdateEventCommand(
    Guid EventId,
    string Title,
    string Description,
    Guid CategoryId,
    string Location,
    DateTime Date,
    TimeSpan StartTime,
    TimeSpan EndTime,
    int MaximumCapacity,
    decimal TicketPrice,
    bool AllowCancellation,
    double? Latitude,
    double? Longitude
) : IRequest<EventDto>;

public sealed class UpdateEventCommandHandler
    : IRequestHandler<UpdateEventCommand, EventDto> {
    private readonly IEventService _eventService;
    private readonly ICategoryService _categoryService;

    public UpdateEventCommandHandler(
        IEventService eventService,
        ICategoryService categoryService) {
        _eventService = eventService;
        _categoryService = categoryService;
    }

    public async Task<EventDto> Handle(
        UpdateEventCommand request,
        CancellationToken cancellationToken) {
        var categoryExists = await _categoryService.ExistsAsync(
            request.CategoryId, cancellationToken);

        Guard.AgainstBusinessRule(
            !categoryExists,
            "The selected category does not exist.");

        var updateRequest = new UpdateEventRequest {
            Title = request.Title,
            Description = request.Description,
            CategoryId = request.CategoryId,
            Location = request.Location,
            Date = request.Date,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            MaximumCapacity = request.MaximumCapacity,
            TicketPrice = request.TicketPrice,
            AllowCancellation = request.AllowCancellation,
            Latitude = request.Latitude,
            Longitude = request.Longitude
        };

        return await _eventService.UpdateAsync(
            request.EventId, updateRequest, cancellationToken);
    }
}
