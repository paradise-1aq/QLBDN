using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLBDN.Models
{
    [Table("TICKET_BOOKING")]
    public class TicketBooking
    {
        [Key]
        [Column("BookingID")]
        public int BookingId { get; set; }

        [Column("BookingDateTime")]
        public DateTime BookingDateTime { get; set; }

        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public string? Status { get; set; }

        [Column("UserID")]
        public int UserId { get; set; }

        [Column("MatchID")]
        public int MatchId { get; set; }

        // === Navigation ===
        public virtual Match Match { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
