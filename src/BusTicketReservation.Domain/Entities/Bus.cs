using BusTicketReservation.Domain.Common;
using BusTicketReservation.Domain.ValueObjects;
using BusTicketReservation.Domain.DomainEvents;

namespace BusTicketReservation.Domain.Entities;

public class Bus : BaseEntity, IAggregateRoot
{
    public string Name { get; private set; }
    public string CompanyName { get; private set; }
    public int TotalSeats { get; private set; }
    public string BusType { get; private set; }
    
    // Navigation Properties
    private readonly List<Seat> _seats = new();
    private readonly List<BusSchedule> _schedules = new();
    
    public IReadOnlyList<Seat> Seats => _seats.AsReadOnly();
    public IReadOnlyList<BusSchedule> Schedules => _schedules.AsReadOnly();
    
    private Bus() { } // For EF Core
    
    public Bus(string name, string companyName, int totalSeats, string busType)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));
            
        if (string.IsNullOrWhiteSpace(companyName))
            throw new ArgumentNullException(nameof(companyName));
            
        if (totalSeats <= 0 || totalSeats > 60)
            throw new ArgumentException("Total seats must be between 1 and 60", nameof(totalSeats));
            
        if (string.IsNullOrWhiteSpace(busType))
            throw new ArgumentNullException(nameof(busType));
            
        Name = name.Trim();
        CompanyName = companyName.Trim();
        TotalSeats = totalSeats;
        BusType = busType.Trim();
        
        GenerateSeats();
    }
    
    private void GenerateSeats()
    {
        for (int i = 1; i <= TotalSeats; i++)
        {
            var row = CalculateRow(i);
            var seatNumber = GenerateSeatNumber(i, row);
            _seats.Add(new Seat(Id, seatNumber, row));
        }
    }
    
    private static string CalculateRow(int seatIndex)
    {
        // Assuming 4 seats per row (2 on each side)
        var rowNumber = (seatIndex - 1) / 4 + 1;
        return $"Row {rowNumber}";
    }
    
    private static string GenerateSeatNumber(int seatIndex, string row)
    {
        var seatInRow = (seatIndex - 1) % 4 + 1;
        var seatLetter = seatInRow switch
        {
            1 => "A",
            2 => "B",
            3 => "C",
            4 => "D",
            _ => "A"
        };
        
        var rowNumber = (seatIndex - 1) / 4 + 1;
        return $"{rowNumber}{seatLetter}";
    }
    
    public int GetAvailableSeatsCount()
    {
        return _seats.Count(s => s.Status == SeatStatus.Available);
    }
    
    public int GetBookedSeatsCount()
    {
        return _seats.Count(s => s.Status == SeatStatus.Booked || s.Status == SeatStatus.Sold);
    }
    
    public Seat? GetSeat(string seatNumber)
    {
        return _seats.FirstOrDefault(s => s.SeatNumber == seatNumber);
    }
    
    public IEnumerable<Seat> GetAvailableSeats()
    {
        return _seats.Where(s => s.Status == SeatStatus.Available);
    }
    
    public void AddSchedule(BusSchedule schedule)
    {
        if (schedule.BusId != Id)
            throw new ArgumentException("Schedule bus ID doesn't match this bus");
            
        _schedules.Add(schedule);
        MarkAsUpdated();
    }
}