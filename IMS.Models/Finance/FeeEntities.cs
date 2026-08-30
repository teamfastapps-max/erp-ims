using System;

namespace IMS.Models.Entities
{
    public class FeeStructure
    {
        public Guid FS_Id { get; set; }
        public Guid FS_TenantId { get; set; }
        public string FS_Name { get; set; }
        public string FS_Code { get; set; }
        public Guid? FS_CourseId { get; set; }
        public Guid? FS_BatchId { get; set; }
        public Guid FS_AcademicYearId { get; set; }
        public string FS_Description { get; set; }
        public bool FS_IsActive { get; set; }
        public DateTime FS_CreatedAt { get; set; }
        public DateTime FS_UpdatedAt { get; set; }

        public string CourseName { get; set; }
        public string BatchName { get; set; }
        public string AcademicYearName { get; set; }
    }

    public class FeeStructureItem
    {
        public Guid FSI_Id { get; set; }
        public Guid FSI_FeeStructureId { get; set; }
        public Guid FSI_FeeCategoryId { get; set; }
        public decimal FSI_Amount { get; set; }
        public int? FSI_DueDays { get; set; }
        public bool FSI_IsMandatory { get; set; }

        public string FeeCategoryName { get; set; }
    }

    public class FeeInvoice
    {
        public Guid FI_Id { get; set; }
        public Guid FI_TenantId { get; set; }
        public Guid FI_StudentId { get; set; }
        public string FI_InvoiceNumber { get; set; }
        public DateTime FI_InvoiceDate { get; set; }
        public DateTime FI_DueDate { get; set; }
        public decimal FI_Subtotal { get; set; }
        public decimal FI_DiscountAmount { get; set; }
        public decimal FI_TaxAmount { get; set; }
        public decimal FI_TotalAmount { get; set; }
        public decimal FI_PaidAmount { get; set; }
        public decimal FI_BalanceAmount { get; set; }
        public string FI_Status { get; set; }
        public string FI_Notes { get; set; }
        public DateTime FI_CreatedAt { get; set; }
        public DateTime FI_UpdatedAt { get; set; }

        public string StudentName { get; set; }
        public string StudentCode { get; set; }
    }

    public class Payment
    {
        public Guid PAY_Id { get; set; }
        public Guid PAY_TenantId { get; set; }
        public Guid PAY_StudentId { get; set; }
        public string PAY_PaymentNumber { get; set; }
        public DateTime PAY_PaymentDate { get; set; }
        public decimal PAY_Amount { get; set; }
        public Guid PAY_PaymentMethodId { get; set; }
        public string PAY_Status { get; set; }
        public string PAY_TransactionReference { get; set; }
        public string PAY_Notes { get; set; }
        public Guid PAY_CreatedBy { get; set; }
        public DateTime PAY_CreatedAt { get; set; }
        public DateTime PAY_UpdatedAt { get; set; }

        public string StudentName { get; set; }
        public string StudentCode { get; set; }
        public string PaymentMethodName { get; set; }
    }

    public class FeeCategory
    {
        public Guid FC_Id { get; set; }
        public Guid FC_TenantId { get; set; }
        public string FC_Name { get; set; }
        public string FC_Code { get; set; }
        public string FC_Description { get; set; }
        public bool FC_IsRefundable { get; set; }
        public DateTime FC_CreatedAt { get; set; }
        public DateTime FC_UpdatedAt { get; set; }
    }
}
