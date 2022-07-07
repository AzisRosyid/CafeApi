using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace CafeApi.Models.Enums
{
    public class UserEnum
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum UserRole
        {
            [EnumMember(Value = "Admin")]
            Admin = 0,
            [EnumMember(Value = "Employee")]
            Employee = 1,
            [EnumMember(Value = "Customer")]
            Customer = 2
        }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum UserGender
        {
            [EnumMember(Value = "Hide")]
            Hide = 0,
            [EnumMember(Value = "Male")]
            Male = 1,
            [EnumMember(Value = "Female")]
            Female = 2
        }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum UserType
        {
            [EnumMember(Value = "Local")]
            Local = 0,
            [EnumMember(Value = "Google")]
            Google = 1,
            [EnumMember(Value = "Facebook")]
            Facebook = 2
        }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum UserStatus
        {
            [EnumMember(Value = "Active")]
            Active = 0,
            [EnumMember(Value = "Not Active")]
            NotActive = 1 
        }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum UserSort
        {
            [EnumMember(Value = "Id")]
            Id,
            [EnumMember(Value = "Name")]
            Name,
            [EnumMember(Value = "Email")]
            Email,
            [EnumMember(Value = "Type")]
            Type,
            [EnumMember(Value = "Role")]
            Role,
            [EnumMember(Value = "Gender")]
            Gender,
            [EnumMember(Value = "Date Of Birth")]
            DateOfBirth,
            [EnumMember(Value = "Phone Number")]
            PhoneNumber,
            [EnumMember(Value = "Address")]
            Address,
            [EnumMember(Value = "Status")]
            Status,
            [EnumMember(Value = "Date Created")]
            DateCreated,
            [EnumMember(Value = "Date Updated")]
            DateUpdated,
            [EnumMember(Value = "Date Deleted")]
            DateDeleted
        }
    }
}
