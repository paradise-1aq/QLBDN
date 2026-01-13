namespace QLBDN.Models
{
    public class RefereeMatch
    {
        public int MatchId { get; set; }
        public int RefereeId { get; set; }

        public string? Role { get; set; }
        public DateTime? DateTime { get; set; }

        // ───────────────────────────────
        //   Navigation properties
        // ───────────────────────────────
        public Match Match { get; set; }
        public Referee Referee { get; set; }
    }
}
