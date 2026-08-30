using System;

namespace IMS.Models.Entities
{
    public class Expense
    {
        public Guid EXP_Id { get; set; }
        public Guid EXP_TenantId { get; set; }
        public Guid EXP_BranchId { get; set; }
        public Guid EXP_ExpenseCategoryId { get; set; }
        public Guid? EXP_VendorId { get; set; }
        public string EXP_ExpenseNumber { get; set; }
        public DateTime EXP_ExpenseDate { get; set; }
        public decimal EXP_Amount { get; set; }
        public string EXP_Description { get; set; }
        public Guid? EXP_PaymentMethodId { get; set; }
        public Guid EXP_CreatedBy { get; set; }
        public DateTime EXP_CreatedAt { get; set; }
        public DateTime EXP_UpdatedAt { get; set; }

        public string BranchName { get; set; }
        public string ExpenseCategoryName { get; set; }
        public string VendorName { get; set; }
        public string PaymentMethodName { get; set; }
    }

    public class ExpenseCategory
    {
        public Guid EC_Id { get; set; }
        public Guid EC_TenantId { get; set; }
        public string EC_Name { get; set; }
        public string EC_Code { get; set; }
        public string EC_Description { get; set; }
        public DateTime EC_CreatedAt { get; set; }
        public DateTime EC_UpdatedAt { get; set; }
    }
}
