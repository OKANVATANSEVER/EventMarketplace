using EventMarketplace.Application;
using EventMarketplace.Infrastructure;
using EventMarketplace.Infrastructure.Identity;
using EventMarketplace.Infrastructure.Persistence;
using EventMarketplace.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Blazored.LocalStorage;

var builder = WebApplication.CreateBuilder(args);

// Infrastructure & Application
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

// ASP.NET Identity Cookie Auth
builder.Services
    .AddDefaultIdentity<UserApp>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.AccessDeniedPath = "/access-denied";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
});

// Blazor Server
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

// App services
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AdminStatsService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

// Identity razor pages (for cookie auth endpoints)
app.MapRazorPages();

app.MapRazorComponents<EventMarketplace.Web.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();
