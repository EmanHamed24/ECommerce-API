using ECommerce.Application.DTOs.Cart;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Constants;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Exceptions;

namespace ECommerce.Application.Services;

public class CartService : ICartService
{
    private readonly IRepository<Cart> _cartRepository;
    private readonly IRepository<CartItem> _cartItemRepository;
    private readonly IRepository<Product> _productRepository;

    public CartService(
        IRepository<Cart> cartRepository,
        IRepository<CartItem> cartItemRepository,
        IRepository<Product> productRepository)
    {
        _cartRepository = cartRepository;
        _cartItemRepository = cartItemRepository;
        _productRepository = productRepository;
    }

    public async Task<CartResponse> GetCartAsync(int userId)
    {
        var cart = await _cartRepository
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart is null)
        {
            cart = new Cart
            {
                UserId = userId
            };

            await _cartRepository.AddAsync(cart);
            await _cartRepository.SaveChangesAsync();
        }

        var items = (await _cartItemRepository.GetAllAsync(
              ci => ci.CartId == cart.Id))
              .ToList();

        var responseItems = new List<CartItemResponse>();

        foreach (var item in items)
        {
            var product = await _productRepository
                .GetByIdAsync(item.ProductId);

            if (product is null)
            {
                continue;
            }

            responseItems.Add(new CartItemResponse
            {
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = item.Quantity,
                TotalPrice = product.Price * item.Quantity
            });
        }

        return new CartResponse
        {
            CartId = cart.Id,
            Items = responseItems,
            TotalAmount = responseItems.Sum(x => x.TotalPrice)
        };
    }

    public async Task<CartResponse> AddToCartAsync(
        int userId,
        AddToCartRequest request)
    {
        if (request.Quantity <= 0)
        {
            throw new BusinessException(
                "Quantity must be greater than zero.");
        }

        var product = await _productRepository
            .GetByIdAsync(request.ProductId);

        if (product is null)
        {
            throw new KeyNotFoundException(
                $"Product with ID {request.ProductId} was not found.");
        }

        if (product.Status != ProductStatuses.Active)
        {
            throw new BusinessException(
                "This product is not available.");
        }

        if (request.Quantity > product.StockQuantity)
        {
            throw new BusinessException(
                "Requested quantity exceeds available stock.");
        }

        var cart = await _cartRepository
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart is null)
        {
            cart = new Cart
            {
                UserId = userId
            };

            await _cartRepository.AddAsync(cart);
            await _cartRepository.SaveChangesAsync();
        }

        var existingItem = await _cartItemRepository
            .FirstOrDefaultAsync(ci =>
                ci.CartId == cart.Id &&
                ci.ProductId == request.ProductId);

        if (existingItem is not null)
        {
            var newQuantity =
                existingItem.Quantity + request.Quantity;

            if (newQuantity > product.StockQuantity)
            {
                throw new BusinessException(
                    "Requested quantity exceeds available stock.");
            }

            existingItem.Quantity = newQuantity;

            _cartItemRepository.Update(existingItem);
        }
        else
        {
            var cartItem = new CartItem
            {
                CartId = cart.Id,
                ProductId = product.Id,
                Quantity = request.Quantity
            };

            await _cartItemRepository.AddAsync(cartItem);
        }

        cart.UpdatedAt = DateTime.UtcNow;

        _cartRepository.Update(cart);

        await _cartItemRepository.SaveChangesAsync();

        return await GetCartAsync(userId);
    }

    public async Task RemoveFromCartAsync(
        int userId,
        int productId)
    {
        var cart = await _cartRepository
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart is null)
        {
            throw new KeyNotFoundException(
                "Cart was not found.");
        }

        var cartItem = await _cartItemRepository
            .FirstOrDefaultAsync(ci =>
                ci.CartId == cart.Id &&
                ci.ProductId == productId);

        if (cartItem is null)
        {
            throw new KeyNotFoundException(
                "Product was not found in the cart.");
        }

        _cartItemRepository.Delete(cartItem);

        cart.UpdatedAt = DateTime.UtcNow;

        _cartRepository.Update(cart);

        await _cartItemRepository.SaveChangesAsync();
    }

    public async Task ClearCartAsync(int userId)
    {
        var cart = await _cartRepository
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart is null)
        {
            return;
        }

        var cartItems = await _cartItemRepository.GetAllAsync();

        var userItems = cartItems
            .Where(ci => ci.CartId == cart.Id)
            .ToList();

        foreach (var item in userItems)
        {
            _cartItemRepository.Delete(item);
        }

        cart.UpdatedAt = DateTime.UtcNow;

        _cartRepository.Update(cart);

        await _cartItemRepository.SaveChangesAsync();
    }
}
