using System;
using System.Collections.Generic;

namespace CafeApi.Models
{
    public partial class Review
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long TransactionDetailId { get; set; }
        public decimal Rating { get; set; }
        public string? Description { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public DateTime? DateDeleted { get; set; }

        public virtual TransactionDetail TransactionDetail { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
