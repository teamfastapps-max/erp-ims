using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Models
{
    public class VendorContactModel
    {
        public int VendorContactId { get; set; }
        public int VendorId { get; set; }

        [Required(ErrorMessage = "Contact name is required")]
        [Display(Name = "Contact Name")]
        public string ContactName { get; set; }

        public string Designation { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        public string Phone { get; set; }

        [Display(Name = "Primary Contact")]
        public bool IsPrimary { get; set; }
    }
}
