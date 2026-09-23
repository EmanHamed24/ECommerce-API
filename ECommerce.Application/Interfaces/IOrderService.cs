using ECommerce.Application.DTOs.Orders;

namespace ECommerce.Application.Interfaces;

public interface IOrderService
{
    Task<OrderResponse> CreateOrderAsync(
        int userId,
        CreateOrderRequest request);

    Task<IEnumerable<OrderResponse>> GetMyOrdersAsync(
        int userId);

    Task<OrderResponse?> GetByIdAsync(
        int userId,
        int orderId);
}