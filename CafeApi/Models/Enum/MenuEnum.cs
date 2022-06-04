using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace CafeApi.Models.Enum
{
    public class MenuEnum
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum MenuType
        {
            [EnumMember(Value = "Like")]
            Like,
            [EnumMember(Value = "Bookmark")]
            Bookmark,
            [EnumMember(Value = "Review")]
            Review,
            [EnumMember(Value = "Transaction")]
            Transaction,
            [EnumMember(Value = "Current Transaction")]
            CurrentTransaction,
            [EnumMember(Value = "Transaction")]
            HistoryTransaction,
            [EnumMember(Value = "Failed Transaction")]
            FailedTransaction,
        }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum MenuSort
        {
            [EnumMember(Value = "Id")]
            Id,
            [EnumMember(Value = "Category Id")]
            CategoryId,
            [EnumMember(Value = "Name")]
            Name,
            [EnumMember(Value = "Price")]
            Price,
            [EnumMember(Value = "Stock")]
            Stock,
            [EnumMember(Value = "Image")]
            Image,
            [EnumMember(Value = "Description")]
            Description,
            [EnumMember(Value = "Like")]
            Like,
            [EnumMember(Value = "Bookmark")]
            Bookmark,
            [EnumMember(Value = "Rating")]
            Rating,
            [EnumMember(Value = "Review")]
            Review,
            [EnumMember(Value = "Date Created")]
            DateCreated,
            [EnumMember(Value = "Date Updated")]
            DateUpdated,
            [EnumMember(Value = "Date Deleted")]
            DateDeleted
        }
    }
}
