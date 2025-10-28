using BusTicketReservation.Domain.Entities;
using BusTicketReservation.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace BusTicketReservation.Infrastructure.Data;

public static class SeedData
{
    public static async Task InitializeAsync(BusTicketDbContext context)
    {
        try
        {
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Check if data already exists
            if (await context.Routes.AnyAsync())
            {
                Console.WriteLine("📊 Seed data already exists");
                return; // Database has been seeded
            }

            Console.WriteLine("🌱 Adding seed data...");

            // Seed Routes
            var routes = new List<Route>
            {
                new Route("Dhaka", "Rajshahi", 256.5m, TimeSpan.FromHours(5.5)),
                new Route("Dhaka", "Chittagong", 264.0m, TimeSpan.FromHours(6)),
                new Route("Dhaka", "Sylhet", 245.0m, TimeSpan.FromHours(5))
            };

            await context.Routes.AddRangeAsync(routes);
            await context.SaveChangesAsync();

            // Seed Buses
            var buses = new List<Bus>
            {
                new Bus("Green Line Express", "Green Line Paribahan", 40, "AC Seater"),
                new Bus("Shyamoli Luxury", "Shyamoli Paribahan", 32, "AC Sleeper"),
                new Bus("Ena Transport", "Ena Paribahan", 45, "Non-AC Seater"),
                new Bus("Hanif Enterprise", "Hanif Paribahan", 40, "AC Seater")
            };

            await context.Buses.AddRangeAsync(buses);
            await context.SaveChangesAsync();

            // Get saved routes and buses with their IDs
            var savedRoutes = await context.Routes.ToListAsync();
            var savedBuses = await context.Buses.ToListAsync();

            var dhakaRajshahiRoute = savedRoutes.First(r => r.FromCity == "Dhaka" && r.ToCity == "Rajshahi");
            var dhakaChittagongRoute = savedRoutes.First(r => r.FromCity == "Dhaka" && r.ToCity == "Chittagong");
            var dhakaSylhetRoute = savedRoutes.First(r => r.FromCity == "Dhaka" && r.ToCity == "Sylhet");

            var greenLineBus = savedBuses.First(b => b.Name == "Green Line Express");
            var shyamoliBus = savedBuses.First(b => b.Name == "Shyamoli Luxury");
            var enaBus = savedBuses.First(b => b.Name == "Ena Transport");
            var hanifBus = savedBuses.First(b => b.Name == "Hanif Enterprise");

            // Seed Bus Schedules for next 7 days
            var schedules = new List<BusSchedule>();
            var baseDate = DateTime.Today;

            // Green Line Express - Dhaka to Rajshahi (Multiple schedules)
            for (int day = 0; day < 7; day++)
            {
                var journeyDate = baseDate.AddDays(day);
                
                schedules.Add(new BusSchedule(
                    greenLineBus.Id,
                    dhakaRajshahiRoute.Id,
                    journeyDate.AddHours(8), // 8:00 AM departure
                    journeyDate.AddHours(13).AddMinutes(30), // 1:30 PM arrival
                    journeyDate,
                    new Money(800, "BDT")
                ));

                schedules.Add(new BusSchedule(
                    greenLineBus.Id,
                    dhakaRajshahiRoute.Id,
                    journeyDate.AddHours(22), // 10:00 PM departure
                    journeyDate.AddDays(1).AddHours(3).AddMinutes(30), // 3:30 AM next day arrival
                    journeyDate,
                    new Money(900, "BDT")
                ));
            }

            // Hanif Enterprise - Dhaka to Rajshahi
            for (int day = 0; day < 7; day++)
            {
                var journeyDate = baseDate.AddDays(day);
                
                schedules.Add(new BusSchedule(
                    hanifBus.Id,
                    dhakaRajshahiRoute.Id,
                    journeyDate.AddHours(10), // 10:00 AM departure
                    journeyDate.AddHours(15).AddMinutes(30), // 3:30 PM arrival
                    journeyDate,
                    new Money(850, "BDT")
                ));
            }

            // Shyamoli Luxury - Dhaka to Chittagong
            for (int day = 0; day < 7; day++)
            {
                var journeyDate = baseDate.AddDays(day);
                
                schedules.Add(new BusSchedule(
                    shyamoliBus.Id,
                    dhakaChittagongRoute.Id,
                    journeyDate.AddHours(9), // 9:00 AM departure
                    journeyDate.AddHours(15), // 3:00 PM arrival
                    journeyDate,
                    new Money(1200, "BDT")
                ));

                schedules.Add(new BusSchedule(
                    shyamoliBus.Id,
                    dhakaChittagongRoute.Id,
                    journeyDate.AddHours(23), // 11:00 PM departure
                    journeyDate.AddDays(1).AddHours(5), // 5:00 AM next day arrival
                    journeyDate,
                    new Money(1400, "BDT")
                ));
            }

            // Ena Transport - Dhaka to Sylhet
            for (int day = 0; day < 7; day++)
            {
                var journeyDate = baseDate.AddDays(day);
                
                schedules.Add(new BusSchedule(
                    enaBus.Id,
                    dhakaSylhetRoute.Id,
                    journeyDate.AddHours(7).AddMinutes(30), // 7:30 AM departure
                    journeyDate.AddHours(12).AddMinutes(30), // 12:30 PM arrival
                    journeyDate,
                    new Money(650, "BDT")
                ));

                schedules.Add(new BusSchedule(
                    enaBus.Id,
                    dhakaSylhetRoute.Id,
                    journeyDate.AddHours(21), // 9:00 PM departure
                    journeyDate.AddDays(1).AddHours(2), // 2:00 AM next day arrival
                    journeyDate,
                    new Money(700, "BDT")
                ));
            }

            await context.BusSchedules.AddRangeAsync(schedules);
            await context.SaveChangesAsync();

            Console.WriteLine($"✅ Seed Data Added Successfully!");
            Console.WriteLine($"   - {routes.Count} Routes");
            Console.WriteLine($"   - {buses.Count} Buses");
            Console.WriteLine($"   - {schedules.Count} Bus Schedules");
            Console.WriteLine($"   - Routes: Dhaka-Rajshahi, Dhaka-Chittagong, Dhaka-Sylhet");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error seeding data: {ex.Message}");
            throw;
        }
    }
}
