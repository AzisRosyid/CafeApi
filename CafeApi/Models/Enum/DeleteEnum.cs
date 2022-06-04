using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace CafeApi.Models.Enum
{
    public class DeleteEnum
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum DeleteType
        {
            [EnumMember(Value = "Safe")]
            Safe,
            [EnumMember(Value = "Permanent")]
            Permanent
        }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum Delete
        {
            [EnumMember(Value = "Deleted")]
            Deleted = 0,
            [EnumMember(Value = "Not Deleted")]
            NotDeleted = 1,
        }
    }
}
