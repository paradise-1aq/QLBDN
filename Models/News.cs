using System;
using System.Collections.Generic;

namespace QLBDN.Models
{
    public class News
    {
        public int NewsId { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public DateTime? PostedDate { get; set; }
        public string? ImageUrl { get; set; }

        public int? UserId { get; set; }
        public virtual User? User { get; set; }

        public virtual ICollection<Interaction> Interactions { get; set; } = new List<Interaction>();
    }

}
