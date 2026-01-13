using System.ComponentModel.DataAnnotations;

namespace QLBDN.Models
{
    public class AssignRefereesViewModel
    {
        public int MatchId { get; set; }

        [Display(Name = "Trọng tài chính")]
        public int? MainRefereeId { get; set; }

        [Display(Name = "Trợ lý 1")]
        public int? Assistant1Id { get; set; }

        [Display(Name = "Trợ lý 2")]
        public int? Assistant2Id { get; set; }

        [Display(Name = "Trọng tài bàn")]
        public int? FourthOfficialId { get; set; }

        [Display(Name = "VAR")]
        public int? VarRefereeId { get; set; }
    }
}
