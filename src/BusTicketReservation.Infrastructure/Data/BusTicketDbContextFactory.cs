using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace BusTicketReservation.Infrastructure.Data;

public class BusTicketDbContextFactory : IDesignTimeDbContextFactory<BusTicketDbContext>
{
    public BusTicketDbContext CreateDbContext(string[] args)
    {
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../BusTicketReservation.WebApi"))
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        // Get connection string
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // Create DbContext options
        var optionsBuilder = new DbContextOptionsBuilder<BusTicketDbContext>();
        optionsBuilder.UseNpgsql(connectionString, 
            npgsqlOptions => npgsqlOptions.MigrationsAssembly("BusTicketReservation.Infrastructure"));

        return new BusTicketDbContext(optionsBuilder.Options);
    }
}
