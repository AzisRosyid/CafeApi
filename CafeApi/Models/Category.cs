using System;
using System.Collections.Generic;

namespace CafeApi.Models
{
    public partial class Category
    {
        public Category()
        {
            InverseParent = new HashSet<Category>();
            Menus = new HashSet<Menu>();
        }

        public long Id { get; set; }
        public long? ParentId { get; set; }
        public string Name { get; set; } = null!;
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public DateTime? DateDeleted { get; set; }

        public virtual Category? Parent { get; set; }
        public virtual ICollection<Category> InverseParent { get; set; }
        public virtual ICollection<Menu> Menus { get; set; }
    }
}
