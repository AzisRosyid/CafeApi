using CafeApi.Models.Parameters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using static CafeApi.Models.Enums.UserEnum;
using System.ComponentModel.DataAnnotations;

namespace CafeApi.Models.Parameters
{
    public class RegisterParameter
    {
        [BindRequired]
        [Required]
        [StringLength(150)]
        public string Name { get; set; } = null!;

        [BindRequired]
        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; } = null!;

        [BindRequired]
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [BindRequired]
        [Required]
        [EnumDataType(typeof(UserType))]
        public UserType Type { get; set; }

        [BindRequired]
        [Required]
        [EnumDataType(typeof(UserGender))]
        public UserGender Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [BindRequired]
        [Required]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; } = null!;

        public virtual UserAddressParameter? Addresses { get; set; } = null;

        public IFormFile? Image { get; set; }

        [BindRequired]
        [Required]
        [EnumDataType(typeof(UserStatus))]
        public UserStatus Status { get; set; }
    }
}
