using System;
using System.Collections.Generic;

namespace CafeApi.Models
{
    public partial class MenuContent
    {
        public long Id { get; set; }
        public long MenuId { get; set; }
        public int Type { get; set; }
        public int TypeOrder { get; set; }
        public string? Value { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public DateTime? DateDeleted { get; set; }

        public virtual Menu Menu { get; set; } = null!;
    }
}
