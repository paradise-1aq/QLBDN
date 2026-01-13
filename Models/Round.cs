using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLBDN.Models
{
    [Table("ROUND")]
    public class Round
    {
        [Key]
        [Column("RoundID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RoundId { get; set; }

        [Required]
        [Column("RoundName")]
        [MaxLength(100)]
        public string RoundName { get; set; } = string.Empty;

        [Column("TableName")]
        [MaxLength(2)]
        public string? TableName { get; set; }
    }
}
