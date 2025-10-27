using BusTicketReservation.Domain.Common;

namespace BusTicketReservation.Domain.Entities
{
    public class User : BaseEntity, IAggregateRoot
    {
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? PasswordHash { get; set; }
        public bool IsEmailVerified { get; set; } = false;
        public DateTime? EmailVerifiedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
        public virtual ICollection<OtpCode> OtpCodes { get; set; } = new List<OtpCode>();

        public void MarkEmailAsVerified()
        {
            IsEmailVerified = true;
            EmailVerifiedAt = DateTime.UtcNow;
            MarkAsUpdated();
        }

        public void UpdateLastLogin()
        {
            LastLoginAt = DateTime.UtcNow;
            MarkAsUpdated();
        }
    }
}