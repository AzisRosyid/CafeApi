using System;
using System.Collections.Generic;

namespace CafeApi.Models
{
    public partial class User
    {
        public User()
        {
            Bookmarks = new HashSet<Bookmark>();
            Likes = new HashSet<Like>();
            Reviews = new HashSet<Review>();
            Transactions = new HashSet<Transaction>();
            UserAddresses = new HashSet<UserAddress>();
            UserCoupons = new HashSet<UserCoupon>();
        }

        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public int Type { get; set; }
        public int Role { get; set; }
        public int Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string PhoneNumber { get; set; } = null!;
        public string? Image { get; set; }
        public int Status { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public DateTime? DateDeleted { get; set; }

        public virtual ICollection<Bookmark> Bookmarks { get; set; }
        public virtual ICollection<Like> Likes { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
        public virtual ICollection<UserAddress> UserAddresses { get; set; }
        public virtual ICollection<UserCoupon> UserCoupons { get; set; }
    }
}
