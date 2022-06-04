using System;
using System.Collections.Generic;

namespace CafeApi.Models
{
    public partial class Coin
    {
        public long Id { get; set; }
        public long TransactionId { get; set; }
        public double Value { get; set; }
        public int Status { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public DateTime? DateDeleted { get; set; }

        public virtual Transaction Transaction { get; set; } = null!;
    }
}
