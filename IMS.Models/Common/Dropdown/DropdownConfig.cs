
namespace IMS.Models.Common.Dropdown
{
   
    /// <summary>
    /// Configuration for the Generic Dropdown Framework.
    /// One configuration per dropdown entity.
    ///
    /// Examples:
    /// PaymentMode
    /// ProductCategory
    /// ProductBrand
    /// ProductUnit
    /// Warehouse
    /// VendorCategory
    /// Bank
    /// BankBranch
    /// </summary>
    public class DropdownConfig
    {
        /// <summary>
        /// Unique entity name.
        /// Example:
        /// PaymentMode
        /// ProductCategory
        /// </summary>
        public string EntityType { get; set; }

        /// <summary>
        /// Database table name.
        /// Example:
        /// dbo.PaymentMode_PM
        /// dbo.ProductCategory_PC
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// Primary Key column.
        /// Example:
        /// PM_Id
        /// PC_Id
        /// </summary>
        public string KeyColumn { get; set; }

        /// <summary>
        /// Value column returned to dropdown.
        /// Usually same as KeyColumn.
        /// </summary>
        public string ValueColumn { get; set; }

        /// <summary>
        /// Display text column.
        /// Example:
        /// PM_ModeName
        /// PC_CategoryName
        /// </summary>
        public string TextColumn { get; set; }

        /// <summary>
        /// Optional code column.
        /// Example:
        /// PM_ModeCode
        /// PB_BrandCode
        /// </summary>
        public string CodeColumn { get; set; }

        /// <summary>
        /// Active column.
        /// Used when ActiveOnly = true.
        /// Example:
        /// PM_IsActive
        /// PC_IsActive
        /// </summary>
        public string ActiveColumn { get; set; }

        /// <summary>
        /// Parent column used for cascading dropdowns.
        /// Example:
        /// State -> CountryId
        /// District -> StateId
        /// Product -> CategoryId
        /// </summary>
        public string ParentColumn { get; set; }

        /// <summary>
        /// Parent entity type.
        /// Example:
        /// Country
        /// State
        /// ProductCategory
        /// </summary>
        public string ParentEntityType { get; set; }

        /// <summary>
        /// Default sort column.
        /// </summary>
        public string OrderByColumn { get; set; }

        /// <summary>
        /// Default sort direction.
        /// ASC / DESC
        /// </summary>
        public string OrderByDirection { get; set; } = "ASC";

        /// <summary>
        /// Additional filter if required.
        /// Example:
        /// CompanyId = 1
        /// TenantId = 5
        /// </summary>
        public string AdditionalWhereClause { get; set; }
    }
    
}
