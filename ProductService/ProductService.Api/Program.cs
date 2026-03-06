using ProductService.Api.Data;
using ProductService.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseSqlite("Data Source=products.db"));

// HTTP client to validate seller in CustomerService
builder.Services.AddHttpClient<ICustomerClient, CustomerClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:CustomerService"] ?? "http://customerservice:8080/");
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    db.Database.Migrate();

    if (!db.Products.Any())
    {
        db.Products.AddRange(
            // Undergraduate Programs
            new ProductService.Api.Models.Product
            {
                SellerId = 1, Name = "Bachelor of Science in Business Administration (BBA)",
                Description = "Develop core business competencies in finance, marketing, management and international business at FDU Vancouver.",
                Category = "Undergraduate", Price = 20000m, Stock = 30, DiscountPercent = 0,
                ImageUrl = "https://images.unsplash.com/photo-1454165804606-c3d57bc86b40?w=400", IsActive = true
            },
            new ProductService.Api.Models.Product
            {
                SellerId = 1, Name = "Bachelor of Science in Information Technology (BSIT)",
                Description = "Hands-on IT program covering software development, networking, cybersecurity and cloud computing.",
                Category = "Undergraduate", Price = 21000m, Stock = 25, DiscountPercent = 0,
                ImageUrl = "https://images.unsplash.com/photo-1517694712202-14dd9538aa97?w=400", IsActive = true
            },
            new ProductService.Api.Models.Product
            {
                SellerId = 1, Name = "Bachelor of Arts in Individualized Studies (BAIS)",
                Description = "Customize your degree with interdisciplinary studies tailored to your academic and career goals.",
                Category = "Undergraduate", Price = 18000m, Stock = 20, DiscountPercent = 0,
                ImageUrl = "https://images.unsplash.com/photo-1523240795612-9a054b0db644?w=400", IsActive = true
            },
            new ProductService.Api.Models.Product
            {
                SellerId = 1, Name = "BAIS — Hospitality & Tourism Management",
                Description = "Specialize in hospitality and tourism within the Individualized Studies framework at FDU Vancouver.",
                Category = "Undergraduate", Price = 19000m, Stock = 20, DiscountPercent = 0,
                ImageUrl = "https://images.unsplash.com/photo-1566073771259-6a8506099945?w=400", IsActive = true
            },
            // Graduate Programs
            new ProductService.Api.Models.Product
            {
                SellerId = 1, Name = "Master of Administrative Science (MAS)",
                Description = "Graduate program focused on organizational leadership, strategic management and administrative excellence.",
                Category = "Graduate", Price = 15000m, Stock = 15, DiscountPercent = 5,
                ImageUrl = "https://images.unsplash.com/photo-1507679799987-c73779587ccf?w=400", IsActive = true
            },
            new ProductService.Api.Models.Product
            {
                SellerId = 1, Name = "Master of Health Administration (MHA)",
                Description = "Prepare for leadership roles in healthcare organizations with this CAHME-aligned graduate program.",
                Category = "Graduate", Price = 18000m, Stock = 15, DiscountPercent = 0,
                ImageUrl = "https://images.unsplash.com/photo-1519494026892-80bbd2d6fd0d?w=400", IsActive = true
            },
            new ProductService.Api.Models.Product
            {
                SellerId = 1, Name = "Master of Hospitality Management Studies (MHMS)",
                Description = "Advanced hospitality management program for professionals seeking leadership in the global hospitality industry.",
                Category = "Graduate", Price = 14000m, Stock = 15, DiscountPercent = 10,
                ImageUrl = "https://images.unsplash.com/photo-1551882547-ff40c63fe5fa?w=400", IsActive = true
            },
            new ProductService.Api.Models.Product
            {
                SellerId = 1, Name = "Master of Science in Applied Computer Science (MSACS)",
                Description = "Advanced computing program covering AI, data science, software engineering and applied research methods.",
                Category = "Graduate", Price = 16000m, Stock = 20, DiscountPercent = 0,
                ImageUrl = "https://images.unsplash.com/photo-1555949963-aa79dcee981c?w=400", IsActive = true
            }
        );
        db.SaveChanges();
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
