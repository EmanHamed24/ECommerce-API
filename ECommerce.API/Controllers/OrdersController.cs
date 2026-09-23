using System.Security.Claims;
using ECommerce.Application.DTOs.Orders;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[Authorize(Roles = "Customer")]
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<ActionResult<OrderResponse>> CreateOrder(
        CreateOrderRequest request)
    {
        var userId = GetUserId();

        var order = await _orderService.CreateOrderAsync(
            userId,
            request);

        return Ok(order);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderResponse>>> GetMyOrders()
    {
        var userId = GetUserId();

        var orders = await _orderService.GetMyOrdersAsync(userId);

        return Ok(orders);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderResponse>> GetById(int id)
    {
        var userId = GetUserId();

        var order = await _orderService.GetByIdAsync(
            userId,
            id);

        if (order is null)
        {
            return NotFound();
        }

        return Ok(order);
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