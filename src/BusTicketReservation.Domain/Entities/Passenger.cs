using BusTicketReservation.Domain.Common;
using BusTicketReservation.Domain.ValueObjects;

namespace BusTicketReservation.Domain.Entities;

public class Passenger : BaseEntity
{
    public string Name { get; private set; }
    public string MobileNumber { get; private set; }
    public string? Email { get; private set; }
    
    // Navigation Properties
    private readonly List<Ticket> _tickets = new();
    public IReadOnlyList<Ticket> Tickets => _tickets.AsReadOnly();
    
    private Passenger() { } // For EF Core
    
    public Passenger(string name, string mobileNumber, string? email = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));
            
        if (string.IsNullOrWhiteSpace(mobileNumber))
            throw new ArgumentNullException(nameof(mobileNumber));
            
        if (!IsValidMobileNumber(mobileNumber))
            throw new ArgumentException("Invalid mobile number format", nameof(mobileNumber));
            
        Name = name.Trim();
        MobileNumber = mobileNumber.Trim();
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
    }
    
    private static bool IsValidMobileNumber(string mobileNumber)
    {
        // Simple validation - adjust based on requirements
        var digits = mobileNumber.Where(char.IsDigit).Count();
        return digits >= 10 && digits <= 15;
    }
    
    public PassengerDetails GetDetails() => new PassengerDetails(Name, MobileNumber, Email);
    
    public void UpdateContactInfo(string? email)
    {
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
        MarkAsUpdated();
    }
    
    public void AddTicket(Ticket ticket)
    {
        if (ticket.PassengerId != Id)
            throw new ArgumentException("Ticket passenger ID doesn't match this passenger");
            
        _tickets.Add(ticket);
        MarkAsUpdated();
    }
    
    public override string ToString() => $"{Name} ({MobileNumber})";
}