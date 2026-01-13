using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLBDN.Models
{
    [Table("USER")]
    public class User
    {
        [Key]
        [Column("UserID")]
        public int UserId { get; set; }

        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Password { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Role { get; set; } = string.Empty;

        public ICollection<Referee> Referees { get; set; } = new List<Referee>();


        // 🔥 Navigation: 1 User có nhiều TicketBooking
        [InverseProperty(nameof(TicketBooking.User))]
        public virtual ICollection<TicketBooking> TicketBookings { get; set; }
            = new List<TicketBooking>();

        public virtual ICollection<News> News { get; set; } = new List<News>();
        public virtual ICollection<Interaction> Interactions { get; set; } = new List<Interaction>();


    }
}
