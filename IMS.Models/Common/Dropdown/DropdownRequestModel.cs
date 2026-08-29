
namespace IMS.Models.Common.Dropdown
{
    /// <summary>
    /// Generic request model for all dropdowns.
    /// Supports normal dropdowns, cascading dropdowns,
    /// search and future pagination.
    /// </summary>
    public class DropdownRequestModel
    {
        /// <summary>
        /// Registered entity name.
        /// Example:
        /// PaymentMode
        /// ProductCategory
        /// ProductBrand
        /// ProductUnit
        /// Warehouse
        /// VendorCategory
        /// </summary>
        public string EntityType { get; set; }

        /// <summary>
        /// Parent Id for cascading dropdowns.
        /// Example:
        /// Country -> State
        /// State -> District
        /// Category -> Product
        /// </summary>
        public int? ParentId { get; set; }

        /// <summary>
        /// Search text.
        /// Used for Select2 / autocomplete.
        /// </summary>
        public string Search { get; set; }

        /// <summary>
        /// Returns only active records.
        /// Default = true.
        /// </summary>
        public bool ActiveOnly { get; set; } = true;

        /// <summary>
        /// Page number.
        /// Reserved for future server-side paging.
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// Page size.
        /// Reserved for future lazy loading.
        /// </summary>
        public int PageSize { get; set; } = 100;
    }
}
