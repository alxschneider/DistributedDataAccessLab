using WebFrontend.Components;
using WebFrontend.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// API base URLs (configurable via env vars for Docker)
var customerUrl = builder.Configuration["Services:CustomerService"] ?? "http://localhost:5001";
var orderUrl = builder.Configuration["Services:OrderService"] ?? "http://localhost:5002";
var productUrl = builder.Configuration["Services:ProductService"] ?? "http://localhost:5003";
var notificationUrl = builder.Configuration["Services:NotificationService"] ?? "http://localhost:5004";
var cartUrl = builder.Configuration["Services:CartService"] ?? "http://localhost:5005";
var paymentUrl = builder.Configuration["Services:PaymentService"] ?? "http://localhost:5006";

builder.Services.AddHttpClient<CustomerService>(c => c.BaseAddress = new Uri(customerUrl));
builder.Services.AddHttpClient<ProductService>(c => c.BaseAddress = new Uri(productUrl));
builder.Services.AddHttpClient<OrderService>(c => c.BaseAddress = new Uri(orderUrl));
builder.Services.AddHttpClient<CartService>(c => c.BaseAddress = new Uri(cartUrl));
builder.Services.AddHttpClient<PaymentService>(c => c.BaseAddress = new Uri(paymentUrl));
builder.Services.AddHttpClient<NotificationService>(c => c.BaseAddress = new Uri(notificationUrl));

builder.Services.AddScoped<ProfileState>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
