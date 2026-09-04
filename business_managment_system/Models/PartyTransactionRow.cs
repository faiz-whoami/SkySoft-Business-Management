using System;

namespace business_managment_system.Models
{
    public class PartyTransactionRow
    {
        public int TransactionId { get; set; }
        public string TransactionType { get; set; }
        public string Status { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
