using Application.Common.Models;
using Application.DTOs.Orders;
using Application.Interfaces.Orders.Models;

namespace Application.Interfaces;

public interface IOrderService {
    Task<OrderDto> CreateAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken = default);

    Task CancelAsync(
        Guid orderId,
        Guid userId,
        bool isAdmin,
        CancellationToken cancellationToken = default);

    Task RefundAsync(
        Guid orderId,
        CancellationToken cancellationToken = default);

    Task<OrderDto> GetByIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default);

    Task<PagedResult<OrderListDto>> GetMyOrdersAsync(
        Guid userId,
        OrderFilterRequest filter,
        CancellationToken cancellationToken = default);

    Task<PagedResult<OrderListDto>> GetAllAsync(
        OrderFilterRequest filter,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<EventBuyerDto>> GetPaidBuyersByEventAsync(
        Guid eventId,
        CancellationToken cancellationToken = default);
}
