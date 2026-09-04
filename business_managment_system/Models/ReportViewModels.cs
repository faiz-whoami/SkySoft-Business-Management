using System.Collections.Generic;

namespace business_managment_system.Models
{
    public class ReportIndexViewModel
    {
        public bool CrystalAvailable { get; set; }
        public string CrystalMessage { get; set; }
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
