using System;

namespace QLBDN.Models
{
    // ViewModel dùng cho 1 dòng trong bảng xếp hạng
    public class StandingsRowViewModel
    {
        public int ClubId { get; set; }
        public string ClubName { get; set; } = string.Empty;

        public int Played { get; set; }
        public int Won { get; set; }
        public int Drawn { get; set; }
        public int Lost { get; set; }

        public int GoalsFor { get; set; }      // Bàn thắng
        public int GoalsAgainst { get; set; }  // Bàn thua
        public int GoalDifference => GoalsFor - GoalsAgainst;

        public int Points => Won * 3 + Drawn;  // Thắng 3đ, Hòa 1đ

        // Vị trí trên BXH (sau khi sắp xếp)
        public int Rank { get; set; }
    }
}
