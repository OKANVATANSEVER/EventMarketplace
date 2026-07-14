using EventMarketplace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EventMarketplace.Infrastructure.Persistence;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        const string connectionString =
            "server=192.168.0.18;port=3306;user=root;password=Ok@n1984;database=EventMarketplaceDb;";

        optionsBuilder.UseMySql(
            connectionString,
            new MariaDbServerVersion(new Version(10, 11, 0)));

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
