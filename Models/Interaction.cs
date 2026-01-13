using System;

namespace QLBDN.Models
{
    public class Interaction
    {
        public int InteractionId { get; set; }

        public string? Content { get; set; }
        public string? Type { get; set; }
        public DateTime? DateTime { get; set; }

        public int? UserId { get; set; }
        public virtual User? User { get; set; }

        public int? NewsId { get; set; }
        public virtual News? News { get; set; }
    }

}
