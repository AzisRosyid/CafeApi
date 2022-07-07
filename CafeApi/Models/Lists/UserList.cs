using CafeApi.Models.Parameters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using static CafeApi.Models.Enums.UserEnum;

namespace CafeApi.Models.Lists
{
    public class UserList
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        [EnumDataType(typeof(UserType))]
        public UserType Type { get; set; }
        [EnumDataType(typeof(UserRole))]
        public UserRole Role { get; set; }
        [EnumDataType(typeof(UserGender))]
        public UserGender? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string PhoneNumber { get; set; } = null!;
        public List<UserAddressList> Address { get; set; }
        public string? Image { get; set; }
        [EnumDataType(typeof(UserStatus))]
        public double Coins { get; set; }
        public UserStatus Status { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public DateTime? DateDeleted { get; set; }
    }
}
