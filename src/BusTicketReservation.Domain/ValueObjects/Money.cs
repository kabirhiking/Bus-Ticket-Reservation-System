using BusTicketReservation.Domain.Common;

namespace BusTicketReservation.Domain.ValueObjects;

public class Money : ValueObject
{
    public decimal Amount { get; private set; }
    public string Currency { get; private set; }
    
    private Money() { } // For EF Core
    
    public Money(decimal amount, string currency = "USD")
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));
            
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentNullException(nameof(currency));
            
        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
    
    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot add money with different currencies");
            
        return new Money(Amount + other.Amount, Currency);
    }
    
    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot subtract money with different currencies");
            
        return new Money(Amount - other.Amount, Currency);
    }
    
    public static Money operator +(Money left, Money right) => left.Add(right);
    public static Money operator -(Money left, Money right) => left.Subtract(right);
    
    public override string ToString() => $"{Amount} {Currency}";
}