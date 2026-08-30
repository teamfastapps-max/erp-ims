using System;
using System.Collections.Generic;
using System.Linq;

namespace IMS.Models.Common.Master
{
    /// <summary>
    /// Single source of truth for all master entity configs.
    /// Generic DAL/Service/Controller all read from this registry
    /// instead of having table-specific logic scattered around.
    /// 
    /// Column names match actual database schema (verified via INFORMATION_SCHEMA.COLUMNS).
    /// </summary>
    public static class MasterConfigRegistry
    {
        private static readonly List<MasterConfig> _configs = new List<MasterConfig>
        {
            // NOTE: BankMaster_BM excluded — BM_Id is INT (identity), not UNIQUEIDENTIFIER.
            // The generic master module requires GUID keys. Bank should use a dedicated controller/SP.

            // 1. Branch Master
            new MasterConfig
            {
                EntityType = "Branch",
                SpName = "USP_Branches_B",
                TableName = "dbo.Branches_B",
                KeyColumn = "B_Id",
                DisplayName = "Branch Master",
                SoftDelete = true,
                HasAuditColumns = true,
                MenuOrder = 1,
                Icon = "fa-building",

                ViewPermission = "Master.Branch.View",
                CreatePermission = "Master.Branch.Create",
                EditPermission = "Master.Branch.Edit",
                DeletePermission = "Master.Branch.Delete",

                Fields = new List<MasterFieldConfig>
                {
                    new MasterFieldConfig { ColumnName = "B_Name", PropertyName = "Name", DisplayName = "Branch Name", IsRequired = true, IsUnique = true, MaxLength = 200 },
                    new MasterFieldConfig { ColumnName = "B_Code", PropertyName = "Code", DisplayName = "Branch Code", IsRequired = true, IsUnique = true, MaxLength = 50 },
                    new MasterFieldConfig { ColumnName = "B_Email", PropertyName = "Email", DisplayName = "Email", MaxLength = 255 },
                    new MasterFieldConfig { ColumnName = "B_Phone", PropertyName = "Phone", DisplayName = "Phone", MaxLength = 30 },
                    new MasterFieldConfig { ColumnName = "B_AddressLine1", PropertyName = "AddressLine1", DisplayName = "Address Line 1", MaxLength = 255 },
                    new MasterFieldConfig { ColumnName = "B_AddressLine2", PropertyName = "AddressLine2", DisplayName = "Address Line 2", MaxLength = 255 },
                    new MasterFieldConfig { ColumnName = "B_City", PropertyName = "City", DisplayName = "City", MaxLength = 100 },
                    new MasterFieldConfig { ColumnName = "B_State", PropertyName = "State", DisplayName = "State", MaxLength = 100 },
                    new MasterFieldConfig { ColumnName = "B_PostalCode", PropertyName = "PostalCode", DisplayName = "Postal Code", MaxLength = 20 },
                    new MasterFieldConfig { ColumnName = "B_CountryCode", PropertyName = "CountryCode", DisplayName = "Country Code", MaxLength = 2 }
                }
            },

            // 2. Course Master
            new MasterConfig
            {
                EntityType = "Course",
                SpName = "USP_Courses_C",
                TableName = "dbo.Courses_C",
                KeyColumn = "C_Id",
                DisplayName = "Course Master",
                SoftDelete = true,
                HasAuditColumns = true,
                MenuOrder = 2,
                Icon = "fa-book",

                ViewPermission = "Master.Course.View",
                CreatePermission = "Master.Course.Create",
                EditPermission = "Master.Course.Edit",
                DeletePermission = "Master.Course.Delete",

                Fields = new List<MasterFieldConfig>
                {
                    new MasterFieldConfig { ColumnName = "C_Name", PropertyName = "Name", DisplayName = "Course Name", IsRequired = true, IsUnique = true, MaxLength = 200 },
                    new MasterFieldConfig { ColumnName = "C_Code", PropertyName = "Code", DisplayName = "Course Code", IsRequired = true, IsUnique = true, MaxLength = 50 },
                    new MasterFieldConfig { ColumnName = "C_ProgramId", PropertyName = "ProgramId", DisplayName = "Program", FieldType = MasterFieldType.Dropdown, LookupEntityType = "Program", LookupValueField = "P_Id", LookupTextField = "P_Name" },
                    new MasterFieldConfig { ColumnName = "C_Description", PropertyName = "Description", DisplayName = "Description", FieldType = MasterFieldType.TextArea }
                }
            },

            // 3. Academic Year Master
            new MasterConfig
            {
                EntityType = "AcademicYear",
                SpName = "USP_AcademicYears_AY",
                TableName = "dbo.AcademicYears_AY",
                KeyColumn = "AY_Id",
                DisplayName = "Academic Year Master",
                SoftDelete = false,
                HasAuditColumns = true,
                MenuOrder = 3,
                Icon = "fa-calendar",

                ViewPermission = "Master.AcademicYear.View",
                CreatePermission = "Master.AcademicYear.Create",
                EditPermission = "Master.AcademicYear.Edit",
                DeletePermission = "Master.AcademicYear.Delete",

                Fields = new List<MasterFieldConfig>
                {
                    new MasterFieldConfig
                    {
                        ColumnName = "AY_Name",
                        PropertyName = "Name",
                        DisplayName = "Academic Year Name",
                        IsRequired = true,
                        IsUnique = true,
                        MaxLength = 100
                    },

                    new MasterFieldConfig
                    {
                        ColumnName = "AY_Code",
                        PropertyName = "Code",
                        DisplayName = "Code",
                        IsRequired = true,
                        IsUnique = true,
                        MaxLength = 50
                    },

                    new MasterFieldConfig
                    {
                        ColumnName = "AY_StartDate",
                        PropertyName = "StartDate",
                        DisplayName = "Start Date",
                        FieldType = MasterFieldType.Date,
                        IsRequired = true
                    },

                    new MasterFieldConfig
                    {
                        ColumnName = "AY_EndDate",
                        PropertyName = "EndDate",
                        DisplayName = "End Date",
                        FieldType = MasterFieldType.Date,
                        IsRequired = true,
                        DateRangeStartField = "AY_StartDate",
                        DateRangeEndField = "AY_EndDate"
                    },

                    new MasterFieldConfig
                    {
                        ColumnName = "AY_IsCurrent",
                        PropertyName = "IsCurrent",
                        DisplayName = "Is Current Year",
                        FieldType = MasterFieldType.Boolean
                    }
                }
            },

            // 4. Department Master
            new MasterConfig
            {
                EntityType = "Department",
                SpName = "USP_Departments_D",
                TableName = "dbo.Departments_D",
                KeyColumn = "D_Id",
                DisplayName = "Department Master",
                SoftDelete = false,
                HasAuditColumns = true,
                MenuOrder = 4,
                Icon = "fa-sitemap",

                ViewPermission = "Master.Department.View",
                CreatePermission = "Master.Department.Create",
                EditPermission = "Master.Department.Edit",
                DeletePermission = "Master.Department.Delete",

                Fields = new List<MasterFieldConfig>
                {
                    new MasterFieldConfig { ColumnName = "D_Name", PropertyName = "Name", DisplayName = "Department Name", IsRequired = true, IsUnique = true, MaxLength = 150 },
                    new MasterFieldConfig { ColumnName = "D_Code", PropertyName = "Code", DisplayName = "Department Code", IsRequired = true, IsUnique = true, MaxLength = 50 },
                    new MasterFieldConfig { ColumnName = "D_BranchId", PropertyName = "BranchId", DisplayName = "Branch", FieldType = MasterFieldType.Dropdown, LookupEntityType = "Branch", LookupValueField = "B_Id", LookupTextField = "B_Name" },
                    new MasterFieldConfig { ColumnName = "D_Description", PropertyName = "Description", DisplayName = "Description", FieldType = MasterFieldType.TextArea }
                }
            },

            // 5. Designation Master
            new MasterConfig
            {
                EntityType = "Designation",
                SpName = "USP_Designations_DS",
                TableName = "dbo.Designations_DS",
                KeyColumn = "DS_Id",
                DisplayName = "Designation Master",
                SoftDelete = false,
                HasAuditColumns = true,
                MenuOrder = 5,
                Icon = "fa-id-badge",

                ViewPermission = "Master.Designation.View",
                CreatePermission = "Master.Designation.Create",
                EditPermission = "Master.Designation.Edit",
                DeletePermission = "Master.Designation.Delete",

                Fields = new List<MasterFieldConfig>
                {
                    new MasterFieldConfig { ColumnName = "DS_Name", PropertyName = "Name", DisplayName = "Designation Name", IsRequired = true, IsUnique = true, MaxLength = 100 },
                    new MasterFieldConfig { ColumnName = "DS_Code", PropertyName = "Code", DisplayName = "Designation Code", IsRequired = true, IsUnique = true, MaxLength = 50 }
                }
            },

            // 6. Document Type Master
            new MasterConfig
            {
                EntityType = "DocumentType",
                SpName = "USP_DocumentTypes_DT",
                TableName = "dbo.DocumentTypes_DT",
                KeyColumn = "DT_Id",
                DisplayName = "Document Type Master",
                SoftDelete = false,
                HasAuditColumns = true,
                MenuOrder = 6,
                Icon = "fa-file-text",

                ViewPermission = "Master.DocumentType.View",
                CreatePermission = "Master.DocumentType.Create",
                EditPermission = "Master.DocumentType.Edit",
                DeletePermission = "Master.DocumentType.Delete",

                Fields = new List<MasterFieldConfig>
                {
                    new MasterFieldConfig
                    {
                        ColumnName = "DT_Name",
                        PropertyName = "Name",
                        DisplayName = "Document Type Name",
                        IsRequired = true,
                        IsUnique = true,
                        MaxLength = 100
                    },

                    new MasterFieldConfig
                    {
                        ColumnName = "DT_Code",
                        PropertyName = "Code",
                        DisplayName = "Code",
                        IsRequired = true,
                        IsUnique = true,
                        MaxLength = 50
                    },

                    new MasterFieldConfig
                    {
                        ColumnName = "DT_EntityType",
                        PropertyName = "EntityType",
                        DisplayName = "Entity Type",
                        IsRequired = true,
                        MaxLength = 30
                    },

                    new MasterFieldConfig
                    {
                        ColumnName = "DT_IsRequired",
                        PropertyName = "IsRequired",
                        DisplayName = "Mandatory Upload",
                        FieldType = MasterFieldType.Boolean
                    }
                }
            },

           // 7. Exam Type Master
            new MasterConfig
            {
                EntityType = "ExamType",
                SpName = "USP_ExamTypes_ET",
                TableName = "dbo.ExamTypes_ET",
                KeyColumn = "ET_Id",
                DisplayName = "Exam Type Master",
                SoftDelete = false,
                HasAuditColumns = true,
                MenuOrder = 7,
                Icon = "fa-pencil-square-o",

                ViewPermission = "Master.ExamType.View",
                CreatePermission = "Master.ExamType.Create",
                EditPermission = "Master.ExamType.Edit",
                DeletePermission = "Master.ExamType.Delete",

                Fields = new List<MasterFieldConfig>
                {
                    new MasterFieldConfig
                    {
                        ColumnName = "ET_Name",
                        PropertyName = "Name",
                        DisplayName = "Exam Type Name",
                        IsRequired = true,
                        IsUnique = true,
                        MaxLength = 100
                    },

                    new MasterFieldConfig
                    {
                        ColumnName = "ET_Code",
                        PropertyName = "Code",
                        DisplayName = "Exam Code",
                        IsRequired = true,
                        IsUnique = true,
                        MaxLength = 50
                    },

                    new MasterFieldConfig
                    {
                        ColumnName = "ET_WeightagePercentage",
                        PropertyName = "WeightagePercentage",
                        DisplayName = "Weightage %",
                        FieldType = MasterFieldType.Number,
                        IsRequired = true
                    }
                }
            },

            // 8. Expense Category Master
            new MasterConfig
            {
                EntityType = "ExpenseCategory",
                SpName = "USP_ExpenseCategories_EC",
                TableName = "dbo.ExpenseCategories_EC",
                KeyColumn = "EC_Id",
                DisplayName = "Expense Category Master",
                SoftDelete = false,
                HasAuditColumns = true,
                MenuOrder = 8,
                Icon = "fa-money",

                ViewPermission = "Master.ExpenseCategory.View",
                CreatePermission = "Master.ExpenseCategory.Create",
                EditPermission = "Master.ExpenseCategory.Edit",
                DeletePermission = "Master.ExpenseCategory.Delete",

                Fields = new List<MasterFieldConfig>
                {
                    new MasterFieldConfig { ColumnName = "EC_Name", PropertyName = "Name", DisplayName = "Category Name", IsRequired = true, IsUnique = true, MaxLength = 100 },
                    new MasterFieldConfig { ColumnName = "EC_Code", PropertyName = "Code", DisplayName = "Category Code", IsRequired = true, IsUnique = true, MaxLength = 50 },
                    new MasterFieldConfig { ColumnName = "EC_Description", PropertyName = "Description", DisplayName = "Description", FieldType = MasterFieldType.TextArea }
                }
            },

            // 9. Fee Category Master
            new MasterConfig
            {
                EntityType = "FeeCategory",
                SpName = "USP_FeeCategories_FC",
                TableName = "dbo.FeeCategories_FC",
                KeyColumn = "FC_Id",
                DisplayName = "Fee Category Master",
                SoftDelete = false,
                HasAuditColumns = true,
                MenuOrder = 9,
                Icon = "fa-credit-card",

                ViewPermission = "Master.FeeCategory.View",
                CreatePermission = "Master.FeeCategory.Create",
                EditPermission = "Master.FeeCategory.Edit",
                DeletePermission = "Master.FeeCategory.Delete",

                Fields = new List<MasterFieldConfig>
                {
                    new MasterFieldConfig { ColumnName = "FC_Name", PropertyName = "Name", DisplayName = "Category Name", IsRequired = true, IsUnique = true, MaxLength = 100 },
                    new MasterFieldConfig { ColumnName = "FC_Code", PropertyName = "Code", DisplayName = "Category Code", IsRequired = true, IsUnique = true, MaxLength = 50 },
                    new MasterFieldConfig { ColumnName = "FC_Description", PropertyName = "Description", DisplayName = "Description", FieldType = MasterFieldType.TextArea },
                    new MasterFieldConfig { ColumnName = "FC_IsRefundable", PropertyName = "IsRefundable", DisplayName = "Refundable", FieldType = MasterFieldType.Boolean }
                }
            },

            // 10. Grade Scale Master
            new MasterConfig
            {
                EntityType = "GradeScale",
                SpName = "USP_GradeScales_GS",
                TableName = "dbo.GradeScales_GS",
                KeyColumn = "GS_Id",
                DisplayName = "Grade Scale Master",
                SoftDelete = true,
                HasAuditColumns = true,
                MenuOrder = 10,
                Icon = "fa-graduation-cap",

                ViewPermission = "Master.GradeScale.View",
                CreatePermission = "Master.GradeScale.Create",
                EditPermission = "Master.GradeScale.Edit",
                DeletePermission = "Master.GradeScale.Delete",

                Fields = new List<MasterFieldConfig>
                {
                    new MasterFieldConfig { ColumnName = "GS_Name", PropertyName = "Name", DisplayName = "Grade Scale Name", IsRequired = true, IsUnique = true, MaxLength = 100 },
                    new MasterFieldConfig { ColumnName = "GS_Code", PropertyName = "Code", DisplayName = "Grade Code", IsRequired = true, IsUnique = true, MaxLength = 50 },
                    new MasterFieldConfig { ColumnName = "GS_Description", PropertyName = "Description", DisplayName = "Description", FieldType = MasterFieldType.TextArea },
                    new MasterFieldConfig { ColumnName = "GS_IsDefault", PropertyName = "IsDefault", DisplayName = "Default Scale", FieldType = MasterFieldType.Boolean }
                }
            },

            // 11. Classroom Master
            new MasterConfig
            {
                EntityType = "Classroom",
                SpName = "USP_Classrooms_CR",
                TableName = "dbo.Classrooms_CR",
                KeyColumn = "CR_Id",
                DisplayName = "Classroom Master",
                SoftDelete = false,
                HasAuditColumns = true,
                MenuOrder = 11,
                Icon = "fa-home",

                ViewPermission = "Master.Classroom.View",
                CreatePermission = "Master.Classroom.Create",
                EditPermission = "Master.Classroom.Edit",
                DeletePermission = "Master.Classroom.Delete",

                Fields = new List<MasterFieldConfig>
                {
                    new MasterFieldConfig
                    {
                        ColumnName = "CR_Name",
                        PropertyName = "Name",
                        DisplayName = "Room Name",
                        IsRequired = true,
                        MaxLength = 100
                    },

                    new MasterFieldConfig
                    {
                        ColumnName = "CR_Code",
                        PropertyName = "Code",
                        DisplayName = "Room Code",
                        IsRequired = true,
                        IsUnique = true,
                        MaxLength = 50
                    },

                    new MasterFieldConfig
                    {
                        ColumnName = "CR_BranchId",
                        PropertyName = "BranchId",
                        DisplayName = "Branch",
                        FieldType = MasterFieldType.Dropdown,
                        LookupEntityType = "Branch",
                        LookupValueField = "B_Id",
                        LookupTextField = "B_Name"
                    },

                    new MasterFieldConfig
                    {
                        ColumnName = "CR_Capacity",
                        PropertyName = "Capacity",
                        DisplayName = "Seating Capacity",
                        FieldType = MasterFieldType.Number,
                        IsRequired = true
                    },

                    new MasterFieldConfig
                    {
                        ColumnName = "CR_Location",
                        PropertyName = "Location",
                        DisplayName = "Location",
                        MaxLength = 255
                    }
                }
            },

            // 12. Payment Method Master
            new MasterConfig
            {
                EntityType = "PaymentMethod",
                SpName = "USP_PaymentMethods_PM",
                TableName = "dbo.PaymentMethods_PM",
                KeyColumn = "PM_Id",
                DisplayName = "Payment Method Master",
                SoftDelete = true,
                HasAuditColumns = true,
                MenuOrder = 12,
                Icon = "fa-credit-card-alt",

                ViewPermission = "Master.PaymentMethod.View",
                CreatePermission = "Master.PaymentMethod.Create",
                EditPermission = "Master.PaymentMethod.Edit",
                DeletePermission = "Master.PaymentMethod.Delete",

                Fields = new List<MasterFieldConfig>
                {
                    new MasterFieldConfig
                    {
                        ColumnName = "PM_Name",
                        PropertyName = "Name",
                        DisplayName = "Method Name",
                        IsRequired = true,
                        IsUnique = true,
                        MaxLength = 100
                    },

                    new MasterFieldConfig
                    {
                        ColumnName = "PM_Type",
                        PropertyName = "Type",
                        DisplayName = "Method Type",
                        IsRequired = true,
                        MaxLength = 30
                    }
                }
            },

            // 13. Discount Master
            new MasterConfig
            {
                EntityType = "Discount",
                SpName = "USP_Discounts_DIS",
                TableName = "dbo.Discounts_DIS",
                KeyColumn = "DIS_Id",
                DisplayName = "Discount Master",
                SoftDelete = true,
                HasAuditColumns = true,
                MenuOrder = 13,
                Icon = "fa-percent",

                ViewPermission = "Master.Discount.View",
                CreatePermission = "Master.Discount.Create",
                EditPermission = "Master.Discount.Edit",
                DeletePermission = "Master.Discount.Delete",

                Fields = new List<MasterFieldConfig>
                {
                    new MasterFieldConfig
                    {
                        ColumnName = "DIS_Name",
                        PropertyName = "Name",
                        DisplayName = "Discount Policy Name",
                        IsRequired = true,
                        IsUnique = true,
                        MaxLength = 100
                    },

                    new MasterFieldConfig
                    {
                        ColumnName = "DIS_Code",
                        PropertyName = "Code",
                        DisplayName = "Code",
                        IsRequired = true,
                        IsUnique = true,
                        MaxLength = 50
                    },

                    new MasterFieldConfig
                    {
                        ColumnName = "DIS_DiscountType",
                        PropertyName = "DiscountType",
                        DisplayName = "Type",
                        IsRequired = true,
                        MaxLength = 20
                    },

                    new MasterFieldConfig
                    {
                        ColumnName = "DIS_Value",
                        PropertyName = "Value",
                        DisplayName = "Discount Value",
                        FieldType = MasterFieldType.Number,
                        IsRequired = true
                    },

                    new MasterFieldConfig
                    {
                        ColumnName = "DIS_Description",
                        PropertyName = "Description",
                        DisplayName = "Description",
                        FieldType = MasterFieldType.TextArea
                    }
                }
            },

            // 14. Subject Master
            new MasterConfig
            {
                EntityType = "Subject",
                SpName = "USP_Subjects_SB",
                TableName = "dbo.Subjects_SB",
                KeyColumn = "SB_Id",
                DisplayName = "Subject Master",
                SoftDelete = false,
                HasAuditColumns = true,
                MenuOrder = 14,
                Icon = "fa-book",

                ViewPermission = "Master.Subject.View",
                CreatePermission = "Master.Subject.Create",
                EditPermission = "Master.Subject.Edit",
                DeletePermission = "Master.Subject.Delete",

                Fields = new List<MasterFieldConfig>
                {
                    new MasterFieldConfig { ColumnName = "SB_Name", PropertyName = "Name", DisplayName = "Subject Name", IsRequired = true, IsUnique = true, MaxLength = 200 },
                    new MasterFieldConfig { ColumnName = "SB_Code", PropertyName = "Code", DisplayName = "Subject Code", IsRequired = true, IsUnique = true, MaxLength = 50 },
                    new MasterFieldConfig { ColumnName = "SB_Credits", PropertyName = "Credits", DisplayName = "Credits", FieldType = MasterFieldType.Number },
                    new MasterFieldConfig { ColumnName = "SB_MaxMarks", PropertyName = "MaxMarks", DisplayName = "Maximum Marks", FieldType = MasterFieldType.Number },
                    new MasterFieldConfig { ColumnName = "SB_PassMarks", PropertyName = "PassMarks", DisplayName = "Pass Marks", FieldType = MasterFieldType.Number },
                    new MasterFieldConfig { ColumnName = "SB_Description", PropertyName = "Description", DisplayName = "Description", FieldType = MasterFieldType.TextArea }
                }
            },

            // 15. Program Master
            new MasterConfig
            {
                EntityType = "Program",
                SpName = "USP_Programs_P",
                TableName = "dbo.Programs_P",
                KeyColumn = "P_Id",
                DisplayName = "Program Master",
                SoftDelete = true,
                HasAuditColumns = true,
                MenuOrder = 15,
                Icon = "fa-graduation-cap",

                ViewPermission = "Master.Program.View",
                CreatePermission = "Master.Program.Create",
                EditPermission = "Master.Program.Edit",
                DeletePermission = "Master.Program.Delete",

                Fields = new List<MasterFieldConfig>
                {
                    new MasterFieldConfig { ColumnName = "P_Name", PropertyName = "Name", DisplayName = "Program Name", IsRequired = true, IsUnique = true, MaxLength = 200 },
                    new MasterFieldConfig { ColumnName = "P_Code", PropertyName = "Code", DisplayName = "Program Code", IsRequired = true, IsUnique = true, MaxLength = 50 },
                    new MasterFieldConfig { ColumnName = "P_DurationValue", PropertyName = "DurationValue", DisplayName = "Duration Value", FieldType = MasterFieldType.Number },
                    new MasterFieldConfig { ColumnName = "P_DurationUnit", PropertyName = "DurationUnit", DisplayName = "Duration Unit (e.g. Years, Months)", MaxLength = 20 },
                    new MasterFieldConfig { ColumnName = "P_Description", PropertyName = "Description", DisplayName = "Description", FieldType = MasterFieldType.TextArea }
                }
            },

            // 16. Vendor Master
            new MasterConfig
            {
                EntityType = "Vendor",
                SpName = "USP_Vendors_V",
                TableName = "dbo.Vendors_V",
                KeyColumn = "V_Id",
                DisplayName = "Vendor Master",
                SoftDelete = false,
                HasAuditColumns = true,
                MenuOrder = 16,
                Icon = "fa-truck",

                ViewPermission = "Master.Vendor.View",
                CreatePermission = "Master.Vendor.Create",
                EditPermission = "Master.Vendor.Edit",
                DeletePermission = "Master.Vendor.Delete",

                Fields = new List<MasterFieldConfig>
                {
                    new MasterFieldConfig { ColumnName = "V_Name", PropertyName = "Name", DisplayName = "Vendor Name", IsRequired = true, IsUnique = true, MaxLength = 200 },
                    new MasterFieldConfig { ColumnName = "V_Code", PropertyName = "Code", DisplayName = "Vendor Code", IsRequired = true, IsUnique = true, MaxLength = 50 },
                    new MasterFieldConfig { ColumnName = "V_Email", PropertyName = "Email", DisplayName = "Email", MaxLength = 255 },
                    new MasterFieldConfig { ColumnName = "V_Phone", PropertyName = "Phone", DisplayName = "Phone", MaxLength = 30 },
                    new MasterFieldConfig { ColumnName = "V_TaxNumber", PropertyName = "TaxNumber", DisplayName = "GSTIN / Tax Number", MaxLength = 100 },
                    new MasterFieldConfig { ColumnName = "V_Address", PropertyName = "Address", DisplayName = "Address", FieldType = MasterFieldType.TextArea }
                }
            },

            // 17. Student (lookup only — full CRUD via StudentsController)
            new MasterConfig
            {
                EntityType = "Student",
                SpName = "USP_Students_S",
                TableName = "dbo.Students_S",
                KeyColumn = "S_Id",
                DisplayName = "Student",
                SoftDelete = false,
                HasAuditColumns = true,
                MenuOrder = 20,
                Icon = "fa-user-graduate",

                Fields = new List<MasterFieldConfig>
                {
                    new MasterFieldConfig { ColumnName = "S_FirstName", PropertyName = "FirstName", DisplayName = "First Name", IsRequired = true, MaxLength = 100 },
                    new MasterFieldConfig { ColumnName = "S_LastName", PropertyName = "LastName", DisplayName = "Last Name", IsRequired = true, MaxLength = 100 },
                    new MasterFieldConfig { ColumnName = "S_StudentCode", PropertyName = "StudentCode", DisplayName = "Student Code", IsRequired = true, MaxLength = 50 }
                }
            },

            // 18. Batch (lookup only — full CRUD via BatchController)
            new MasterConfig
            {
                EntityType = "Batch",
                SpName = "USP_Batches_BT",
                TableName = "dbo.Batches_BT",
                KeyColumn = "BT_Id",
                DisplayName = "Batch",
                SoftDelete = false,
                HasAuditColumns = true,
                MenuOrder = 21,
                Icon = "fa-users",

                Fields = new List<MasterFieldConfig>
                {
                    new MasterFieldConfig { ColumnName = "BT_Name", PropertyName = "Name", DisplayName = "Batch Name", IsRequired = true, MaxLength = 150 },
                    new MasterFieldConfig { ColumnName = "BT_Code", PropertyName = "Code", DisplayName = "Batch Code", IsRequired = true, MaxLength = 50 }
                }
            },

            // 19. Staff (lookup only — full CRUD via dedicated controller)
            new MasterConfig
            {
                EntityType = "Staff",
                SpName = "USP_Staff_ST",
                TableName = "dbo.Staff_ST",
                KeyColumn = "ST_Id",
                DisplayName = "Staff",
                SoftDelete = false,
                HasAuditColumns = true,
                MenuOrder = 22,
                Icon = "fa-chalkboard-user",

                Fields = new List<MasterFieldConfig>
                {
                    new MasterFieldConfig { ColumnName = "ST_FirstName", PropertyName = "FirstName", DisplayName = "First Name", IsRequired = true, MaxLength = 100 },
                    new MasterFieldConfig { ColumnName = "ST_LastName", PropertyName = "LastName", DisplayName = "Last Name", IsRequired = true, MaxLength = 100 },
                    new MasterFieldConfig { ColumnName = "ST_EmployeeCode", PropertyName = "EmployeeCode", DisplayName = "Employee Code", IsRequired = true, MaxLength = 50 }
                }
            },

            // 20. Notification Template Master
            new MasterConfig
            {
                EntityType = "NotificationTemplate",
                SpName = "USP_NotificationTemplates_NT",
                TableName = "dbo.NotificationTemplates_NT",
                KeyColumn = "NT_Id",
                DisplayName = "Notification Template Master",
                SoftDelete = true,
                HasAuditColumns = true,
                MenuOrder = 17,
                Icon = "fa-bell",

                ViewPermission = "Master.NotificationTemplate.View",
                CreatePermission = "Master.NotificationTemplate.Create",
                EditPermission = "Master.NotificationTemplate.Edit",
                DeletePermission = "Master.NotificationTemplate.Delete",

                Fields = new List<MasterFieldConfig>
                {
                    new MasterFieldConfig
                    {
                        ColumnName = "NT_Name",
                        PropertyName = "Name",
                        DisplayName = "Template Name",
                        IsRequired = true,
                        IsUnique = true,
                        MaxLength = 150
                    },

                    new MasterFieldConfig
                    {
                        ColumnName = "NT_EventKey",
                        PropertyName = "EventKey",
                        DisplayName = "Event Key",
                        IsRequired = true,
                        IsUnique = true,
                        MaxLength = 100
                    },

                    new MasterFieldConfig
                    {
                        ColumnName = "NT_Channel",
                        PropertyName = "Channel",
                        DisplayName = "Channel",
                        IsRequired = true,
                        MaxLength = 20
                    },

                    new MasterFieldConfig
                    {
                        ColumnName = "NT_Subject",
                        PropertyName = "Subject",
                        DisplayName = "Email Subject Line",
                        MaxLength = 255
                    },

                    new MasterFieldConfig
                    {
                        ColumnName = "NT_BodyTemplate",
                        PropertyName = "BodyTemplate",
                        DisplayName = "Template Content",
                        FieldType = MasterFieldType.TextArea,
                        IsRequired = true
                    }
                }
            } 
        };
        public static List<MasterConfig> GetAll() => _configs;

        public static MasterConfig GetByEntityType(string entityType)
        {
            return _configs.FirstOrDefault(c => c.EntityType.Equals(entityType, StringComparison.OrdinalIgnoreCase));
        }
    }
}
