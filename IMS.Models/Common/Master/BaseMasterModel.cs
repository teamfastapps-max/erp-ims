
namespace IMS.Models.Common.Master
{
    /// <summary>
    /// Base model for all master/lookup tables (Bank, PaymentMode, Product, 
    /// ProductBrand, ProductCategory, ProductUnit, VendorCategory, Warehouse, etc.)
    /// Individual master models inherit this and add entity-specific fields (e.g. FKs).
    /// </summary>
    public class BaseMasterModel
    {
        public Guid Id { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public bool IsActive { get; set; } = true;

        public Guid? CreatedBy { get; set; }

        public DateTime? CreatedOn { get; set; }

        public Guid? ModifiedBy { get; set; }

        public DateTime? ModifiedOn { get; set; }

        // Used by generic DAL/Service to know which physical table & entity this maps to
        public string EntityType { get; set; }

        /// <summary>
        /// for warehouse need these prop
        /// </summary>
        public string Address { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string Country { get; set; }

        public string PostalCode { get; set; }
    }
}