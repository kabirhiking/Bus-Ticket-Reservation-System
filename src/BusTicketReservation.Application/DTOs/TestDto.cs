namespace BusTicketReservation.Application.DTOs
{
    public class TestDto
    {
        public long Id { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateTestDto
    {
        // No properties needed since the table only has auto-generated fields
        // This DTO exists for consistency and future extensibility
    }
}