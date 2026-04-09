using WebFrontend.Components;
using WebFrontend.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// All traffic goes through the API Gateway
var gatewayUrl = builder.Configuration["Services:ApiGateway"] ?? "http://localhost:5000";

builder.Services.AddHttpClient<CustomerService>(c => c.BaseAddress = new Uri(gatewayUrl));
builder.Services.AddHttpClient<ProductService>(c => c.BaseAddress = new Uri(gatewayUrl));
builder.Services.AddHttpClient<OrderService>(c => c.BaseAddress = new Uri(gatewayUrl));
builder.Services.AddHttpClient<CartService>(c => c.BaseAddress = new Uri(gatewayUrl));
builder.Services.AddHttpClient<PaymentService>(c => c.BaseAddress = new Uri(gatewayUrl));
builder.Services.AddHttpClient<NotificationService>(c => c.BaseAddress = new Uri(gatewayUrl));

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
