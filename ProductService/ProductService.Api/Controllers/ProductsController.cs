using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductService.Api.Data;
using ProductService.Api.Models;
using ProductService.Api.Services;

namespace ProductService.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly ProductDbContext _context;
    private readonly ICustomerClient _customerClient;

    public ProductsController(ProductDbContext context, ICustomerClient customerClient)
    {
        _context = context;
        _customerClient = customerClient;
    }

    //!!!IMporant: In a bigger or prod enviroment, its normal to have the business logic in a separate service layer, and the controller just calls that service.
    //  For simplicity, we put all logic in the controller here.!!!

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? category,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] int? sellerId,
        [FromQuery] string? search)
    {
        var query = _context.Products.Where(p => p.IsActive);

        if (!string.IsNullOrEmpty(category))
            query = query.Where(p => p.Category == category);

        if (minPrice.HasValue)
            query = query.Where(p => p.Price >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(p => p.Price <= maxPrice.Value);

        if (sellerId.HasValue)
            query = query.Where(p => p.SellerId == sellerId.Value);

        if (!string.IsNullOrEmpty(search))
            query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search));

        var products = await query.ToListAsync();

        var result = products.Select(p => new
        {
            p.Id,
            p.SellerId,
            p.Name,
            p.Description,
            p.Category,
            p.Price,
            p.Stock,
            p.DiscountPercent,
            p.FinalPrice,
            p.ImageUrl,
            p.CreatedAt,
            p.UpdatedAt
        });

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null || !product.IsActive)
            return NotFound();

        return Ok(new
        {
            product.Id,
            product.SellerId,
            product.Name,
            product.Description,
            product.Category,
            product.Price,
            product.Stock,
            product.DiscountPercent,
            product.FinalPrice,
            product.ImageUrl,
            product.IsActive,
            product.CreatedAt,
            product.UpdatedAt
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(Product product)
    {
        // Validate seller exists in CustomerService
        var sellerExists = await _customerClient.SellerExistsAsync(product.SellerId);
        if (!sellerExists)
            return BadRequest("Seller does not exist or is not a Seller role.");

        product.CreatedAt = DateTime.UtcNow;
        product.IsActive = true;

        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, new
        {
            product.Id,
            product.SellerId,
            product.Name,
            product.Description,
            product.Category,
            product.Price,
            product.Stock,
            product.DiscountPercent,
            product.FinalPrice,
            product.ImageUrl,
            product.CreatedAt
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Product updated)
    {
        var callerIdStr = Request.Headers["X-User-Id"].FirstOrDefault();
        var product = await _context.Products.FindAsync(id);
        if (product == null || !product.IsActive)
            return NotFound();

        // Only the seller owner can update
        if (int.TryParse(callerIdStr, out var callerId) && callerId != product.SellerId)
            return Forbid();

        product.Name = updated.Name;
        product.Description = updated.Description;
        product.Category = updated.Category;
        product.Price = updated.Price;
        product.ImageUrl = updated.ImageUrl;
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(product);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var callerIdStr = Request.Headers["X-User-Id"].FirstOrDefault();
        var product = await _context.Products.FindAsync(id);
        if (product == null || !product.IsActive)
            return NotFound();

        if (int.TryParse(callerIdStr, out var callerId) && callerId != product.SellerId)
            return Forbid();

        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Set absolute stock quantity
    /// </summary>
    [HttpPatch("{id}/stock")]
    public async Task<IActionResult> UpdateStock(int id, [FromBody] StockUpdateRequest request)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null || !product.IsActive)
            return NotFound();

        product.Stock = request.Quantity;
        product.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { product.Id, product.Stock });
    }

    /// <summary>
    /// Update discount percentage
    /// </summary>
    [HttpPatch("{id}/discount")]
    public async Task<IActionResult> UpdateDiscount(int id, [FromBody] DiscountUpdateRequest request)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null || !product.IsActive)
            return NotFound();

        if (request.DiscountPercent < 0 || request.DiscountPercent > 100)
            return BadRequest("Discount must be between 0 and 100.");

        product.DiscountPercent = request.DiscountPercent;
        product.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { product.Id, product.DiscountPercent, product.FinalPrice });
    }

    /// <summary>
    /// Decrement stock (called by OrderService during checkout)
    /// </summary>
    [HttpPatch("{id}/stock/decrement")]
    public async Task<IActionResult> DecrementStock(int id, [FromBody] StockDecrementRequest request)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null || !product.IsActive)
            return NotFound();

        if (product.Stock < request.Quantity)
            return BadRequest($"Insufficient stock. Available: {product.Stock}");

        product.Stock -= request.Quantity;
        product.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { product.Id, product.Stock });
    }

    /// <summary>
    /// Increment stock (called by OrderService on cancel/return)
    /// </summary>
    [HttpPatch("{id}/stock/increment")]
    public async Task<IActionResult> IncrementStock(int id, [FromBody] StockDecrementRequest request)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null || !product.IsActive)
            return NotFound();

        product.Stock += request.Quantity;
        product.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { product.Id, product.Stock });
    }

    [HttpGet("seller/{sellerId}")]
    public async Task<IActionResult> GetBySeller(int sellerId)
    {
        var products = await _context.Products
            .Where(p => p.SellerId == sellerId && p.IsActive)
            .ToListAsync();

        return Ok(products.Select(p => new
        {
            p.Id,
            p.Name,
            p.Category,
            p.Price,
            p.Stock,
            p.DiscountPercent,
            p.FinalPrice,
            p.CreatedAt
        }));
    }

    /// <summary>
    /// General product dashboard
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var products = await _context.Products.Where(p => p.IsActive).ToListAsync();

        var totalProducts = products.Count;
        var averagePrice = totalProducts > 0 ? Math.Round(products.Average(p => p.Price), 2) : 0;
        var averageStock = totalProducts > 0 ? Math.Round(products.Average(p => (decimal)p.Stock), 2) : 0;
        var outOfStock = products.Count(p => p.Stock == 0);
        var withDiscount = products.Count(p => p.DiscountPercent > 0);

        var byCategory = products
            .GroupBy(p => string.IsNullOrEmpty(p.Category) ? "Uncategorized" : p.Category)
            .Select(g => new { Category = g.Key, Count = g.Count(), AvgPrice = Math.Round(g.Average(p => p.Price), 2) })
            .OrderByDescending(x => x.Count)
            .ToList();

        return Ok(new
        {
            TotalProducts = totalProducts,
            AveragePrice = averagePrice,
            AverageStock = averageStock,
            OutOfStock = outOfStock,
            WithDiscount = withDiscount,
            ByCategory = byCategory
        });
    }

    /// <summary>
    /// Seller-specific product dashboard
    /// </summary>
    [HttpGet("seller/{sellerId}/dashboard")]
    public async Task<IActionResult> SellerDashboard(int sellerId)
    {
        var products = await _context.Products
            .Where(p => p.SellerId == sellerId && p.IsActive)
            .ToListAsync();

        return Ok(new
        {
            TotalProducts = products.Count,
            AveragePrice = products.Count > 0 ? Math.Round(products.Average(p => p.Price), 2) : 0,
            TotalStock = products.Sum(p => p.Stock),
            OutOfStock = products.Count(p => p.Stock == 0),
            WithDiscount = products.Count(p => p.DiscountPercent > 0),
            TotalValueInStock = Math.Round(products.Sum(p => p.FinalPrice * p.Stock), 2)
        });
    }
}

public record StockUpdateRequest(int Quantity);
public record StockDecrementRequest(int Quantity);
public record DiscountUpdateRequest(decimal DiscountPercent);
