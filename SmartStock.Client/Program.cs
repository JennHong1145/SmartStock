using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SmartStock.Client;
using SmartStock.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Typed client: the base address is the API, not the client's own address
builder.Services.AddHttpClient<ProductApiClient>(client =>
	client.BaseAddress = new Uri("https://localhost:7067/"));

builder.Services.AddHttpClient<OrderApiClient>(client =>
	client.BaseAddress = new Uri("https://localhost:7067/"));

await builder.Build().RunAsync();