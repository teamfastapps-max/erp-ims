
namespace IMS.Models.Common.Dropdown
{

        /// <summary>
        /// Central registry for all generic dropdown configurations.
        /// Register every dropdown once here.
        /// </summary>
    public static class DropdownConfigRegistry
    {
        private static readonly List<DropdownConfig> _configs = new List<DropdownConfig>
    {
        #region Payment Mode

        new DropdownConfig
        {
            EntityType = "PaymentMode",
            TableName = "dbo.PaymentMode_PM",
            KeyColumn = "PM_Id",
            ValueColumn = "PM_Id",
            TextColumn = "PM_ModeName",
            CodeColumn = "PM_ModeCode",
            ActiveColumn = "PM_IsActive",
            OrderByColumn = "PM_ModeName"
        },

        #endregion


        #region Product Category

        new DropdownConfig
        {
            EntityType = "ProductCategory",
            TableName = "dbo.ProductCategory_PC",
            KeyColumn = "PC_Id",
            ValueColumn = "PC_Id",
            TextColumn = "PC_CategoryName",
            CodeColumn = "PC_CategoryCode",
            ActiveColumn = "PC_IsActive",
            OrderByColumn = "PC_CategoryName"
        },

        #endregion


        #region Product Brand

        new DropdownConfig
        {
            EntityType = "ProductBrand",
            TableName = "dbo.ProductBrand_PB",
            KeyColumn = "PB_Id",
            ValueColumn = "PB_Id",
            TextColumn = "PB_BrandName",
            CodeColumn = "PB_BrandCode",
            ActiveColumn = "PB_IsActive",
            OrderByColumn = "PB_BrandName"
        },

        #endregion


        #region Product Unit

        new DropdownConfig
        {
            EntityType = "ProductUnit",
            TableName = "dbo.ProductUnit_PU",
            KeyColumn = "PU_Id",
            ValueColumn = "PU_Id",
            TextColumn = "PU_UnitName",
            CodeColumn = "PU_UnitCode",
            ActiveColumn = "PU_IsActive",
            OrderByColumn = "PU_UnitName"
        },

        #endregion


        #region Vendor Category

        new DropdownConfig
        {
            EntityType = "VendorCategory",
            TableName = "dbo.VendorCategories_VC",
            KeyColumn = "VC_Id",
            ValueColumn = "VC_Id",
            TextColumn = "VC_CategoryName",
            CodeColumn = "VC_CategoryCode",
            ActiveColumn = "VC_IsActive",
            OrderByColumn = "VC_CategoryName"
        },

        #endregion


        /*
            * Future Examples
            *
            * new DropdownConfig
            * {
            *      EntityType = "Warehouse",
            *      TableName = "dbo.Warehouse_WH",
            *      KeyColumn = "WH_Id",
            *      ValueColumn = "WH_Id",
            *      TextColumn = "WH_Name",
            *      CodeColumn = "WH_Code",
            *      ActiveColumn = "WH_IsActive",
            *      OrderByColumn = "WH_Name"
            * },
            *
            *
            * Cascading Example
            *
            * new DropdownConfig
            * {
            *      EntityType = "District",
            *      TableName = "dbo.District_DM",
            *
            *      KeyColumn = "DM_Id",
            *      ValueColumn = "DM_Id",
            *      TextColumn = "DM_Name",
            *
            *      ParentColumn = "DM_StateId",
            *      ParentEntityType = "State",
            *
            *      ActiveColumn = "DM_IsActive",
            *      OrderByColumn = "DM_Name"
            * }
            *
            */
    };

        /// <summary>
        /// Returns configuration by EntityType.
        /// </summary>
        public static DropdownConfig GetByEntityType(string entityType)
        {
            return _configs.FirstOrDefault(x =>
                x.EntityType.Equals(entityType, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Returns all registered dropdowns.
        /// </summary>
        public static List<DropdownConfig> GetAll()
        {
            return _configs;
        }
    }
}

