using System;
using System.Collections.Generic;

namespace CafeApi.Models
{
    public partial class Menu
    {
        public Menu()
        {
            Bookmarks = new HashSet<Bookmark>();
            Likes = new HashSet<Like>();
            MenuContents = new HashSet<MenuContent>();
            TransactionDetails = new HashSet<TransactionDetail>();
        }

        public long Id { get; set; }
        public long? CategoryId { get; set; }
        public string Name { get; set; } = null!;
        public double Price { get; set; }
        public long Stock { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public DateTime? DateDeleted { get; set; }

        public virtual Category? Category { get; set; }
        public virtual ICollection<Bookmark> Bookmarks { get; set; }
        public virtual ICollection<Like> Likes { get; set; }
        public virtual ICollection<MenuContent> MenuContents { get; set; }
        public virtual ICollection<TransactionDetail> TransactionDetails { get; set; }
    }
}
