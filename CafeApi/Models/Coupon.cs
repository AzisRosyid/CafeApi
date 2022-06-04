using System;
using System.Collections.Generic;

namespace CafeApi.Models
{
    public partial class Coupon
    {
        public Coupon()
        {
            UserCoupons = new HashSet<UserCoupon>();
        }

        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public double Value { get; set; }
        public double MinTransaction { get; set; }
        public double MaxValue { get; set; }
        public int Status { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public DateTime? DateDeleted { get; set; }

        public virtual ICollection<UserCoupon> UserCoupons { get; set; }
    }
}
