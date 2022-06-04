using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using static CafeApi.Models.Enum.MenuContentEnum;

namespace CafeApi.Models.Parameter
{
    public class MenuParameter
    {
        public long? CategoryId { get; set; }

        [BindRequired]
        [Required]
        public string Name { get; set; } = null!;

        [BindRequired]
        [Required]
        public double Price { get; set; }

        [BindRequired]
        [Required]
        public long Stock { get; set; }

        public bool? ImageDelete { get; set; }
        public List<int>? ImageOrders { get; set; }
        public List<IFormFile>? Images { get; set; }
        public List<int>? DescriptionOrders { get; set; }
        public List<string>? Descriptions { get; set; }
    }
}
