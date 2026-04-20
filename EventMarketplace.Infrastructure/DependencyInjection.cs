using EventMarketplace.Application.Repositories;
using EventMarketplace.Application.Services;
using EventMarketplace.Infrastructure.Identity;
using EventMarketplace.Infrastructure.Persistence;
using EventMarketplace.Infrastructure.Repositories;
using EventMarketplace.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventMarketplace.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");

        var mysqlVersion = new MySqlServerVersion(new Version(8, 0, 0));

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMySql(connectionString, mysqlVersion));

        services
            .AddIdentityCore<UserApp>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ISmsService, SmsService>();

        return services;
    }
}