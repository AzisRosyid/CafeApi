using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace CafeApi.Models.Enum
{
    public class MenuContentEnum
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum MenuContentType
        {
            [EnumMember(Value = "Image")]
            Image = 0,
            [EnumMember(Value = "Description")]
            Description = 1
        }
    }
}
