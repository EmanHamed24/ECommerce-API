using ECommerce.Application.DTOs.Coupons;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Exceptions;
using ECommerce.Application.Interfaces;
using Moq;

namespace ECommerce.Tests.Unit;

public class CouponServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenDiscountIsGreaterThan100()
    {
        var repositoryMock = new Mock<IRepository<Coupon>>();

        var service = new CouponService(
            repositoryMock.Object);

        var request = new CreateCouponRequest
        {
            Code = "SAVE150",
            DiscountPercentage = 150,
            ExpirationDate = DateTime.UtcNow.AddDays(30)
        };

        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => service.CreateAsync(request));

        Assert.Equal(
            "Discount percentage must be greater than 0 and less than or equal to 100.",
            exception.Message);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateCouponSuccessfully()
    {
        var repositoryMock =
            new Mock<IRepository<Coupon>>();

        var service = new CouponService(
            repositoryMock.Object);

        var request = new CreateCouponRequest
        {
            Code = "SAVE10",
            DiscountPercentage = 10,
            ExpirationDate = DateTime.UtcNow.AddDays(30)
        };

        var result = await service.CreateAsync(request);

        Assert.NotNull(result);
        Assert.Equal("SAVE10", result.Code);
        Assert.Equal(10, result.DiscountPercentage);

        repositoryMock.Verify(
            r => r.AddAsync(It.Is<Coupon>(c =>
                c.Code == "SAVE10" &&
                c.DiscountPercentage == 10)),
            Times.Once);

        repositoryMock.Verify(
            r => r.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenDiscountIsZero()
    {
        var repositoryMock =
            new Mock<IRepository<Coupon>>();

        var service = new CouponService(
            repositoryMock.Object);

        var request = new CreateCouponRequest
        {
            Code = "SAVE0",
            DiscountPercentage = 0,
            ExpirationDate = DateTime.UtcNow.AddDays(30)
        };

        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => service.CreateAsync(request));

        Assert.Equal(
            "Discount percentage must be greater than 0 and less than or equal to 100.",
            exception.Message);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCoupon_WhenCouponExists()
    {
        var repositoryMock =
            new Mock<IRepository<Coupon>>();

        var coupon = new Coupon
        {
            Id = 1,
            Code = "SAVE10",
            DiscountPercentage = 10,
            ExpirationDate = DateTime.UtcNow.AddDays(30),
            IsActive = true
        };

        repositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(coupon);

        var service = new CouponService(
            repositoryMock.Object);

        var result = await service.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("SAVE10", result.Code);
        Assert.Equal(10, result.DiscountPercentage);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenCouponDoesNotExist()
    {
        var repositoryMock =
            new Mock<IRepository<Coupon>>();

        repositoryMock
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Coupon?)null);

        var service = new CouponService(
            repositoryMock.Object);

        var result = await service.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateCouponSuccessfully()
    {
        var repositoryMock =
            new Mock<IRepository<Coupon>>();

        var coupon = new Coupon
        {
            Id = 1,
            Code = "SAVE10",
            DiscountPercentage = 10,
            ExpirationDate = DateTime.UtcNow.AddDays(30),
            IsActive = true
        };

        repositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(coupon);

        var service = new CouponService(
            repositoryMock.Object);

        var request = new UpdateCouponRequest
        {
            DiscountPercentage = 20,
            ExpirationDate = DateTime.UtcNow.AddDays(60),
            IsActive = false
        };

        await service.UpdateAsync(1, request);

        Assert.Equal(20, coupon.DiscountPercentage);
        Assert.Equal(
            request.ExpirationDate,
            coupon.ExpirationDate);
        Assert.False(coupon.IsActive);

        repositoryMock.Verify(
            r => r.Update(coupon),
            Times.Once);

        repositoryMock.Verify(
            r => r.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenCouponDoesNotExist()
    {
        var repositoryMock =
            new Mock<IRepository<Coupon>>();

        repositoryMock
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Coupon?)null);

        var service = new CouponService(
            repositoryMock.Object);

        var request = new UpdateCouponRequest
        {
            DiscountPercentage = 20,
            ExpirationDate = DateTime.UtcNow.AddDays(60),
            IsActive = true
        };

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.UpdateAsync(999, request));

        Assert.Equal(
            "Coupon with ID 999 was not found.",
            exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenDiscountIsGreaterThan100()
    {
        var repositoryMock =
            new Mock<IRepository<Coupon>>();

        var coupon = new Coupon
        {
            Id = 1,
            Code = "SAVE10",
            DiscountPercentage = 10,
            ExpirationDate = DateTime.UtcNow.AddDays(30),
            IsActive = true
        };

        repositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(coupon);

        var service = new CouponService(
            repositoryMock.Object);

        var request = new UpdateCouponRequest
        {
            DiscountPercentage = 150,
            ExpirationDate = DateTime.UtcNow.AddDays(60),
            IsActive = true
        };

        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => service.UpdateAsync(1, request));

        Assert.Equal(
            "Discount percentage must be greater than 0 and less than or equal to 100.",
            exception.Message);

        repositoryMock.Verify(
            r => r.Update(It.IsAny<Coupon>()),
            Times.Never);

        repositoryMock.Verify(
            r => r.SaveChangesAsync(),
            Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenCouponDoesNotExist()
    {
        var repositoryMock =
            new Mock<IRepository<Coupon>>();

        repositoryMock
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Coupon?)null);

        var service = new CouponService(
            repositoryMock.Object);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.DeleteAsync(999));

        Assert.Equal(
            "Coupon with ID 999 was not found.",
            exception.Message);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteCouponSuccessfully()
    {
        var repositoryMock =
            new Mock<IRepository<Coupon>>();

        var coupon = new Coupon
        {
            Id = 1,
            Code = "SAVE10",
            DiscountPercentage = 10,
            ExpirationDate = DateTime.UtcNow.AddDays(30),
            IsActive = true
        };

        repositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(coupon);

        var service = new CouponService(
            repositoryMock.Object);

        await service.DeleteAsync(1);

        repositoryMock.Verify(
            r => r.Delete(coupon),
            Times.Once);

        repositoryMock.Verify(
            r => r.SaveChangesAsync(),
            Times.Once);
    }
}
