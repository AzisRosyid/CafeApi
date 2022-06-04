using System.ComponentModel.DataAnnotations;
using static CafeApi.Models.Enum.UserAddressEnum;

namespace CafeApi.Models.List
{
    public class UserAddressList
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Address { get; set; } = null!;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        [EnumDataType(typeof(UserAddressStatus))]
        public UserAddressStatus Status { get; set; }
    }
}
