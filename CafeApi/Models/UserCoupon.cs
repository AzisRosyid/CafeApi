using System;
using System.Collections.Generic;

namespace CafeApi.Models
{
    public partial class UserCoupon
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long CouponId { get; set; }
        public string? Code { get; set; }
        public DateTime DateExpired { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public DateTime? DateDeleted { get; set; }

        public virtual Coupon Coupon { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
