namespace CafeApi.Models.Lists
{
    public class CategoryList
    {
        public long Id { get; set; }
        public long? ParentId { get; set; }
        public string Name { get; set; } = null!;
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public DateTime? DateDeleted { get; set; }
        
    }
}
