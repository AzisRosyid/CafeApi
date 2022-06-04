using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace CafeApi.Models.List
{
    public class TransactionEnum
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum TransactionStatus
        {
            [EnumMember(Value = "Cancelled")]
            Cancelled,
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
