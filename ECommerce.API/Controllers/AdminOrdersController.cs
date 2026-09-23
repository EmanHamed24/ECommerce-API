using ECommerce.Application.DTOs.Orders;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin/orders")]
public class AdminOrdersController : ControllerBase
{
    private readonly IAdminOrderService _adminOrderService;

    public AdminOrdersController(
        IAdminOrderService adminOrderService)
    {
        _adminOrderService = adminOrderService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderResponse>>> GetAllOrders()
    {
        var orders = await _adminOrderService.GetAllOrdersAsync();

        return Ok(orders);
    }

    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateOrderStatus(
        int id,
        UpdateOrderStatusRequest request)
    {
        await _adminOrderService.UpdateOrderStatusAsync(
            id,
            request);

        return NoContent();
    }
}
