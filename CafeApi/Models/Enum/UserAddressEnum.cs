using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace CafeApi.Models.Enum
{
    public class UserAddressEnum
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum UserAddressStatus
        {
            [EnumMember(Value = "Primary")]
            Primary = 0,
            [EnumMember(Value = "Secondary")]
            Secondary = 1
        }
    }
}
