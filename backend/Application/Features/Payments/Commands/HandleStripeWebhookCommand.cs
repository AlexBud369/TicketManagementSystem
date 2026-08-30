using Application.Interfaces;
using MediatR;

namespace Application.Features.Payments.Commands;

public sealed record HandleStripeWebhookCommand(
    string Payload,
    string Signature
) : IRequest<Unit>;

public sealed class HandleStripeWebhookCommandHandler
    : IRequestHandler<HandleStripeWebhookCommand, Unit> {
    private readonly IPaymentService _paymentService;

    public HandleStripeWebhookCommandHandler(
        IPaymentService paymentService) {
        _paymentService = paymentService;
    }

    public async Task<Unit> Handle(
        HandleStripeWebhookCommand request,
        CancellationToken cancellationToken) {
        await _paymentService.HandleWebhookAsync(
            request.Payload,
            request.Signature,
            cancellationToken);

        return Unit.Value;
    }
}
