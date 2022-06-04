using System;
using System.Collections.Generic;

namespace CafeApi.Models
{
    public partial class Bookmark
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long MenuId { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public DateTime? DateDeleted { get; set; }

        public virtual Menu Menu { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
