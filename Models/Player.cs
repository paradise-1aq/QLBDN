using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLBDN.Models
{
    [Table("PLAYER")]
    public class Player
    {
        [Key]
        [Column("PlayerID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PlayerId { get; set; }

        [Required]
        [Column("FullName")]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Column("DateOfBirth")]
        public DateTime? DateOfBirth { get; set; }

        [Column("Nationality")]
        [MaxLength(50)]
        public string? Nationality { get; set; }

        [Column("Status")]
        [MaxLength(20)]
        public string? Status { get; set; }

        [Column("ShirtNumber")]
        public int? ShirtNumber { get; set; }

        [Column("AvatarUrl")]
        [MaxLength(255)]
        public string? AvatarUrl { get; set; }

        [Column("ClubID")]
        public int? ClubId { get; set; }

        [Column("RoleID")]
        public int? RoleId { get; set; }

        // 🔗 Navigation
        [ForeignKey(nameof(ClubId))]
        public Club? Club { get; set; }

        [ForeignKey(nameof(RoleId))]
        public Role? Role { get; set; }

        // 🧩 Helper: hiển thị tên vị trí
        [NotMapped]
        public string Position =>
            Role?.RoleName ??
            (RoleId switch
            {
                1 => "Thủ môn",
                2 => "Hậu vệ",
                3 => "Tiền vệ",
                4 => "Tiền đạo",
                _ => "Chưa xác định"
            });

        // 📸 Helper: lấy ảnh hiển thị
        [NotMapped]
        public string AvatarDisplay =>
            !string.IsNullOrEmpty(AvatarUrl)
                ? AvatarUrl
                : "/images/default-avatar.png";

        // 🟢 Helper: hiển thị trạng thái thân thiện
        [NotMapped]
        public string StatusDisplay =>
            string.IsNullOrEmpty(Status)
                ? "Không xác định"
                : Status switch
                {
                    "Active" => "Đang thi đấu",
                    "Injured" => "Chấn thương",
                    "Suspended" => "Treo giò",
                    "Retired" => "Giải nghệ",
                    _ => Status
                };
        public virtual ICollection<MatchEvent> MatchEvents { get; set; } = new List<MatchEvent>();

        // ✅ Đây là phần bạn cần thêm để upload ảnh
        [NotMapped]
        public IFormFile? AvatarFile { get; set; }
    }
}
