using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLBDN.Models
{
    [Table("CLUB")]
    public class Club
    {
        [Key]
        [Column("ClubID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ClubId { get; set; }

        [Column("Name")]
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Column("HomeStadium")]
        [MaxLength(100)]
        public string? HomeStadium { get; set; }

        // Navigation
        public ICollection<Player>? Players { get; set; }
        public ICollection<MatchDetail>? MatchDetails { get; set; }
    }
}
