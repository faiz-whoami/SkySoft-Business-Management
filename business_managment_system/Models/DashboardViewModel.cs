using System;
using System.Collections.Generic;

namespace business_managment_system.Models
{
    public class DashboardViewModel
    {
        public int ActiveCustomers { get; set; }
        public int ActiveSuppliers { get; set; }
        public int ActiveVendors { get; set; }
        public decimal MonthSalesTotal { get; set; }
        public string MonthLabel { get; set; }
        public IList<DashboardTransactionRow> RecentTransactions { get; set; }

        public DashboardViewModel()
        {
            RecentTransactions = new List<DashboardTransactionRow>();
        }
    }

    public class DashboardTransactionRow
    {
        public int TransactionId { get; set; }
        public string TransactionType { get; set; }
        public string Status { get; set; }
        public DateTime TransactionDate { get; set; }
        public string PartyName { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
