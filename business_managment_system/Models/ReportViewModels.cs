using System.Collections.Generic;

namespace business_managment_system.Models
{
    public class ReportIndexViewModel
    {
        public string PartyType { get; set; }
        public int? TransactionId { get; set; }
        public int? Year { get; set; }
        public IList<int> Years { get; set; }

        public ReportIndexViewModel()
        {
            Years = new List<int>();
        }
    }
}
