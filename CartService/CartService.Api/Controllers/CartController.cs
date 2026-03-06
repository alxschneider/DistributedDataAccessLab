using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CartService.Api.Data;
using CartService.Api.Models;
using CartService.Api.Services;

namespace CartService.Api.Controllers;

[ApiController]
[Route("api/cart")]
public class CartController : ControllerBase
{
    private readonly CartDbContext _context;
    private readonly IProductClient _productClient;
    private readonly IOrderClient _orderClient;
    private readonly INotificationClient _notificationClient;

    public CartController(CartDbContext context, IProductClient productClient, IOrderClient orderClient, INotificationClient notificationClient)
    {
        _context = context;
        _productClient = productClient;
        _orderClient = orderClient;
        _notificationClient = notificationClient;
    }

    //!!!IMporant: In a bigger or prod enviroment, its normal to have the business logic in a separate service layer, and the controller just calls that service.
    //  For simplicity, we put all logic in the controller here.!!!

    /// <summary>
    /// Get cart with items for a buyer
    /// </summary>
    [HttpGet("{buyerId}")]
    public async Task<IActionResult> GetCart(int buyerId)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.BuyerId == buyerId);

        if (cart == null)
            return Ok(new { BuyerId = buyerId, Items = Array.Empty<object>(), Total = 0m });

        var total = cart.Items.Sum(i =>
        {
            var finalPrice = i.UnitPrice * (1 - i.DiscountPercent / 100);
            return finalPrice * i.Quantity;
        });

        return Ok(new
        {
            cart.Id,
            cart.BuyerId,
            cart.Items,
            Total = Math.Round(total, 2),
            cart.CreatedAt,
            cart.UpdatedAt
        });
    }

    /// <summary>
    /// Add item to cart (validates product via ProductService)
    /// </summary>
    [HttpPost("{buyerId}/items")]
    public async Task<IActionResult> AddItem(int buyerId, [FromBody] AddCartItemRequest request)
    {
        // Validate product
        var product = await _productClient.GetProductAsync(request.ProductId);
        if (product == null)
            return BadRequest($"Product {request.ProductId} does not exist.");

        if (product.Stock < request.Quantity)
            return BadRequest($"Insufficient stock for '{product.Name}'. Available: {product.Stock}");

        // Get or create cart
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.BuyerId == buyerId);

        if (cart == null)
        {
            cart = new Cart { BuyerId = buyerId, CreatedAt = DateTime.UtcNow };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
        }

        // Check if product already in cart
        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
        if (existingItem != null)
        {
            existingItem.Quantity += request.Quantity;
            existingItem.UnitPrice = product.Price;
            existingItem.DiscountPercent = product.DiscountPercent;
        }
        else
        {
            cart.Items.Add(new CartItem
            {
                ProductId = product.Id,
                SellerId = product.SellerId,
                ProductName = product.Name,
                Quantity = request.Quantity,
                UnitPrice = product.Price,
                DiscountPercent = product.DiscountPercent
            });
        }

        cart.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Notify buyer
        await _notificationClient.SendNotificationAsync(
            buyerId,
            "CartItemAdded",
            "Item added to cart",
            $"'{product.Name}' (x{request.Quantity}) has been added to your cart.");

        // Notify seller
        await _notificationClient.SendNotificationAsync(
            product.SellerId,
            "CartItemAdded",
            "Someone added your course to cart",
            $"A buyer added '{product.Name}' (x{request.Quantity}) to their cart.");

        return Ok(cart);
    }

    /// <summary>
    /// Update item quantity
    /// </summary>
    [HttpPut("{buyerId}/items/{itemId}")]
    public async Task<IActionResult> UpdateItem(int buyerId, int itemId, [FromBody] UpdateCartItemRequest request)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.BuyerId == buyerId);

        if (cart == null)
            return NotFound("Cart not found.");

        var item = cart.Items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            return NotFound("Item not found in cart.");

        if (request.Quantity <= 0)
        {
            cart.Items.Remove(item);
        }
        else
        {
            item.Quantity = request.Quantity;
        }

        cart.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(cart);
    }

    /// <summary>
    /// Remove item from cart
    /// </summary>
    [HttpDelete("{buyerId}/items/{itemId}")]
    public async Task<IActionResult> RemoveItem(int buyerId, int itemId)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.BuyerId == buyerId);

        if (cart == null)
            return NotFound("Cart not found.");

        var item = cart.Items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            return NotFound("Item not found in cart.");

        cart.Items.Remove(item);
        cart.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Clear entire cart
    /// </summary>
    [HttpDelete("{buyerId}")]
    public async Task<IActionResult> ClearCart(int buyerId)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.BuyerId == buyerId);

        if (cart == null)
            return NotFound("Cart not found.");

        _context.CartItems.RemoveRange(cart.Items);
        _context.Carts.Remove(cart);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Checkout: creates an order from the cart, then clears the cart
    /// </summary>
    [HttpPost("{buyerId}/checkout")]
    public async Task<IActionResult> Checkout(int buyerId)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.BuyerId == buyerId);

        if (cart == null || cart.Items.Count == 0)
            return BadRequest("Cart is empty.");

        var orderItems = cart.Items.Select(i => new OrderItemRequest
        {
            ProductId = i.ProductId,
            Quantity = i.Quantity
        }).ToList();

        var orderId = await _orderClient.CreateOrderAsync(buyerId, orderItems);
        if (orderId == null)
            return BadRequest("Failed to create order. Check product availability.");

        // Clear cart after successful checkout
        _context.CartItems.RemoveRange(cart.Items);
        _context.Carts.Remove(cart);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Checkout successful", OrderId = orderId });
    }
}

public class AddCartItemRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class UpdateCartItemRequest
{
    public int Quantity { get; set; }
}
