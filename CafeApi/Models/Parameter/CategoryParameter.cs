using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace CafeApi.Models.Parameter
{
    public class CategoryParameter
    {
        public long? ParentId { get; set; }

        [BindRequired]
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = null!;
    }
}
