using System;
using System.Collections.Generic;

namespace CafeApi.Models
{
    public partial class Transaction
    {
        public Transaction()
        {
            Coins = new HashSet<Coin>();
            TransactionDetails = new HashSet<TransactionDetail>();
        }

        public long Id { get; set; }
        public long UserId { get; set; }
        public long PaymentId { get; set; }
        public DateTime Date { get; set; }
        public double Total { get; set; }
        public string PaymentCode { get; set; } = null!;
        public int Status { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public DateTime? DateDeleted { get; set; }

        public virtual Payment Payment { get; set; } = null!;
        public virtual User User { get; set; } = null!;
        public virtual ICollection<Coin> Coins { get; set; }
        public virtual ICollection<TransactionDetail> TransactionDetails { get; set; }
    }
}
