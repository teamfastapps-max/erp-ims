using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Models
{
    public class VendorAddressModel
    {
        public int VendorAddressId { get; set; }
        public int VendorId { get; set; }

        [Required(ErrorMessage = "Address type is required")]
        [Display(Name = "Address Type")]
        public string AddressType { get; set; } = "Office";

        [Display(Name = "Address Line 1")]
        public string AddressLine1 { get; set; }

        [Display(Name = "Address Line 2")]
        public string AddressLine2 { get; set; }

        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }

        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; }

        [Display(Name = "Primary Address")]
        public bool IsPrimary { get; set; }
    }
}
