using SimpleFrontend.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var gateway = builder.Configuration["Services:ApiGateway"] ?? "http://localhost:5000";

builder.Services.AddHttpClient("Api", c => c.BaseAddress = new Uri(gateway));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
