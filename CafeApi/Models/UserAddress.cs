using System;
using System.Collections.Generic;

namespace CafeApi.Models
{
    public partial class UserAddress
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Address { get; set; } = null!;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int Status { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
