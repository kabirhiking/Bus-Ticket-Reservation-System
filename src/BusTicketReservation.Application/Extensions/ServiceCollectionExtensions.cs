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
        
        // Add domain services
        services.AddScoped<SeatBookingDomainService>();
        
        return services;
    }
}