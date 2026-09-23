using System.Security.Claims;
using ECommerce.Application.DTOs.Cart;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[Authorize(Roles = "Customer")]
[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet]
    public async Task<ActionResult<CartResponse>> GetCart()
    {
        var userId = GetUserId();

        var cart = await _cartService.GetCartAsync(userId);

        return Ok(cart);
    }

    [HttpPost("items")]
    public async Task<ActionResult<CartResponse>> AddToCart(
        AddToCartRequest request)
    {
        var userId = GetUserId();

        var cart = await _cartService.AddToCartAsync(
            userId,
            request);

        return Ok(cart);
    }

    [HttpDelete("items/{productId:int}")]
    public async Task<IActionResult> RemoveFromCart(
        int productId)
    {
        var userId = GetUserId();

        await _cartService.RemoveFromCartAsync(
            userId,
            productId);

        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        var userId = GetUserId();

        await _cartService.ClearCartAsync(userId);

        return NoContent();
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