using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLBDN.Models
{
    [Table("MATCH")]
    public class Match
    {
        [Key]
        [Column("MatchID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MatchId { get; set; }

        [Column("DateTime")]
        public DateTime? DateTime { get; set; }

        [Column("Stadium")]
        [MaxLength(100)]
        public string? Stadium { get; set; }

        [Column("Status")]
        [MaxLength(50)]
        public string? Status { get; set; } // Completed, Scheduled, Ongoing, Cancelled

        [Column("RoundID")]
        public int? RoundId { get; set; }

        [Column("SeasonID")]
        public int? SeasonId { get; set; }

        // 🔗 Navigation properties
        [ForeignKey(nameof(RoundId))]
        public virtual Round? Round { get; set; }

        [ForeignKey(nameof(SeasonId))]
        public virtual Season? Season { get; set; }

        // Chi tiết trận đấu (CLB + số bàn thắng)
        [InverseProperty(nameof(MatchDetail.Match))]
        public virtual ICollection<MatchDetail> MatchDetails { get; set; }
            = new List<MatchDetail>();

        // 🔥 NEW — Navigation tới bảng TicketBooking (1 MATCH có nhiều vé)
        [InverseProperty(nameof(TicketBooking.Match))]
        public virtual ICollection<TicketBooking> TicketBookings { get; set; }
            = new List<TicketBooking>();

        public virtual ICollection<MatchEvent> MatchEvents { get; set; } = new List<MatchEvent>();
        public virtual ICollection<RefereeMatch> RefereeMatches { get; set; } = new List<RefereeMatch>();


        public override string ToString()
        {
            return $"Match {{ ID={MatchId}, Stadium={Stadium}, Status={Status}, RoundID={RoundId}, SeasonID={SeasonId} }}";
        }
    }
}
