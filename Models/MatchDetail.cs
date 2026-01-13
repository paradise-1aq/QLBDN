using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QLBDN.Models
{
    [Table("MATCH_DETAIL")]
    public class MatchDetail
    {
        [Key, Column(Order = 0)]
        public int MatchId { get; set; }

        [Key, Column(Order = 1)]
        public int ClubId { get; set; }

        [Column("Goals")]
        public int? Goals { get; set; }

        [Column("IsWinner")]
        public bool? IsWinner { get; set; }

        [Column("IsHomeTeam")]
        public bool? IsHomeTeam { get; set; }

        [ForeignKey("MatchId")]
        public Match? Match { get; set; }

        [ForeignKey("ClubId")]
        public Club? Club { get; set; }
    }
}
