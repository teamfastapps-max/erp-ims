using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IMS.Models.ViewModels
{
    public class FeeStructureListItemViewModel
    {
        public Guid FS_Id { get; set; }
        public string FS_Code { get; set; }
        public string FS_Name { get; set; }
        public string CourseName { get; set; }
        public string BatchName { get; set; }
        public string AcademicYearName { get; set; }
        public bool FS_IsActive { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class FeeStructureIndexViewModel
    {
        public List<FeeStructureListItemViewModel> FeeStructures { get; set; } = new();
        public string SearchTerm { get; set; }
        public Guid? AcademicYearFilter { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public List<SelectListItem> AcademicYearOptions { get; set; } = new();
    }

    public class FeeStructureFormViewModel
    {
        public Guid? FS_Id { get; set; }
        public string FS_Name { get; set; }
        public string FS_Code { get; set; }
        public Guid? FS_CourseId { get; set; }
        public Guid? FS_BatchId { get; set; }
        public Guid FS_AcademicYearId { get; set; }
        public string FS_Description { get; set; }
        public bool FS_IsActive { get; set; } = true;

        public List<FeeStructureItemFormViewModel> Items { get; set; } = new();

        public List<SelectListItem> CourseOptions { get; set; } = new();
        public List<SelectListItem> BatchOptions { get; set; } = new();
        public List<SelectListItem> AcademicYearOptions { get; set; } = new();
        public List<SelectListItem> FeeCategoryOptions { get; set; } = new();
    }

    public class FeeStructureItemFormViewModel
    {
        public Guid? FSI_Id { get; set; }
        public Guid FSI_FeeCategoryId { get; set; }
        public decimal FSI_Amount { get; set; }
        public int? FSI_DueDays { get; set; }
        public bool FSI_IsMandatory { get; set; } = true;
    }

    public class FeeInvoiceListItemViewModel
    {
        public Guid FI_Id { get; set; }
        public string FI_InvoiceNumber { get; set; }
        public string StudentCode { get; set; }
        public string StudentName { get; set; }
        public DateTime FI_InvoiceDate { get; set; }
        public DateTime FI_DueDate { get; set; }
        public decimal FI_TotalAmount { get; set; }
        public decimal FI_PaidAmount { get; set; }
        public decimal FI_BalanceAmount { get; set; }
        public string FI_Status { get; set; }
    }

    public class FeeInvoiceIndexViewModel
    {
        public List<FeeInvoiceListItemViewModel> Invoices { get; set; } = new();
        public string SearchTerm { get; set; }
        public string StatusFilter { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public List<SelectListItem> StatusOptions { get; set; } = new();
    }

    public class FeeInvoiceFormViewModel
    {
        public Guid? FI_Id { get; set; }
        public Guid FI_StudentId { get; set; }
        public DateTime FI_InvoiceDate { get; set; } = DateTime.Today;
        public DateTime FI_DueDate { get; set; } = DateTime.Today.AddDays(30);
        public decimal FI_DiscountAmount { get; set; }
        public decimal FI_TaxAmount { get; set; }
        public string FI_Notes { get; set; }

        public List<FeeInvoiceItemFormViewModel> Items { get; set; } = new();

        public List<SelectListItem> StudentOptions { get; set; } = new();
        public List<SelectListItem> FeeCategoryOptions { get; set; } = new();
    }

    public class FeeInvoiceItemFormViewModel
    {
        public Guid? FII_Id { get; set; }
        public Guid FII_FeeCategoryId { get; set; }
        public string FII_Description { get; set; }
        public decimal FII_Quantity { get; set; } = 1;
        public decimal FII_UnitAmount { get; set; }
        public decimal FII_DiscountAmount { get; set; }
        public decimal FII_TaxAmount { get; set; }
    }

    public class PaymentListItemViewModel
    {
        public Guid PAY_Id { get; set; }
        public string PAY_PaymentNumber { get; set; }
        public string StudentCode { get; set; }
        public string StudentName { get; set; }
        public DateTime PAY_PaymentDate { get; set; }
        public decimal PAY_Amount { get; set; }
        public string PaymentMethodName { get; set; }
        public string PAY_Status { get; set; }
    }

    public class PaymentIndexViewModel
    {
        public List<PaymentListItemViewModel> Payments { get; set; } = new();
        public string SearchTerm { get; set; }
        public string StatusFilter { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public List<SelectListItem> StatusOptions { get; set; } = new();
    }

    public class PaymentFormViewModel
    {
        public Guid? PAY_Id { get; set; }
        public Guid PAY_StudentId { get; set; }
        public DateTime PAY_PaymentDate { get; set; } = DateTime.Today;
        public decimal PAY_Amount { get; set; }
        public Guid PAY_PaymentMethodId { get; set; }
        public string PAY_TransactionReference { get; set; }
        public string PAY_Notes { get; set; }

        public List<SelectListItem> StudentOptions { get; set; } = new();
        public List<SelectListItem> PaymentMethodOptions { get; set; } = new();
    }
}
