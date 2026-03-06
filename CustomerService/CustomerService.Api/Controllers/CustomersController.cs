using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CustomerService.Api.Data;
using CustomerService.Api.Models;
using CustomerService.Api.Services;

namespace CustomerService.Api.Controllers;

[ApiController]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    private readonly CustomerDbContext _context;
    private readonly IOrderStatsClient _orderStatsClient;
    private readonly IProductStatsClient _productStatsClient;

    public CustomersController(
        CustomerDbContext context,
        IOrderStatsClient orderStatsClient,
        IProductStatsClient productStatsClient)
    {
        _context = context;
        _orderStatsClient = orderStatsClient;
        _productStatsClient = productStatsClient;
    }

    //!!!IMporant: In a bigger or prod enviroment, its normal to have the business logic in a separate service layer, and the controller just calls that service.
    //  For simplicity, we put all logic in the controller here.!!!

   
    /// Buyer sees all sellers (public info). Seller does NOT see other sellers.    
    /// getting params from querystrings, and also from headers to identify the caller role and id (in a real app we would use authentication and claims for this)
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? role,
        [FromQuery] string? search)
    {
        //having those only to simulate profiles on the frontend, in a real app we would use authentication and claims to identify the caller and their role
        var callerRole = Request.Headers["X-User-Role"].FirstOrDefault() ?? "Buyer";
        var callerIdStr = Request.Headers["X-User-Id"].FirstOrDefault();

        //already using Ef, so Using Linq + this lambda expression to run all queries, using Sqlite for perfomance and simplicity.
        var query = _context.Customers.Where(c => c.IsActive);

        if (!string.IsNullOrEmpty(role))
            query = query.Where(c => c.Role == role);

        if (!string.IsNullOrEmpty(search))
            query = query.Where(c => c.Name.Contains(search) || c.Email.Contains(search));

        // Seller cannot see other sellers
        if (callerRole == "Seller")
        {
            if (int.TryParse(callerIdStr, out var callerId))
            {
                query = query.Where(c => c.Role == "Buyer" || c.Id == callerId);
            }
            else
            {
                query = query.Where(c => c.Role == "Buyer");
            }
        }

        var customers = await query
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.Email,
                c.Phone,
                c.Role,
                c.Address,
                c.CreatedAt
            })
            .ToListAsync();

        return Ok(customers);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var callerRole = Request.Headers["X-User-Role"].FirstOrDefault() ?? "Buyer";
        var callerIdStr = Request.Headers["X-User-Id"].FirstOrDefault();

        var customer = await _context.Customers.FindAsync(id);
        if (customer == null || !customer.IsActive)
            return NotFound();

        // Seller can only see their own profile (not other sellers)
        if (callerRole == "Seller" && customer.Role == "Seller")
        {
            if (int.TryParse(callerIdStr, out var callerId) && callerId != id)
                return StatusCode(403, "Access denied.");
        }

        return Ok(customer);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Customer customer)
    {
        if (customer.Role != "Buyer" && customer.Role != "Seller")
            return BadRequest("Role must be 'Buyer' or 'Seller'.");

        var emailExists = await _context.Customers.AnyAsync(c => c.Email == customer.Email && c.IsActive);
        if (emailExists)
            return BadRequest("Email already in use.");

            //could have other validations here (like document uniqueness, phone format, etc)

        customer.CreatedAt = DateTime.UtcNow;
        customer.IsActive = true;

        await _context.Customers.AddAsync(customer); //adding to the dbcontext, but not yet saving to the database
        await _context.SaveChangesAsync(); //saving

//using nameof to make sure the route name is correct and will be refactored if we change the method name. Also returning the created customer in the response body.
        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Customer updated)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null || !customer.IsActive)
            return NotFound();

        customer.Name = updated.Name;
        customer.Email = updated.Email;
        customer.Phone = updated.Phone;
        customer.Document = updated.Document;
        customer.Address = updated.Address;

        await _context.SaveChangesAsync();
        return Ok(customer);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null || !customer.IsActive)
            return NotFound();

        customer.IsActive = false;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// General dashboard: total buyers, total sellers, new last 30 days
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var now = DateTime.UtcNow;
        var thirtyDaysAgo = now.AddDays(-30);

        var totalBuyers = await _context.Customers.CountAsync(c => c.IsActive && c.Role == "Buyer");
        var totalSellers = await _context.Customers.CountAsync(c => c.IsActive && c.Role == "Seller");
        var newBuyersLast30Days = await _context.Customers.CountAsync(c => c.IsActive && c.Role == "Buyer" && c.CreatedAt >= thirtyDaysAgo);
        var newSellersLast30Days = await _context.Customers.CountAsync(c => c.IsActive && c.Role == "Seller" && c.CreatedAt >= thirtyDaysAgo);
        var totalInactive = await _context.Customers.CountAsync(c => !c.IsActive);

        return Ok(new
        {
            TotalBuyers = totalBuyers,
            TotalSellers = totalSellers,
            NewBuyersLast30Days = newBuyersLast30Days,
            NewSellersLast30Days = newSellersLast30Days,
            TotalInactive = totalInactive
        });
    }

    /// <summary>
    /// Personal dashboard: Buyer gets order stats, Seller gets product+sales stats
    /// </summary>
    [HttpGet("{id}/dashboard")]
    public async Task<IActionResult> PersonalDashboard(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null || !customer.IsActive)
            return NotFound();

        if (customer.Role == "Buyer")
        {
            var orderStats = await _orderStatsClient.GetBuyerDashboardAsync(id);
            return Ok(new
            {
                Customer = new { customer.Id, customer.Name, customer.Role },
                OrderStats = orderStats
            });
        }
        else // Seller
        {
            var orderStats = await _orderStatsClient.GetSellerDashboardAsync(id);
            var productStats = await _productStatsClient.GetSellerProductsDashboardAsync(id);
            return Ok(new
            {
                Customer = new { customer.Id, customer.Name, customer.Role },
                OrderStats = orderStats,
                ProductStats = productStats
            });
        }
    }
}
