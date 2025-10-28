using BusTicketReservation.Application.DTOs;

namespace BusTicketReservation.Application.Services;

public interface IBoardingPointService
{
    List<BoardingPointDto> GetBoardingPoints(string city);
    List<DroppingPointDto> GetDroppingPoints(string city);
}

public class BoardingPointService : IBoardingPointService
{
    private static readonly Dictionary<string, List<BoardingPointDto>> BoardingPoints = new()
    {
        ["Dhaka"] = new List<BoardingPointDto>
        {
            new() { Id = Guid.NewGuid(), Name = "Sayedabad Bus Terminal", Address = "Sayedabad, Dhaka", Time = "07:30 AM" },
            new() { Id = Guid.NewGuid(), Name = "Gabtoli Bus Terminal", Address = "Gabtoli, Dhaka", Time = "08:00 AM" },
            new() { Id = Guid.NewGuid(), Name = "Mohakhali Bus Terminal", Address = "Mohakhali, Dhaka", Time = "08:15 AM" },
            new() { Id = Guid.NewGuid(), Name = "Kalyanpur Bus Stand", Address = "Kalyanpur, Dhaka", Time = "08:30 AM" },
            new() { Id = Guid.NewGuid(), Name = "Abdullahpur Bus Stop", Address = "Abdullahpur, Dhaka", Time = "08:45 AM" }
        },
        ["Rajshahi"] = new List<BoardingPointDto>
        {
            new() { Id = Guid.NewGuid(), Name = "Rajshahi Bus Terminal", Address = "New Market, Rajshahi", Time = "07:45 AM" },
            new() { Id = Guid.NewGuid(), Name = "C&B More Point", Address = "C&B More, Rajshahi", Time = "08:00 AM" },
            new() { Id = Guid.NewGuid(), Name = "Shaheb Bazar", Address = "Shaheb Bazar, Rajshahi", Time = "08:15 AM" },
            new() { Id = Guid.NewGuid(), Name = "Railway Station", Address = "Railway Station, Rajshahi", Time = "08:30 AM" }
        },
        ["Chittagong"] = new List<BoardingPointDto>
        {
            new() { Id = Guid.NewGuid(), Name = "Oxygen Bus Terminal", Address = "Oxygen, Chittagong", Time = "08:00 AM" },
            new() { Id = Guid.NewGuid(), Name = "Bahaddarhat Bus Stand", Address = "Bahaddarhat, Chittagong", Time = "08:15 AM" },
            new() { Id = Guid.NewGuid(), Name = "New Market", Address = "New Market, Chittagong", Time = "08:30 AM" },
            new() { Id = Guid.NewGuid(), Name = "Wasa Circle", Address = "Wasa Circle, Chittagong", Time = "08:45 AM" }
        },
        ["Sylhet"] = new List<BoardingPointDto>
        {
            new() { Id = Guid.NewGuid(), Name = "Kadamtoli Bus Terminal", Address = "Kadamtoli, Sylhet", Time = "07:30 AM" },
            new() { Id = Guid.NewGuid(), Name = "Bandarbazar Point", Address = "Bandarbazar, Sylhet", Time = "07:45 AM" },
            new() { Id = Guid.NewGuid(), Name = "Zindabazar", Address = "Zindabazar, Sylhet", Time = "08:00 AM" },
            new() { Id = Guid.NewGuid(), Name = "Amberkhana", Address = "Amberkhana, Sylhet", Time = "08:15 AM" }
        }
    };

    private static readonly Dictionary<string, List<DroppingPointDto>> DroppingPoints = new()
    {
        ["Dhaka"] = new List<DroppingPointDto>
        {
            new() { Id = Guid.NewGuid(), Name = "Sayedabad Bus Terminal", Address = "Sayedabad, Dhaka", Time = "01:30 PM" },
            new() { Id = Guid.NewGuid(), Name = "Gabtoli Bus Terminal", Address = "Gabtoli, Dhaka", Time = "01:45 PM" },
            new() { Id = Guid.NewGuid(), Name = "Mohakhali Bus Terminal", Address = "Mohakhali, Dhaka", Time = "02:00 PM" },
            new() { Id = Guid.NewGuid(), Name = "Kalyanpur Bus Stand", Address = "Kalyanpur, Dhaka", Time = "02:15 PM" }
        },
        ["Rajshahi"] = new List<DroppingPointDto>
        {
            new() { Id = Guid.NewGuid(), Name = "Rajshahi Bus Terminal", Address = "New Market, Rajshahi", Time = "01:30 PM" },
            new() { Id = Guid.NewGuid(), Name = "C&B More Point", Address = "C&B More, Rajshahi", Time = "01:45 PM" },
            new() { Id = Guid.NewGuid(), Name = "Shaheb Bazar", Address = "Shaheb Bazar, Rajshahi", Time = "02:00 PM" },
            new() { Id = Guid.NewGuid(), Name = "Railway Station", Address = "Railway Station, Rajshahi", Time = "02:15 PM" }
        },
        ["Chittagong"] = new List<DroppingPointDto>
        {
            new() { Id = Guid.NewGuid(), Name = "Oxygen Bus Terminal", Address = "Oxygen, Chittagong", Time = "03:00 PM" },
            new() { Id = Guid.NewGuid(), Name = "Bahaddarhat Bus Stand", Address = "Bahaddarhat, Chittagong", Time = "03:15 PM" },
            new() { Id = Guid.NewGuid(), Name = "New Market", Address = "New Market, Chittagong", Time = "03:30 PM" },
            new() { Id = Guid.NewGuid(), Name = "Wasa Circle", Address = "Wasa Circle, Chittagong", Time = "03:45 PM" }
        },
        ["Sylhet"] = new List<DroppingPointDto>
        {
            new() { Id = Guid.NewGuid(), Name = "Kadamtoli Bus Terminal", Address = "Kadamtoli, Sylhet", Time = "12:30 PM" },
            new() { Id = Guid.NewGuid(), Name = "Bandarbazar Point", Address = "Bandarbazar, Sylhet", Time = "12:45 PM" },
            new() { Id = Guid.NewGuid(), Name = "Zindabazar", Address = "Zindabazar, Sylhet", Time = "01:00 PM" },
            new() { Id = Guid.NewGuid(), Name = "Amberkhana", Address = "Amberkhana, Sylhet", Time = "01:15 PM" }
        }
    };

    public List<BoardingPointDto> GetBoardingPoints(string city)
    {
        return BoardingPoints.TryGetValue(city, out var points) ? points : new List<BoardingPointDto>();
    }

    public List<DroppingPointDto> GetDroppingPoints(string city)
    {
        return DroppingPoints.TryGetValue(city, out var points) ? points : new List<DroppingPointDto>();
    }
}