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
        // Add DbContext
        services.AddDbContext<BusTicketDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(BusTicketDbContext).Assembly.FullName)));

        // Add repositories
        services.AddScoped<IBusRepository, BusRepository>();
        services.AddScoped<IRouteRepository, RouteRepository>();
        services.AddScoped<IBusScheduleRepository, BusScheduleRepository>();
        services.AddScoped<ISeatRepository, SeatRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<IPassengerRepository, PassengerRepository>();
        
        // Add authentication repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOtpRepository, OtpRepository>();

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