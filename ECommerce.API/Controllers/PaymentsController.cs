using System.Security.Claims;
using ECommerce.Application.DTOs.Payments;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[Authorize(Roles = "Customer")]
[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost]
    public async Task<ActionResult<PaymentResponse>> ProcessPayment(
        ProcessPaymentRequest request)
    {
        var userId = GetUserId();

        var payment = await _paymentService.ProcessPaymentAsync(
            userId,
            request);

        return Ok(payment);
    }

    private int GetUserId()
    {
        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var userId))
        {
            throw new UnauthorizedAccessException(
                "User ID was not found in the token.");
        }

        return userId;
    }
}