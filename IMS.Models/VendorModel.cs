using System.ComponentModel.DataAnnotations;
using IMS.Models;
namespace IMS.Models
{
    // ============================================================================
    // VENDOR MASTER
    // ============================================================================
    public class VendorModel
    {
        public int VendorId { get; set; }

        [Required(ErrorMessage = "Vendor code is required")]
        [StringLength(50, ErrorMessage = "Max 50 characters")]
        [Display(Name = "Vendor Code")]
        public string VendorCode { get; set; }

        [Required(ErrorMessage = "Vendor name is required")]
        [StringLength(200, ErrorMessage = "Max 200 characters")]
        [Display(Name = "Vendor Name")]
        public string VendorName { get; set; }

        [Display(Name = "Category")]
        public int? VendorCategoryId { get; set; }
        public string VendorCategory { get; set; }

        [StringLength(50)]
        [Display(Name = "Tax Registration No.")]
        public string TaxRegistrationNumber { get; set; }

        [StringLength(10)]
        [Display(Name = "Currency")]
        public string CurrencyCode { get; set; }
        public string CurrencyName { get; set; }
        public string CurrencySymbol { get; set; }

        [Display(Name = "Overall Rating")]
        public decimal? OverallRating { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        public int? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // Related Data
        public List<VendorAddressModel> Addresses { get; set; } = new();
        public List<VendorContactModel> Contacts { get; set; } = new();
        public List<VendorBankModel> BankDetails { get; set; } = new();
        public List<VendorDocumentModel> Documents { get; set; } = new();

        // Dropdown sources
        public List<VendorCategoryModel> Categories { get; set; } = new();
        public List<CurrencyModel> Currencies { get; set; } = new();
    }

    // ============================================================================
    // VENDOR LIST ITEM (for grid / index)
    // ============================================================================
    public class VendorListItemModel
    {
        public int VendorId { get; set; }
        public string VendorCode { get; set; }
        public string VendorName { get; set; }
        public string VendorCategory { get; set; }
        public string TaxRegistrationNumber { get; set; }
        public string CurrencyCode { get; set; }
        public decimal? OverallRating { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public string PrimaryContactName { get; set; }
        public string PrimaryContactPhone { get; set; }
        public string PrimaryContactEmail { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public int TotalCount { get; set; }
    }

    // ============================================================================
    // VENDOR LIST FILTER
    // ============================================================================
    public class VendorFilterModel
    {
        public string SearchTerm { get; set; }
        public int? VendorCategoryId { get; set; }
        public bool? IsActive { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        public List<VendorListItemModel> Vendors { get; set; } = new();
        public List<VendorCategoryModel> Categories { get; set; } = new();
    }





    //public class VendorModel
    //{
    //    public int VendorId { get; set; }

    //    public string VendorCode { get; set; }

    //    [Required]
    //    [Display(Name = "Vendor Name")]
    //    public string VendorName { get; set; }

    //    public string ContactPerson { get; set; }

    //    [EmailAddress]
    //    public string Email { get; set; }

    //    public string MobileNo { get; set; }

    //    public string GSTNo { get; set; }

    //    public string PANNo { get; set; }

    //    public string Address { get; set; }

    //    public string City { get; set; }

    //    public string State { get; set; }

    //    public string Country { get; set; }

    //    public string PinCode { get; set; }

    //    public bool IsActive { get; set; }

    //}
}
