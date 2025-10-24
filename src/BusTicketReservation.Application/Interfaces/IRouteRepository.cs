using BusTicketReservation.Domain.Entities;

namespace BusTicketReservation.Application.Interfaces;

public interface IRouteRepository : IEntityRepository<Route>
{
    Task<Route?> GetRouteByFromAndToAsync(string from, string to);
    Task<IEnumerable<Route>> GetRoutesByFromCityAsync(string fromCity);
    Task<IEnumerable<Route>> GetRoutesToCityAsync(string toCity);
}