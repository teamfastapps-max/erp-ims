using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Models
{
    public class VendorBankModel
    {
        public int VendorBankId { get; set; }
        public int VendorId { get; set; }

        [Required(ErrorMessage = "Bank name is required")]
        [Display(Name = "Bank Name")]
        public string BankName { get; set; }

        [Required(ErrorMessage = "Account holder name is required")]
        [Display(Name = "Account Holder Name")]
        public string AccountHolderName { get; set; }

        [Required(ErrorMessage = "Account number is required")]
        [Display(Name = "Account Number")]
        public string AccountNumber { get; set; }

        [Display(Name = "IFSC / SWIFT Code")]
        public string IFSCOrSwiftCode { get; set; }

        [Display(Name = "Branch Name")]
        public string BranchName { get; set; }

        [Display(Name = "Primary Account")]
        public bool IsPrimary { get; set; }
    }
}
