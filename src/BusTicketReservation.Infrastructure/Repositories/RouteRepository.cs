using Microsoft.EntityFrameworkCore;
using BusTicketReservation.Application.Interfaces;
using BusTicketReservation.Domain.Entities;
using BusTicketReservation.Infrastructure.Data;

namespace BusTicketReservation.Infrastructure.Repositories;

public class RouteRepository : EntityRepository<Route>, IRouteRepository
{
    public RouteRepository(BusTicketDbContext context) : base(context)
    {
    }

    public async Task<Route?> GetRouteByFromAndToAsync(string from, string to)
    {
        return await _dbSet
            .FirstOrDefaultAsync(r => r.FromCity.ToLower() == from.ToLower() &&
                                     r.ToCity.ToLower() == to.ToLower());
    }

    public async Task<IEnumerable<Route>> GetRoutesByFromCityAsync(string fromCity)
    {
        return await _dbSet
            .Where(r => r.FromCity.ToLower() == fromCity.ToLower())
            .ToListAsync();
    }

    public async Task<IEnumerable<Route>> GetRoutesToCityAsync(string toCity)
    {
        return await _dbSet
            .Where(r => r.ToCity.ToLower() == toCity.ToLower())
            .ToListAsync();
    }

    public async Task<IEnumerable<Route>> GetByOriginAndDestinationAsync(string origin, string destination)
    {
        return await _dbSet
            .Where(r => r.FromCity.ToLower() == origin.ToLower() &&
                       r.ToCity.ToLower() == destination.ToLower())
            .ToListAsync();
    }

    public async Task<IEnumerable<Route>> GetByOriginAsync(string origin)
    {
        return await _dbSet
            .Where(r => r.FromCity.ToLower() == origin.ToLower())
            .ToListAsync();
    }

    public async Task<IEnumerable<Route>> GetByDestinationAsync(string destination)
    {
        return await _dbSet
            .Where(r => r.ToCity.ToLower() == destination.ToLower())
            .ToListAsync();
    }

    public async Task<IEnumerable<string>> GetAllOriginsAsync()
    {
        return await _dbSet
            .Select(r => r.FromCity)
            .Distinct()
            .OrderBy(o => o)
            .ToListAsync();
    }

    public async Task<IEnumerable<string>> GetAllDestinationsAsync()
    {
        return await _dbSet
            .Select(r => r.ToCity)
            .Distinct()
            .OrderBy(d => d)
            .ToListAsync();
    }

    public async Task<IEnumerable<string>> GetDestinationsByOriginAsync(string origin)
    {
        return await _dbSet
            .Where(r => r.FromCity.ToLower() == origin.ToLower())
            .Select(r => r.ToCity)
            .Distinct()
            .OrderBy(d => d)
            .ToListAsync();
    }

    public async Task<Route?> GetByRouteCodeAsync(string routeCode)
    {
        // Since Route doesn't have RouteCode, we'll create a composite key from FromCity-ToCity
        var parts = routeCode.Split('-');
        if (parts.Length != 2) return null;
        
        return await _dbSet
            .FirstOrDefaultAsync(r => r.FromCity == parts[0] && r.ToCity == parts[1]);
    }
}