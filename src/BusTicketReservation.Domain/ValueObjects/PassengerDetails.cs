using BusTicketReservation.Domain.Common;

namespace BusTicketReservation.Domain.ValueObjects;

public class PassengerDetails : ValueObject
{
    public string Name { get; private set; }
    public string MobileNumber { get; private set; }
    public string? Email { get; private set; }
    
    private PassengerDetails() { } // For EF Core
    
    public PassengerDetails(string name, string mobileNumber, string? email = null)
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
        // Simple validation - can be enhanced based on requirements
        return mobileNumber.Length >= 10 && mobileNumber.All(char.IsDigit);
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name.ToLowerInvariant();
        yield return MobileNumber;
        yield return Email?.ToLowerInvariant() ?? string.Empty;
    }
    
    public override string ToString() => $"{Name} ({MobileNumber})";
}