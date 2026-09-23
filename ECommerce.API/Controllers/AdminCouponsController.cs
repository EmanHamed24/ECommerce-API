using ECommerce.Application.DTOs.Coupons;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin/coupons")]
public class AdminCouponsController : ControllerBase
{
    private readonly ICouponService _couponService;

    public AdminCouponsController(
        ICouponService couponService)
    {
        _couponService = couponService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CouponResponse>>> GetAll()
    {
        var coupons = await _couponService.GetAllAsync();

        return Ok(coupons);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CouponResponse>> GetById(int id)
    {
        var coupon = await _couponService.GetByIdAsync(id);

        if (coupon is null)
        {
            return NotFound();
        }

        return Ok(coupon);
    }

    [HttpPost]
    public async Task<ActionResult<CouponResponse>> Create(
        CreateCouponRequest request)
    {
        var coupon = await _couponService.CreateAsync(request);

        return CreatedAtAction(
            nameof(GetById),
            new { id = coupon.Id },
            coupon);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        UpdateCouponRequest request)
    {
        await _couponService.UpdateAsync(id, request);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _couponService.DeleteAsync(id);

        return NoContent();
    }
}
