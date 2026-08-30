using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IMS.Models.ViewModels
{
    public class ExpenseListItemViewModel
    {
        public Guid EXP_Id { get; set; }
        public string EXP_ExpenseNumber { get; set; }
        public DateTime EXP_ExpenseDate { get; set; }
        public string BranchName { get; set; }
        public string ExpenseCategoryName { get; set; }
        public string VendorName { get; set; }
        public decimal EXP_Amount { get; set; }
        public string EXP_Description { get; set; }
        public string PaymentMethodName { get; set; }
    }

    public class ExpenseIndexViewModel
    {
        public List<ExpenseListItemViewModel> Expenses { get; set; } = new();
        public string SearchTerm { get; set; }
        public Guid? BranchFilter { get; set; }
        public Guid? ExpenseCategoryFilter { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public List<SelectListItem> BranchOptions { get; set; } = new();
        public List<SelectListItem> ExpenseCategoryOptions { get; set; } = new();
    }

    public class ExpenseFormViewModel
    {
        public Guid? EXP_Id { get; set; }
        public Guid EXP_BranchId { get; set; }
        public Guid EXP_ExpenseCategoryId { get; set; }
        public Guid? EXP_VendorId { get; set; }
        public string EXP_ExpenseNumber { get; set; }
        public DateTime EXP_ExpenseDate { get; set; } = DateTime.Today;
        public decimal EXP_Amount { get; set; }
        public string EXP_Description { get; set; }
        public Guid? EXP_PaymentMethodId { get; set; }

        public List<SelectListItem> BranchOptions { get; set; } = new();
        public List<SelectListItem> ExpenseCategoryOptions { get; set; } = new();
        public List<SelectListItem> VendorOptions { get; set; } = new();
        public List<SelectListItem> PaymentMethodOptions { get; set; } = new();
    }

    public class ExpenseDetailsViewModel
    {
        public Guid EXP_Id { get; set; }
        public string EXP_ExpenseNumber { get; set; }
        public DateTime EXP_ExpenseDate { get; set; }
        public string BranchName { get; set; }
        public string ExpenseCategoryName { get; set; }
        public string VendorName { get; set; }
        public decimal EXP_Amount { get; set; }
        public string EXP_Description { get; set; }
        public string PaymentMethodName { get; set; }
        public DateTime EXP_CreatedAt { get; set; }
        public DateTime EXP_UpdatedAt { get; set; }
    }
}
