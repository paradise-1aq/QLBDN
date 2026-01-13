using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLBDN.Models
{
    [Table("MATCH_EVENT")]
    public class MatchEvent
    {
        [Key]
        public int EventID { get; set; }

        [Required(ErrorMessage = "Hãy chọn loại sự kiện")]
        public string EventType { get; set; } = string.Empty;

        public DateTime? DateTime { get; set; } = System.DateTime.Now;

        public string? Description { get; set; }

        [ForeignKey("Match")]
        [Required(ErrorMessage = "Hãy chọn trận đấu")]
        public int MatchID { get; set; }

        public Match? Match { get; set; }   

        [ForeignKey("Player")]
        public int? PlayerID { get; set; }

        public Player? Player { get; set; } 
    }
}
