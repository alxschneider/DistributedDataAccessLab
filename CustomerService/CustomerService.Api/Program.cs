using CustomerService.Api.Data;
using CustomerService.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CustomerDbContext>(options =>
    options.UseSqlite("Data Source=customers.db"));

// HTTP clients for dashboard aggregation
builder.Services.AddHttpClient<IOrderStatsClient, OrderStatsClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:OrderService"] ?? "http://orderservice:8080/");
});

builder.Services.AddHttpClient<IProductStatsClient, ProductStatsClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:ProductService"] ?? "http://productservice:8080/");
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Seed with Seller and Buyer customers if database is empty
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();
    db.Database.Migrate();

    if (!db.Customers.Any())
    {
        db.Customers.AddRange(
            new CustomerService.Api.Models.Customer
            {
                Name = "FDU Vancouver",
                Email = "vancouver@fdu.edu",
                Phone = "16044139566",
                Document = "BC-PSI-2007",
                Role = "Seller",
                Address = "842 Cambie St, Vancouver, BC V6B 2P6",
                IsActive = true
            },
            new CustomerService.Api.Models.Customer
            {
                Name = "Alexandre Schneider",
                Email = "alx.schneider@hotmail.com",
                Phone = "16045551234",
                Document = "BR-12345678900",
                Role = "Buyer",
                Address = "Vancouver, BC",
                IsActive = true
            }
        );
        db.SaveChanges();
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
