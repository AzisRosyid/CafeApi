namespace CafeApi.Models.Lists
{
    public class MenuList
    {
        public long Id { get; set; }
        public CategoryList? CategoryId { get; set; }
        public string Name { get; set; } = null!;
        public double Price { get; set; }
        public long Stock { get; set; }
        public long Sold { get; set; }
        public List<MenuContentList> Images { get; set; }
        public List<MenuContentList> Descriptions { get; set; }
        public bool Bookmark { get; set; }
        public bool Like { get; set; }
        public decimal? Rating { get; set; }
        public List<ReviewList>? Reviews { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public DateTime? DateDeleted { get; set; }
    }
}
