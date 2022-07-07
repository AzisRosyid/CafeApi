using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace CafeApi.Models.Enums
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
