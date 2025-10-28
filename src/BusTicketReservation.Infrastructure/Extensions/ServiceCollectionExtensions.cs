using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BusTicketReservation.Application.Interfaces;
using BusTicketReservation.Infrastructure.Data;
using BusTicketReservation.Infrastructure.Repositories;
using BusTicketReservation.Infrastructure.UnitOfWork;

namespace BusTicketReservation.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Add DbContext with IPv4-only configuration
        services.AddDbContext<BusTicketDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            
            // Configure Npgsql to prefer IPv4 and disable pooling for better error visibility
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(BusTicketDbContext).Assembly.FullName);
                npgsqlOptions.CommandTimeout(30); // 30 seconds timeout
            });
            
            // Enable detailed error logging
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        });

        // Add repositories
        services.AddScoped<IBusRepository, BusRepository>();
        services.AddScoped<IRouteRepository, RouteRepository>();
        services.AddScoped<IBusScheduleRepository, BusScheduleRepository>();
        services.AddScoped<ISeatRepository, SeatRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<IPassengerRepository, PassengerRepository>();
        
        // NOTE: IUserRepository and IOtpRepository are registered in Program.cs
        // using Supabase-based implementations for authentication

        // Add Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();

        return services;
    }

    public static IServiceCollection AddInfrastructureForTesting(
        this IServiceCollection services,
        string connectionString)
    {
        // Add DbContext for testing (in-memory or test database)
        services.AddDbContext<BusTicketDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Add repositories
        services.AddScoped<IBusRepository, BusRepository>();
        services.AddScoped<IRouteRepository, RouteRepository>();
        services.AddScoped<IBusScheduleRepository, BusScheduleRepository>();
        services.AddScoped<ISeatRepository, SeatRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<IPassengerRepository, PassengerRepository>();

        // Add Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();

        return services;
    }
}