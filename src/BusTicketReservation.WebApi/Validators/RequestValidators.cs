using FluentValidation;
using BusTicketReservation.WebApi.DTOs.Requests;

namespace BusTicketReservation.WebApi.Validators;

public class SearchBusesRequestValidator : AbstractValidator<SearchBusesRequest>
{
    public SearchBusesRequestValidator()
    {
        RuleFor(x => x.FromCity)
            .NotEmpty()
            .WithMessage("From city is required")
            .MinimumLength(2)
            .WithMessage("From city must be at least 2 characters")
            .MaximumLength(50)
            .WithMessage("From city must not exceed 50 characters");

        RuleFor(x => x.ToCity)
            .NotEmpty()
            .WithMessage("To city is required")
            .MinimumLength(2)
            .WithMessage("To city must be at least 2 characters")
            .MaximumLength(50)
            .WithMessage("To city must not exceed 50 characters");

        RuleFor(x => x.JourneyDate)
            .NotEmpty()
            .WithMessage("Journey date is required")
            .Must(BeValidJourneyDate)
            .WithMessage("Journey date must be today or in the future");

        RuleFor(x => x.PassengerCount)
            .GreaterThan(0)
            .WithMessage("Passenger count must be greater than 0")
            .LessThanOrEqualTo(10)
            .WithMessage("Passenger count must not exceed 10");

        RuleFor(x => x)
            .Must(x => !x.FromCity.Equals(x.ToCity, StringComparison.OrdinalIgnoreCase))
            .WithMessage("From city and To city cannot be the same")
            .When(x => !string.IsNullOrEmpty(x.FromCity) && !string.IsNullOrEmpty(x.ToCity));
    }

    private bool BeValidJourneyDate(DateTime journeyDate)
    {
        return journeyDate.Date >= DateTime.Today;
    }
}

public class BookTicketRequestValidator : AbstractValidator<BookTicketRequest>
{
    public BookTicketRequestValidator()
    {
        RuleFor(x => x.ScheduleId)
            .NotEmpty()
            .WithMessage("Schedule ID is required");

        RuleFor(x => x.SeatNumbers)
            .NotEmpty()
            .WithMessage("At least one seat must be selected")
            .Must(x => x.Count <= 10)
            .WithMessage("Cannot book more than 10 seats at once");

        RuleForEach(x => x.SeatNumbers)
            .NotEmpty()
            .WithMessage("Seat number cannot be empty")
            .Matches(@"^[A-Z]\d+$")
            .WithMessage("Seat number must be in format like 'A1', 'B2', etc.");

        RuleFor(x => x.PassengerInfo)
            .NotNull()
            .WithMessage("Passenger information is required")
            .SetValidator(new PassengerInfoRequestValidator());
    }
}

public class PassengerInfoRequestValidator : AbstractValidator<PassengerInfoRequest>
{
    public PassengerInfoRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MinimumLength(2)
            .WithMessage("Name must be at least 2 characters")
            .MaximumLength(100)
            .WithMessage("Name must not exceed 100 characters")
            .Matches(@"^[a-zA-Z\s]+$")
            .WithMessage("Name can only contain letters and spaces");

        RuleFor(x => x.MobileNumber)
            .NotEmpty()
            .WithMessage("Mobile number is required")
            .Matches(@"^\d{10,15}$")
            .WithMessage("Mobile number must be 10-15 digits");

        RuleFor(x => x.Email)
            .EmailAddress()
            .WithMessage("Invalid email format")
            .When(x => !string.IsNullOrEmpty(x.Email));
    }
}

public class CancelTicketRequestValidator : AbstractValidator<CancelTicketRequest>
{
    public CancelTicketRequestValidator()
    {
        RuleFor(x => x.TicketId)
            .NotEmpty()
            .WithMessage("Ticket ID is required");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Cancellation reason is required")
            .MinimumLength(5)
            .WithMessage("Reason must be at least 5 characters")
            .MaximumLength(500)
            .WithMessage("Reason must not exceed 500 characters");
    }
}