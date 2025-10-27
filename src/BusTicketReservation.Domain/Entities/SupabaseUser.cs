using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace BusTicketReservation.Domain.Entities
{
    [Table("Users")]
    public class SupabaseUser : BaseModel
    {
        [PrimaryKey("Id")]
        public Guid Id { get; set; }

        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [Column("full_name")]
        public string FullName { get; set; } = string.Empty;

        [Column("phone_number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Column("password_hash")]
        public string PasswordHash { get; set; } = string.Empty;

        [Column("is_email_confirmed")]
        public bool IsEmailConfirmed { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}