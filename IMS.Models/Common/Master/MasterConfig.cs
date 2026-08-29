using System;
using System.Linq;

namespace IMS.Models.Common.Master
{
    /// <summary>
    /// Describes a single field on a master entity — used to drive
    /// dynamic form rendering, validation, and SQL column mapping.
    /// </summary>
    public class MasterFieldConfig
    {
        public string ColumnName { get; set; }      // actual DB column name
        public string GridColumnName { get; set; }   // dropdown id's name /grid column name (if different from ColumnName)
        public string DisplayName { get; set; }      // label shown in UI
        public string PropertyName { get; set; }     // property name on the model
        public MasterFieldType FieldType { get; set; } = MasterFieldType.Text;

        public bool IsRequired { get; set; }
        public int? MaxLength { get; set; }
        public bool IsUnique { get; set; }            // triggers uniqueness check in service
        public bool ShowInGrid { get; set; } = true;
        public bool ShowInForm { get; set; } = true;

        // For dropdown/FK fields (e.g. BankBranch -> Bank)
        public string LookupEntityType { get; set; }  // references another MasterConfig.EntityType
        public string LookupValueField { get; set; }  // e.g. "Id"
        public string LookupTextField { get; set; }   // e.g. "Name"

        // Date-specific configuration (only applies when FieldType == MasterFieldType.Date)
        public DateTime? MinDate { get; set; }
        public DateTime? MaxDate { get; set; }
        public bool AllowFutureDates { get; set; } = true;
        public bool AllowPastDates { get; set; } = true;

        // Date range validation (optional — for paired date fields)
        public string DateRangeStartField { get; set; }  // ColumnName of the start date field
        public string DateRangeEndField { get; set; }    // ColumnName of the end date field
    }

    public enum MasterFieldType
    {
        Text,
        Number,
        Boolean,
        Dropdown,
        TextArea,
        Date,DateTime 
    }

    /// <summary>
    /// Central metadata describing a master table for the generic CRUD engine.
    /// One instance per entity (Bank, PaymentMode, Product, ProductBrand,
    /// ProductCategory, ProductUnit, VendorCategory, Warehouse, etc.)
    /// </summary>
    public class MasterConfig
    {
        public string EntityType { get; set; }        // e.g. "Bank", "Warehouse"
        public string TableName { get; set; }          // e.g. "dbo.BankMaster_BM"
        public string KeyColumn { get; set; } = "Id";
        public string DisplayName { get; set; }         // e.g. "Bank Master" (page title)
        public string SpName { get; set; }   // e.g. "USP_Bank" — single multi-action SP for this entity

        // ADD THIS:
        public string IsActiveColumn =>
            Fields.FirstOrDefault(f => f.FieldType == MasterFieldType.Boolean)?.ColumnName;


        public List<MasterFieldConfig> Fields { get; set; } = new List<MasterFieldConfig>();

        // Permission keys (checked via Permissions constants)
        public string ViewPermission { get; set; }
        public string CreatePermission { get; set; }
        public string EditPermission { get; set; }
        public string DeletePermission { get; set; }

        // Whether delete is a hard delete or soft (IsActive = false)
        public bool SoftDelete { get; set; } = true;

        public bool HasAuditColumns { get; set; } = true;

        public int MenuOrder { get; set; }
        public string Icon { get; set; }
    }
}