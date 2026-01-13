using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLBDN.Models
{
    [Table("SEASON")]
    public class Season
    {
        [Key]
        [Column("SeasonID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SeasonId { get; set; }

        [Required]
        [Column("Name")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Column("StartDate")]
        public DateTime? StartDate { get; set; }

        [Column("EndDate")]
        public DateTime? EndDate { get; set; }
    }
}
