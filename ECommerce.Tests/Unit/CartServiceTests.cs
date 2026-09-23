using ECommerce.Application.DTOs.Cart;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Exceptions;
using Moq;

namespace ECommerce.Tests.Unit;

public class CartServiceTests
{
    [Fact]
    public async Task AddToCartAsync_ShouldThrow_WhenQuantityIsZero()
    {
        var cartRepositoryMock =
            new Mock<IRepository<Cart>>();

        var cartItemRepositoryMock =
            new Mock<IRepository<CartItem>>();

        var productRepositoryMock =
            new Mock<IRepository<Product>>();

        var request = new AddToCartRequest
        {
            ProductId = 1,
            Quantity = 0
        };

        var service = new CartService(
            cartRepositoryMock.Object,
            cartItemRepositoryMock.Object,
            productRepositoryMock.Object);

        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => service.AddToCartAsync(1, request));

        Assert.Equal(
            "Quantity must be greater than zero.",
            exception.Message);
    }

    [Fact]
    public async Task AddToCartAsync_ShouldThrow_WhenProductIsNotAvailable()
    {
        var cartRepositoryMock =
            new Mock<IRepository<Cart>>();

        var cartItemRepositoryMock =
            new Mock<IRepository<CartItem>>();

        var productRepositoryMock =
            new Mock<IRepository<Product>>();

        productRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Product
            {
                Id = 1,
                Name = "Test Product",
                Price = 100,
                StockQuantity = 10,
                Status = "outofstock"
            });

        var service = new CartService(
            cartRepositoryMock.Object,
            cartItemRepositoryMock.Object,
            productRepositoryMock.Object);

        var request = new AddToCartRequest
        {
            ProductId = 1,
            Quantity = 1
        };

        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => service.AddToCartAsync(1, request));

        Assert.Equal(
            "This product is not available.",
            exception.Message);
    }

    [Fact]
    public async Task AddToCartAsync_ShouldThrow_WhenQuantityExceedsStock()
    {
        var cartRepositoryMock =
            new Mock<IRepository<Cart>>();

        var cartItemRepositoryMock =
            new Mock<IRepository<CartItem>>();

        var productRepositoryMock =
            new Mock<IRepository<Product>>();

        productRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Product
            {
                Id = 1,
                Name = "Test Product",
                Price = 100,
                StockQuantity = 2,
                Status = "Active"
            });

        var service = new CartService(
            cartRepositoryMock.Object,
            cartItemRepositoryMock.Object,
            productRepositoryMock.Object);

        var request = new AddToCartRequest
        {
            ProductId = 1,
            Quantity = 5
        };

        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => service.AddToCartAsync(1, request));

        Assert.Equal(
            "Requested quantity exceeds available stock.",
            exception.Message);
    }

    [Fact]
    public async Task AddToCartAsync_ShouldAddProductSuccessfully()
    {
        var cartRepositoryMock =
            new Mock<IRepository<Cart>>();

        var cartItemRepositoryMock =
            new Mock<IRepository<CartItem>>();

        var productRepositoryMock =
            new Mock<IRepository<Product>>();

        productRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Product
            {
                Id = 1,
                Name = "Test Product",
                Price = 100,
                StockQuantity = 10,
                Status = "Active"
            });

        var cart = new Cart
        {
            Id = 1,
            UserId = 1
        };

        cartRepositoryMock
            .SetupSequence(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Cart, bool>>>()))
            .ReturnsAsync((Cart?)null)
            .ReturnsAsync(cart);

        cartItemRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<CartItem, bool>>>()))
            .ReturnsAsync((CartItem?)null);

        var service = new CartService(
            cartRepositoryMock.Object,
            cartItemRepositoryMock.Object,
            productRepositoryMock.Object);

        var request = new AddToCartRequest
        {
            ProductId = 1,
            Quantity = 2
        };

        cartRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new Cart
            {
                Id = 1,
                UserId = 1
            });

        cartItemRepositoryMock
            .Setup(r => r.GetAllAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<CartItem, bool>>>()))
            .ReturnsAsync(new List<CartItem>());

        var result = await service.AddToCartAsync(1, request);

        cartRepositoryMock.Verify(
            r => r.AddAsync(It.Is<Cart>(c => c.UserId == 1)),
            Times.Once);

        cartItemRepositoryMock.Verify(
            r => r.AddAsync(It.Is<CartItem>(ci =>
                ci.ProductId == 1 &&
                ci.Quantity == 2)),
            Times.Once);
    }

    [Fact]
    public async Task AddToCartAsync_ShouldIncreaseQuantity_WhenProductAlreadyExistsInCart()
    {
        var cartRepositoryMock =
            new Mock<IRepository<Cart>>();

        var cartItemRepositoryMock =
            new Mock<IRepository<CartItem>>();

        var productRepositoryMock =
            new Mock<IRepository<Product>>();

        var cart = new Cart
        {
            Id = 1,
            UserId = 1
        };

        var existingCartItem = new CartItem
        {
            Id = 1,
            CartId = 1,
            ProductId = 1,
            Quantity = 2
        };

        productRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Product
            {
                Id = 1,
                Name = "Test Product",
                Price = 100,
                StockQuantity = 10,
                Status = "Active"
            });

        cartRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Cart, bool>>>()))
            .ReturnsAsync(cart);

        cartItemRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<CartItem, bool>>>()))
            .ReturnsAsync(existingCartItem);

        cartItemRepositoryMock
            .Setup(r => r.GetAllAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<CartItem, bool>>>()))
            .ReturnsAsync(new List<CartItem>
            {
            existingCartItem
            });

        var service = new CartService(
            cartRepositoryMock.Object,
            cartItemRepositoryMock.Object,
            productRepositoryMock.Object);

        var request = new AddToCartRequest
        {
            ProductId = 1,
            Quantity = 3
        };

        await service.AddToCartAsync(1, request);

        Assert.Equal(5, existingCartItem.Quantity);

        cartItemRepositoryMock.Verify(
            r => r.Update(It.Is<CartItem>(ci =>
                ci.ProductId == 1 &&
                ci.Quantity == 5)),
            Times.Once);

        cartItemRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<CartItem>()),
            Times.Never);
    }

    [Fact]
    public async Task AddToCartAsync_ShouldThrow_WhenExistingQuantityExceedsStock()
    {
        var cartRepositoryMock =
            new Mock<IRepository<Cart>>();

        var cartItemRepositoryMock =
            new Mock<IRepository<CartItem>>();

        var productRepositoryMock =
            new Mock<IRepository<Product>>();

        var cart = new Cart
        {
            Id = 1,
            UserId = 1
        };

        var existingCartItem = new CartItem
        {
            Id = 1,
            CartId = 1,
            ProductId = 1,
            Quantity = 4
        };

        productRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Product
            {
                Id = 1,
                Name = "Test Product",
                Price = 100,
                StockQuantity = 5,
                Status = "Active"
            });

        cartRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Cart, bool>>>()))
            .ReturnsAsync(cart);

        cartItemRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<CartItem, bool>>>()))
            .ReturnsAsync(existingCartItem);

        var service = new CartService(
            cartRepositoryMock.Object,
            cartItemRepositoryMock.Object,
            productRepositoryMock.Object);

        var request = new AddToCartRequest
        {
            ProductId = 1,
            Quantity = 2
        };

        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => service.AddToCartAsync(1, request));

        Assert.Equal(
            "Requested quantity exceeds available stock.",
            exception.Message);

        Assert.Equal(4, existingCartItem.Quantity);
    }

    [Fact]
    public async Task AddToCartAsync_ShouldThrow_WhenProductDoesNotExist()
    {
        var cartRepositoryMock =
            new Mock<IRepository<Cart>>();

        var cartItemRepositoryMock =
            new Mock<IRepository<CartItem>>();

        var productRepositoryMock =
            new Mock<IRepository<Product>>();

        productRepositoryMock
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Product?)null);

        var service = new CartService(
            cartRepositoryMock.Object,
            cartItemRepositoryMock.Object,
            productRepositoryMock.Object);

        var request = new AddToCartRequest
        {
            ProductId = 999,
            Quantity = 1
        };

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.AddToCartAsync(1, request));

        Assert.Equal(
            "Product with ID 999 was not found.",
            exception.Message);
    }
}
