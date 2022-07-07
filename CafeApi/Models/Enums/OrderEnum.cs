using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace CafeApi.Models.Enums
{
    public class OrderEnum
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum Order
        {
            [EnumMember(Value = "Ascending")]
            Ascending,
            [EnumMember(Value = "Descending")]
            Descending,
        }
    }
}
