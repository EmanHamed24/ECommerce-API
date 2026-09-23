using ECommerce.Application.DTOs.Payments;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;
using ECommerce.Domain.Constants;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Exceptions;
using Moq;

namespace ECommerce.Tests.Unit;

public class PaymentServiceTests
{
    [Fact]
    public async Task ProcessPaymentAsync_ShouldThrow_WhenPaymentAlreadyExists()
    {
        var orderRepositoryMock =
            new Mock<IRepository<Order>>();

        var paymentRepositoryMock =
            new Mock<IRepository<Payment>>();

        orderRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>()))
            .ReturnsAsync(new Order
            {
                Id = 1,
                UserId = 1,
                FinalAmount = 1000,
                Status = OrderStatuses.Pending
            });

        paymentRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Payment, bool>>>()))
            .ReturnsAsync(new Payment
            {
                Id = 1,
                OrderId = 1,
                Amount = 1000,
                Status = PaymentStatuses.Successful
            });

        var service = new PaymentService(
            orderRepositoryMock.Object,
            paymentRepositoryMock.Object);

        var request = new ProcessPaymentRequest
        {
            OrderId = 1
        };

        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => service.ProcessPaymentAsync(1, request));

        Assert.Equal(
            "Payment has already been processed for this order.",
            exception.Message);
    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldThrow_WhenOrderDoesNotExist()
    {
        var orderRepositoryMock =
            new Mock<IRepository<Order>>();

        var paymentRepositoryMock =
            new Mock<IRepository<Payment>>();

        orderRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>()))
            .ReturnsAsync((Order?)null);

        var service = new PaymentService(
            orderRepositoryMock.Object,
            paymentRepositoryMock.Object);

        var request = new ProcessPaymentRequest
        {
            OrderId = 999
        };

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.ProcessPaymentAsync(1, request));

        Assert.Equal(
            "Order with ID 999 was not found.",
            exception.Message);
    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldThrow_WhenOrderBelongsToAnotherUser()
    {
        var orderRepositoryMock =
            new Mock<IRepository<Order>>();

        var paymentRepositoryMock =
            new Mock<IRepository<Payment>>();

        orderRepositoryMock
           .Setup(r => r.FirstOrDefaultAsync(
               It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>()))
           .ReturnsAsync((Order?)null);

        var service = new PaymentService(
            orderRepositoryMock.Object,
            paymentRepositoryMock.Object);

        var request = new ProcessPaymentRequest
        {
            OrderId = 1
        };

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.ProcessPaymentAsync(1, request));

        Assert.Equal(
            "Order with ID 1 was not found.",
            exception.Message);
    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldThrow_WhenOrderIsCancelled()
    {
        var orderRepositoryMock =
            new Mock<IRepository<Order>>();

        var paymentRepositoryMock =
            new Mock<IRepository<Payment>>();

        orderRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>()))
            .ReturnsAsync(new Order
            {
                Id = 1,
                UserId = 1,
                FinalAmount = 1000,
                Status = OrderStatuses.Cancelled
            });

        var service = new PaymentService(
            orderRepositoryMock.Object,
            paymentRepositoryMock.Object);

        var request = new ProcessPaymentRequest
        {
            OrderId = 1
        };

        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => service.ProcessPaymentAsync(1, request));

        Assert.Equal(
            "Cannot pay for a cancelled order.",
            exception.Message);
    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldProcessPaymentSuccessfully()
    {
        var orderRepositoryMock =
            new Mock<IRepository<Order>>();

        var paymentRepositoryMock =
            new Mock<IRepository<Payment>>();

        var order = new Order
        {
            Id = 1,
            UserId = 1,
            FinalAmount = 1000,
            Status = OrderStatuses.Pending
        };

        orderRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>()))
            .ReturnsAsync(order);

        paymentRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Payment, bool>>>()))
            .ReturnsAsync((Payment?)null);

        var service = new PaymentService(
            orderRepositoryMock.Object,
            paymentRepositoryMock.Object);

        var request = new ProcessPaymentRequest
        {
            OrderId = 1
        };

        var result = await service.ProcessPaymentAsync(1, request);

        Assert.NotNull(result);
        Assert.Equal(1, result.OrderId);
        Assert.Equal(1000, result.Amount);
        Assert.Equal(
            PaymentStatuses.Successful,
            result.Status);

        Assert.Equal(
            OrderStatuses.Confirmed,
            order.Status);

        paymentRepositoryMock.Verify(
            r => r.AddAsync(It.Is<Payment>(
                p =>
                    p.OrderId == 1 &&
                    p.Amount == 1000 &&
                    p.Status == PaymentStatuses.Successful)),
            Times.Once);

        orderRepositoryMock.Verify(
            r => r.Update(order),
            Times.Once);
    }
}