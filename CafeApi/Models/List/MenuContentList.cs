using System.ComponentModel.DataAnnotations;
using static CafeApi.Models.Enum.MenuContentEnum;

namespace CafeApi.Models.List
{
    public class MenuContentList
    {
        public long Id { get; set; }
        public long MenuId { get; set; }
        [EnumDataType(typeof(MenuContentType))]
        public MenuContentType Type { get; set; }
        public int TypeOrder { get; set; }
        public string Value { get; set; } = null!;
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public DateTime? DateDeleted { get; set; }
    }
}
