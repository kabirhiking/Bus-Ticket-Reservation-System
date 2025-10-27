namespace BusTicketReservation.WebApi.DTOs
{
    public class TestDto
    {
        public long Id { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateTestDto
    {
        public DateTime? CreatedAt { get; set; }
    }
}