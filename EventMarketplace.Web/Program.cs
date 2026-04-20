using Blazored.LocalStorage;
using EventMarketplace.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();

// JWT-based auth state (scoped per Blazor Server circuit)
builder.Services.AddScoped<TokenStore>();
builder.Services.AddScoped<JwtAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(
    sp => sp.GetRequiredService<JwtAuthStateProvider>());

// API HTTP client
builder.Services.AddHttpClient<ApiClient>((sp, http) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    http.BaseAddress = new Uri(config["ApiSettings:BaseUrl"] ?? "http://localhost:5105/");
});
builder.Services.AddScoped<ApiClient>();

// Persistence for JWT token across page reloads
builder.Services.AddBlazoredLocalStorage();

// Admin dashboard service (thin wrapper over ApiClient)
builder.Services.AddScoped<AdminStatsService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<EventMarketplace.Web.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();

