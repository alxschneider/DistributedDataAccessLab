using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SimpleFrontend;
using SimpleFrontend.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// All API calls go through the API Gateway
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.Configuration["Services:ApiGateway"] ?? "http://localhost:5000")
});

builder.Services.AddScoped<ApiService>();

await builder.Build().RunAsync();
