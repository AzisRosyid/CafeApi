using System;
using System.Collections.Generic;

namespace CafeApi.Models
{
    public partial class TransactionDetail
    {
        public TransactionDetail()
        {
            Reviews = new HashSet<Review>();
        }

        public long Id { get; set; }
        public long TransactionId { get; set; }
        public long MenuId { get; set; }
        public int Quantity { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public DateTime? DateDeleted { get; set; }

        public virtual Menu Menu { get; set; } = null!;
        public virtual Transaction Transaction { get; set; } = null!;
        public virtual ICollection<Review> Reviews { get; set; }
    }
}
