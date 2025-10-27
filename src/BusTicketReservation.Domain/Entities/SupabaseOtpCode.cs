using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace BusTicketReservation.Domain.Entities
{
    [Table("OtpCodes")]
    public class SupabaseOtpCode : BaseModel
    {
        [PrimaryKey("Id")]
        public Guid Id { get; set; }

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("code")]
        public string Code { get; set; } = string.Empty;

        [Column("expires_at")]
        public DateTime ExpiresAt { get; set; }

        [Column("is_used")]
        public bool IsUsed { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("purpose")]
        public string Purpose { get; set; } = string.Empty;
    }
}