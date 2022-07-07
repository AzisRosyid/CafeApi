namespace CafeApi.Models.Lists
{
    public class ReviewList
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long TransactionDetailId { get; set; }
        public decimal Rating { get; set; }
        public string? Description { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public DateTime? DateDeleted { get; set; }
    }
}
