using ChatAI_Assistant.Client;
using ChatAI_Assistant.Client.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;


var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddRadzenComponents();
builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<TooltipService>();
builder.Services.AddScoped<ContextMenuService>();
builder.Services.AddScoped<IChatClientService, ChatClientService>();
builder.Services.AddScoped<IStateService, StateService>();

// Configure HttpClient for API calls
builder.Services.AddHttpClient<IChatClientService, ChatClientService>(client =>
{
    client.BaseAddress = new Uri($"{builder.Configuration["ServerUrl"]}");
    client.DefaultRequestHeaders.Add("User-Agent", "ChatAI-Assistant-Client/1.0");
});

await builder.Build().RunAsync();
