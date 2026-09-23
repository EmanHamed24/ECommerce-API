using ECommerce.Application.DTOs.Orders;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Constants;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Exceptions;

namespace ECommerce.Application.Services;

public class AdminOrderService : IAdminOrderService
{
    private readonly IRepository<Order> _orderRepository;

    public AdminOrderService(
        IRepository<Order> orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<IEnumerable<OrderResponse>> GetAllOrdersAsync()
    {
        var orders = await _orderRepository.GetAllAsync(
            o => true,
            o => o.OrderItems);

        return orders.Select(order => new OrderResponse
        {
            Id = order.Id,
            OrderDate = order.OrderDate,
            TotalAmount = order.TotalAmount,
            DiscountAmount = order.DiscountAmount,
            FinalAmount = order.FinalAmount,
            Status = order.Status,

            Items = order.OrderItems
                .Select(item => new OrderItemResponse
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice
                })
                .ToList()
        });
    }

    public async Task UpdateOrderStatusAsync(
        int orderId,
        UpdateOrderStatusRequest request)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);

        if (order is null)
        {
            throw new KeyNotFoundException(
                $"Order with ID {orderId} was not found.");
        }

        var validStatuses = new[]
        {
            OrderStatuses.Pending,
            OrderStatuses.Confirmed,
            OrderStatuses.Processing,
            OrderStatuses.Shipped,
            OrderStatuses.Delivered,
            OrderStatuses.Cancelled
        };

        if (!validStatuses.Contains(request.Status))
        {
            throw new BusinessException(
                $"Invalid order status '{request.Status}'.");
        }

        if (order.Status == OrderStatuses.Delivered)
        {
            throw new BusinessException(
                "A delivered order cannot be modified.");
        }

        if (order.Status == OrderStatuses.Cancelled)
        {
            throw new BusinessException(
                "A cancelled order cannot be modified.");
        }

        order.Status = request.Status;
        order.UpdatedAt = DateTime.UtcNow;

        _orderRepository.Update(order);

        await _orderRepository.SaveChangesAsync();
    }
}
