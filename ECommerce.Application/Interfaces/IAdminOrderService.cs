using ECommerce.Application.DTOs.Orders;

namespace ECommerce.Application.Interfaces;

public interface IAdminOrderService
{
    Task<IEnumerable<OrderResponse>> GetAllOrdersAsync();

    Task UpdateOrderStatusAsync(
        int orderId,
        UpdateOrderStatusRequest request);
}