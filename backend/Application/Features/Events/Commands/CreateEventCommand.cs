using Application.Common;
using Application.DTOs.Events;
using Application.Interfaces;
using Application.Interfaces.Events.Models;
using MediatR;

namespace Application.Features.Events.Commands;

public sealed record CreateEventCommand(
    string Title,
    string Description,
    Guid CategoryId,
    string Location,
    DateTime Date,
    TimeSpan StartTime,
    TimeSpan EndTime,
    int MaximumCapacity,
    decimal TicketPrice,
    Guid OrganizerId,
    bool AllowCancellation,
    double? Latitude,
    double? Longitude
) : IRequest<EventDto>;

public sealed class CreateEventCommandHandler
    : IRequestHandler<CreateEventCommand, EventDto> {
    private readonly IEventService _eventService;
    private readonly ICategoryService _categoryService;

    public CreateEventCommandHandler(
        IEventService eventService,
        ICategoryService categoryService) {
        _eventService = eventService;
        _categoryService = categoryService;
    }

    public async Task<EventDto> Handle(
        CreateEventCommand request,
        CancellationToken cancellationToken) {
        var categoryExists = await _categoryService.ExistsAsync(
            request.CategoryId, cancellationToken);

        Guard.AgainstBusinessRule(
            !categoryExists,
            "The selected category does not exist.");

        var createRequest = new CreateEventRequest {
            Title = request.Title,
            Description = request.Description,
            CategoryId = request.CategoryId,
            Location = request.Location,
            Date = request.Date,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            MaximumCapacity = request.MaximumCapacity,
            TicketPrice = request.TicketPrice,
            OrganizerId = request.OrganizerId,
            AllowCancellation = request.AllowCancellation,
            Latitude = request.Latitude,
            Longitude = request.Longitude
        };

        return await _eventService.CreateAsync(
            createRequest, cancellationToken);
    }
}
