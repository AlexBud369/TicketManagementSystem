using Application.Common.Models;
using Application.DTOs.Events;
using Application.Interfaces;
using Application.Interfaces.Events.Models;
using MediatR;

namespace Application.Features.Events.Queries;

public sealed record GetAllEventsQuery(
    string Search,
    Guid? CategoryId,
    DateTime? DateFrom,
    DateTime? DateTo,
    decimal? MinPrice,
    decimal? MaxPrice,
    bool? HasAvailableTickets,
    string SortBy,
    bool Descending,
    int Page,
    int PageSize
) : IRequest<PagedResult<EventListDto>>;

public sealed class GetAllEventsQueryHandler
    : IRequestHandler<GetAllEventsQuery, PagedResult<EventListDto>> {
    private readonly IEventService _eventService;

    public GetAllEventsQueryHandler(IEventService eventService) {
        _eventService = eventService;
    }

    public async Task<PagedResult<EventListDto>> Handle(
        GetAllEventsQuery request,
        CancellationToken cancellationToken) {
        var pagination = new PaginationParams {
            Page = request.Page,
            PageSize = request.PageSize
        };

        var filter = new EventFilterRequest {
            Search = request.Search,
            CategoryId = request.CategoryId,
            DateFrom = request.DateFrom,
            DateTo = request.DateTo,
            MinPrice = request.MinPrice,
            MaxPrice = request.MaxPrice,
            HasAvailableTickets = request.HasAvailableTickets,
            SortBy = request.SortBy,
            Descending = request.Descending,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };

        return await _eventService.GetAllAsync(filter, cancellationToken);
    }
}
