using ECommerce.Application.DTOs.Payments;

namespace ECommerce.Application.Interfaces;

public interface IPaymentService
{
    Task<PaymentResponse> ProcessPaymentAsync(
        int userId,
        ProcessPaymentRequest request);
}