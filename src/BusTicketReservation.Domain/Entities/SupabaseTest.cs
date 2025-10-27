using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace BusTicketReservation.Domain.Entities
{
    [Table("test")]
    public class SupabaseTest : BaseModel
    {
        [PrimaryKey("id", false)] // false means it's auto-generated
        public long Id { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}