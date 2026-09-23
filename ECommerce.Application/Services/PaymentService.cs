using ECommerce.Application.DTOs.Payments;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Constants;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Exceptions;

namespace ECommerce.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IRepository<Order> _orderRepository;
    private readonly IRepository<Payment> _paymentRepository;

    public PaymentService(
        IRepository<Order> orderRepository,
        IRepository<Payment> paymentRepository)
    {
        _orderRepository = orderRepository;
        _paymentRepository = paymentRepository;
    }

    public async Task<PaymentResponse> ProcessPaymentAsync(
        int userId,
        ProcessPaymentRequest request)
    {
        var order = await _orderRepository
            .FirstOrDefaultAsync(
                o => o.Id == request.OrderId &&
                     o.UserId == userId);

        if (order is null)
        {
            throw new KeyNotFoundException(
                $"Order with ID {request.OrderId} was not found.");
        }

        if (order.Status == OrderStatuses.Cancelled)
        {
            throw new BusinessException(
                "Cannot pay for a cancelled order.");
        }

        var existingPayment = await _paymentRepository
            .FirstOrDefaultAsync(
                p => p.OrderId == order.Id);

        if (existingPayment is not null)
        {
            throw new BusinessException(
                "Payment has already been processed for this order.");
        }

        var payment = new Payment
        {
            OrderId = order.Id,
            Amount = order.FinalAmount,
            Status = PaymentStatuses.Successful,
            TransactionId = Guid.NewGuid().ToString("N"),
            PaidAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        order.Status = OrderStatuses.Confirmed;
        order.UpdatedAt = DateTime.UtcNow;

        await _paymentRepository.AddAsync(payment);

        _orderRepository.Update(order);

        await _paymentRepository.SaveChangesAsync();

        return new PaymentResponse
        {
            PaymentId = payment.Id,
            OrderId = payment.OrderId,
            Amount = payment.Amount,
            Status = payment.Status,
            TransactionId = payment.TransactionId,
            PaidAt = payment.PaidAt,
            CreatedAt = payment.CreatedAt
        };
    }
}