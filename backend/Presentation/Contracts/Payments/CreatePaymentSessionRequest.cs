namespace Presentation.Contracts.Payments;

public sealed record CreatePaymentSessionRequest(Guid OrderId);
