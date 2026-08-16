using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Models
{
    public class VendorStatsModel
    {
        public int TotalVendors { get; set; }
        public int ActiveVendors { get; set; }
        public int InactiveVendors { get; set; }
        public decimal? AverageRating { get; set; }
        public int NewThisMonth { get; set; }
    }
}
