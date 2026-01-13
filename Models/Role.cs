using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLBDN.Models
{
    [Table("ROLE")]
    public class Role
    {
        [Key]
        [Column("RoleID")]
        public int RoleId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("RoleName")]
        public string RoleName { get; set; } = string.Empty;

        [Column("RoleDescription")]
        public string? RoleDescription { get; set; }

        // Liên kết ngược tới Player
        public ICollection<Player>? Players { get; set; }
    }
}
