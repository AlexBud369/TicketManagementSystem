using Application.Common;
using Application.Interfaces;
using Domain.Enums;
using MediatR;

namespace Application.Features.Payments.Commands;

public sealed record CreatePaymentSessionCommand(
    Guid OrderId,
    Guid UserId
) : IRequest<string>;

public sealed class CreatePaymentSessionCommandHandler
    : IRequestHandler<CreatePaymentSessionCommand, string> {
    private readonly IOrderService _orderService;
    private readonly IPaymentService _paymentService;
    private readonly IAppSettings _appSettings;

    public CreatePaymentSessionCommandHandler(
        IOrderService orderService,
        IPaymentService paymentService,
        IAppSettings appSettings) {
        _orderService = orderService;
        _paymentService = paymentService;
        _appSettings = appSettings;
    }

    public async Task<string> Handle(
        CreatePaymentSessionCommand request,
        CancellationToken cancellationToken) {
        var order = await _orderService.GetByIdAsync(
            request.OrderId, cancellationToken);

        Guard.AgainstNotFound(order, "Order", request.OrderId);

        Guard.AgainstUnauthorizedAccess(
            request.UserId, order.UserId);

        Guard.AgainstBusinessRule(
            order.Status != OrderStatus.Pending,
            "Only pending orders can be paid.");

        var frontend = _appSettings.FrontendUrl.TrimEnd('/');
        var successUrl = $"{frontend}/payments/success?orderId={order.Id}";
        var cancelUrl = $"{frontend}/payments/cancel?orderId={order.Id}";

        return await _paymentService.CreateCheckoutSessionAsync(
            request.OrderId,
            successUrl,
            cancelUrl,
            cancellationToken);
    }
}
