using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace CafeApi.Models.Enum
{
    public class CoinEnum
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum CoinStatus
        {
            [EnumMember(Value = "Increase")]
            Increase,
            [EnumMember(Value = "Decrease")]
            Decrease
        }
    }
}
