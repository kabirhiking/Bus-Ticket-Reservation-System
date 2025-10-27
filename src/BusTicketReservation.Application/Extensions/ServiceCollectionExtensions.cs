using Microsoft.Extensions.DependencyInjection;
using BusTicketReservation.Application.Interfaces;
using BusTicketReservation.Application.Services;
using BusTicketReservation.Domain.DomainServices;

namespace BusTicketReservation.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Add application services
        services.AddScoped<ISearchService, SearchService>();
        services.AddScoped<IBookingService, BookingService>();
        
        // Add authentication services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IOtpService, OtpService>();
        
        // Add Supabase service
        services.AddScoped<ISupabaseService, SupabaseService>();
        
        // Add domain services
        services.AddScoped<SeatBookingDomainService>();
        
        return services;
    }
}