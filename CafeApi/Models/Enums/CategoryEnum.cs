using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace CafeApi.Models.Enums
{
    public class CategoryEnum
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum CategorySort
        {
            [EnumMember(Value = "Id")]
            Id,
            [EnumMember(Value = "ParentId")]
            ParentId,
            [EnumMember(Value = "Name")]
            Name,
            [EnumMember(Value = "DateCreated")]
            DateCreated,
            [EnumMember(Value = "DateUpdated")]
            DateUpdated,
            [EnumMember(Value = "DateDeleted")]
            DateDeleted
        }
    }
}
