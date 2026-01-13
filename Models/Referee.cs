namespace QLBDN.Models
{
    public class Referee
    {
        public int RefereeId { get; set; }
        public string FullName { get; set; }

        public DateTime? DateOfBirth { get; set; }
        public int? Experience { get; set; }
        public string? Level { get; set; }

        public int? UserId { get; set; }

        // ───────────────────────────────
        //   Navigation properties
        // ───────────────────────────────
        public User? User { get; set; }

        public ICollection<RefereeMatch> RefereeMatches { get; set; } = new List<RefereeMatch>();
    }
}
