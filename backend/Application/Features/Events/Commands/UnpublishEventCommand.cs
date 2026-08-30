using Application.Interfaces;
using MediatR;

namespace Application.Features.Events.Commands;

public sealed record UnpublishEventCommand(Guid EventId) : IRequest<Unit>;

public sealed class UnpublishEventCommandHandler
    : IRequestHandler<UnpublishEventCommand, Unit> {
    private readonly IEventService _eventService;
    private readonly IOrderService _orderService;
    private readonly IEmailService _emailService;

    public UnpublishEventCommandHandler(
        IEventService eventService,
        IOrderService orderService,
        IEmailService emailService) {
        _eventService = eventService;
        _orderService = orderService;
        _emailService = emailService;
    }

    public async Task<Unit> Handle(
        UnpublishEventCommand request,
        CancellationToken cancellationToken) {
        var evt = await _eventService.GetByIdAsync(
            request.EventId, cancellationToken);

        var buyers = await _orderService.GetPaidBuyersByEventAsync(
            request.EventId, cancellationToken);

        await _eventService.UnpublishAsync(
            request.EventId,
            cancellationToken);

        foreach (var buyer in buyers) {
            await _emailService.SendEventCancellationEmailAsync(
                buyer.Email,
                buyer.UserName,
                evt.Title,
                cancellationToken);
        }

        return Unit.Value;
    }
}
