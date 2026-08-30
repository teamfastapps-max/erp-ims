using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IMS.DAL.Interfaces;
using IMS.Helpers.Constants;
using IMS.Models.Entities;
using IMS.Models.ViewModels;
using IMS.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IMS.Services
{
    public class FeeService : IFeeService
    {
        private readonly IFeeStructureDAL _fsDAL;
        private readonly IFeeStructureItemDAL _fsiDAL;
        private readonly IFeeInvoiceDAL _fiDAL;
        private readonly IPaymentDAL _payDAL;
        private readonly IMasterService _masterService;

        public FeeService(IFeeStructureDAL fsDAL, IFeeStructureItemDAL fsiDAL,
            IFeeInvoiceDAL fiDAL, IPaymentDAL payDAL, IMasterService masterService)
        {
            _fsDAL = fsDAL;
            _fsiDAL = fsiDAL;
            _fiDAL = fiDAL;
            _payDAL = payDAL;
            _masterService = masterService;
        }

        // ==================== Fee Structures ====================
        public async Task<FeeStructureIndexViewModel> GetFeeStructureListAsync(Guid tenantId, string searchTerm, Guid? academicYearId, int pageNumber, int pageSize)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            var (items, totalCount) = await _fsDAL.GetPagedAsync(tenantId, searchTerm, academicYearId, pageNumber, pageSize);
            return new FeeStructureIndexViewModel
            {
                FeeStructures = items.Select(f => new FeeStructureListItemViewModel
                {
                    FS_Id = f.FS_Id, FS_Code = f.FS_Code, FS_Name = f.FS_Name,
                    CourseName = f.CourseName ?? "-", BatchName = f.BatchName ?? "-",
                    AcademicYearName = f.AcademicYearName ?? "-", FS_IsActive = f.FS_IsActive
                }).ToList(),
                SearchTerm = searchTerm, AcademicYearFilter = academicYearId,
                PageNumber = pageNumber, PageSize = pageSize, TotalCount = totalCount,
                AcademicYearOptions = GetMasterSelectList("AcademicYear", academicYearId?.ToString())
            };
        }

        public async Task<FeeStructureFormViewModel> GetFeeStructureForEditAsync(Guid id, Guid tenantId)
        {
            var fs = await _fsDAL.GetByIdAsync(id, tenantId);
            if (fs == null) return null;
            var items = await _fsiDAL.GetByFeeStructureIdAsync(id);
            var vm = new FeeStructureFormViewModel
            {
                FS_Id = fs.FS_Id, FS_Name = fs.FS_Name, FS_Code = fs.FS_Code,
                FS_CourseId = fs.FS_CourseId, FS_BatchId = fs.FS_BatchId,
                FS_AcademicYearId = fs.FS_AcademicYearId, FS_Description = fs.FS_Description,
                FS_IsActive = fs.FS_IsActive,
                Items = items.Select(i => new FeeStructureItemFormViewModel
                {
                    FSI_Id = i.FSI_Id, FSI_FeeCategoryId = i.FSI_FeeCategoryId,
                    FSI_Amount = i.FSI_Amount, FSI_DueDays = i.FSI_DueDays, FSI_IsMandatory = i.FSI_IsMandatory
                }).ToList()
            };
            PopulateFeeStructureDropdowns(vm, tenantId);
            return vm;
        }

        public async Task<ServiceResult> CreateFeeStructureAsync(FeeStructureFormViewModel model, Guid tenantId)
        {
            if (!string.IsNullOrWhiteSpace(model.FS_Code) && await _fsDAL.IsCodeTakenAsync(tenantId, model.FS_Code, null))
                return ServiceResult.Fail("This fee structure code is already in use.");
            var entity = new FeeStructure { FS_Id = Guid.NewGuid(), FS_TenantId = tenantId, FS_Name = model.FS_Name, FS_Code = model.FS_Code, FS_CourseId = model.FS_CourseId, FS_BatchId = model.FS_BatchId, FS_AcademicYearId = model.FS_AcademicYearId, FS_Description = model.FS_Description, FS_IsActive = model.FS_IsActive };
            var id = await _fsDAL.CreateAsync(entity);
            var fsiItems = model.Items.Select(i => new FeeStructureItem { FSI_Id = Guid.NewGuid(), FSI_FeeStructureId = id, FSI_FeeCategoryId = i.FSI_FeeCategoryId, FSI_Amount = i.FSI_Amount, FSI_DueDays = i.FSI_DueDays, FSI_IsMandatory = i.FSI_IsMandatory }).ToList();
            await _fsiDAL.SaveItemsAsync(id, fsiItems);
            return ServiceResult.Ok("Fee structure created successfully.", id);
        }

        public async Task<ServiceResult> UpdateFeeStructureAsync(FeeStructureFormViewModel model, Guid tenantId)
        {
            if (!model.FS_Id.HasValue) return ServiceResult.Fail("Fee Structure Id is required.");
            if (await _fsDAL.IsCodeTakenAsync(tenantId, model.FS_Code, model.FS_Id)) return ServiceResult.Fail("This code is already in use.");
            var entity = new FeeStructure { FS_Id = model.FS_Id.Value, FS_TenantId = tenantId, FS_Name = model.FS_Name, FS_Code = model.FS_Code, FS_CourseId = model.FS_CourseId, FS_BatchId = model.FS_BatchId, FS_AcademicYearId = model.FS_AcademicYearId, FS_Description = model.FS_Description, FS_IsActive = model.FS_IsActive };
            var success = await _fsDAL.UpdateAsync(entity);
            var fsiItems = model.Items.Select(i => new FeeStructureItem { FSI_Id = i.FSI_Id ?? Guid.NewGuid(), FSI_FeeStructureId = model.FS_Id.Value, FSI_FeeCategoryId = i.FSI_FeeCategoryId, FSI_Amount = i.FSI_Amount, FSI_DueDays = i.FSI_DueDays, FSI_IsMandatory = i.FSI_IsMandatory }).ToList();
            await _fsiDAL.SaveItemsAsync(model.FS_Id.Value, fsiItems);
            return success ? ServiceResult.Ok("Fee structure updated successfully.") : ServiceResult.Fail("Fee structure not found.");
        }

        public async Task<ServiceResult> DeleteFeeStructureAsync(Guid id, Guid tenantId)
        {
            return await _fsDAL.DeleteAsync(id, tenantId) ? ServiceResult.Ok("Fee structure deleted successfully.") : ServiceResult.Fail("Unable to delete fee structure.");
        }

        public void PopulateFeeStructureDropdowns(FeeStructureFormViewModel vm, Guid tenantId)
        {
            vm.CourseOptions = GetMasterSelectList("Course", vm.FS_CourseId?.ToString());
            vm.BatchOptions = GetMasterSelectList("Batch", vm.FS_BatchId?.ToString());
            vm.AcademicYearOptions = GetMasterSelectList("AcademicYear", vm.FS_AcademicYearId.ToString());
            vm.FeeCategoryOptions = GetMasterSelectList("FeeCategory", null);
        }

        // ==================== Fee Invoices ====================
        public async Task<FeeInvoiceIndexViewModel> GetFeeInvoiceListAsync(Guid tenantId, string searchTerm, string status, int pageNumber, int pageSize)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            var (items, totalCount) = await _fiDAL.GetPagedAsync(tenantId, searchTerm, status, pageNumber, pageSize);
            return new FeeInvoiceIndexViewModel
            {
                Invoices = items.Select(i => new FeeInvoiceListItemViewModel
                {
                    FI_Id = i.FI_Id, FI_InvoiceNumber = i.FI_InvoiceNumber,
                    StudentCode = i.StudentCode, StudentName = i.StudentName,
                    FI_InvoiceDate = i.FI_InvoiceDate, FI_DueDate = i.FI_DueDate,
                    FI_TotalAmount = i.FI_TotalAmount, FI_PaidAmount = i.FI_PaidAmount,
                    FI_BalanceAmount = i.FI_BalanceAmount, FI_Status = i.FI_Status
                }).ToList(),
                SearchTerm = searchTerm, StatusFilter = status,
                PageNumber = pageNumber, PageSize = pageSize, TotalCount = totalCount,
                StatusOptions = GetInvoiceStatusSelectList(status)
            };
        }

        public async Task<FeeInvoiceFormViewModel> GetFeeInvoiceForEditAsync(Guid id, Guid tenantId)
        {
            var fi = await _fiDAL.GetByIdAsync(id, tenantId);
            if (fi == null) return null;
            var vm = new FeeInvoiceFormViewModel
            {
                FI_Id = fi.FI_Id, FI_StudentId = fi.FI_StudentId,
                FI_InvoiceDate = fi.FI_InvoiceDate, FI_DueDate = fi.FI_DueDate,
                FI_DiscountAmount = fi.FI_DiscountAmount, FI_TaxAmount = fi.FI_TaxAmount,
                FI_Notes = fi.FI_Notes
            };
            PopulateFeeInvoiceDropdowns(vm, tenantId);
            return vm;
        }

        public async Task<ServiceResult> CreateFeeInvoiceAsync(FeeInvoiceFormViewModel model, Guid tenantId)
        {
            var invoiceNumber = await _fiDAL.GetNextInvoiceNumberAsync(tenantId);
            var subtotal = model.Items.Sum(i => i.FII_Quantity * i.FII_UnitAmount);
            var totalDiscount = model.Items.Sum(i => i.FII_DiscountAmount) + model.FI_DiscountAmount;
            var totalTax = model.Items.Sum(i => i.FII_TaxAmount) + model.FI_TaxAmount;
            var total = subtotal - totalDiscount + totalTax;

            var entity = new FeeInvoice
            {
                FI_Id = Guid.NewGuid(), FI_TenantId = tenantId, FI_StudentId = model.FI_StudentId,
                FI_InvoiceNumber = invoiceNumber, FI_InvoiceDate = model.FI_InvoiceDate,
                FI_DueDate = model.FI_DueDate, FI_Subtotal = subtotal, FI_DiscountAmount = totalDiscount,
                FI_TaxAmount = totalTax, FI_TotalAmount = total, FI_PaidAmount = 0,
                FI_BalanceAmount = total, FI_Status = "Pending", FI_Notes = model.FI_Notes
            };
            var id = await _fiDAL.CreateAsync(entity);
            return ServiceResult.Ok("Invoice created successfully.", id);
        }

        public async Task<ServiceResult> DeleteFeeInvoiceAsync(Guid id, Guid tenantId)
        {
            return await _fiDAL.DeleteAsync(id, tenantId) ? ServiceResult.Ok("Invoice deleted successfully.") : ServiceResult.Fail("Unable to delete invoice.");
        }

        public void PopulateFeeInvoiceDropdowns(FeeInvoiceFormViewModel vm, Guid tenantId)
        {
            vm.StudentOptions = GetMasterSelectList("Student", vm.FI_StudentId.ToString());
            vm.FeeCategoryOptions = GetMasterSelectList("FeeCategory", null);
        }

        // ==================== Payments ====================
        public async Task<PaymentIndexViewModel> GetPaymentListAsync(Guid tenantId, string searchTerm, string status, int pageNumber, int pageSize)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            var (items, totalCount) = await _payDAL.GetPagedAsync(tenantId, searchTerm, status, pageNumber, pageSize);
            return new PaymentIndexViewModel
            {
                Payments = items.Select(p => new PaymentListItemViewModel
                {
                    PAY_Id = p.PAY_Id, PAY_PaymentNumber = p.PAY_PaymentNumber,
                    StudentCode = p.StudentCode, StudentName = p.StudentName,
                    PAY_PaymentDate = p.PAY_PaymentDate, PAY_Amount = p.PAY_Amount,
                    PaymentMethodName = p.PaymentMethodName ?? "-", PAY_Status = p.PAY_Status
                }).ToList(),
                SearchTerm = searchTerm, StatusFilter = status,
                PageNumber = pageNumber, PageSize = pageSize, TotalCount = totalCount,
                StatusOptions = GetPaymentStatusSelectList(status)
            };
        }

        public async Task<PaymentFormViewModel> GetPaymentForEditAsync(Guid id, Guid tenantId)
        {
            var p = await _payDAL.GetByIdAsync(id, tenantId);
            if (p == null) return null;
            var vm = new PaymentFormViewModel
            {
                PAY_Id = p.PAY_Id, PAY_StudentId = p.PAY_StudentId,
                PAY_PaymentDate = p.PAY_PaymentDate, PAY_Amount = p.PAY_Amount,
                PAY_PaymentMethodId = p.PAY_PaymentMethodId,
                PAY_TransactionReference = p.PAY_TransactionReference, PAY_Notes = p.PAY_Notes
            };
            PopulatePaymentDropdowns(vm, tenantId);
            return vm;
        }

        public async Task<ServiceResult> CreatePaymentAsync(PaymentFormViewModel model, Guid tenantId, Guid createdBy)
        {
            var paymentNumber = await _payDAL.GetNextPaymentNumberAsync(tenantId);
            var entity = new Payment
            {
                PAY_Id = Guid.NewGuid(), PAY_TenantId = tenantId, PAY_StudentId = model.PAY_StudentId,
                PAY_PaymentNumber = paymentNumber, PAY_PaymentDate = model.PAY_PaymentDate,
                PAY_Amount = model.PAY_Amount, PAY_PaymentMethodId = model.PAY_PaymentMethodId,
                PAY_Status = "Completed", PAY_TransactionReference = model.PAY_TransactionReference,
                PAY_Notes = model.PAY_Notes, PAY_CreatedBy = createdBy
            };
            var id = await _payDAL.CreateAsync(entity);
            return ServiceResult.Ok("Payment recorded successfully.", id);
        }

        public async Task<ServiceResult> DeletePaymentAsync(Guid id, Guid tenantId)
        {
            return await _payDAL.DeleteAsync(id, tenantId) ? ServiceResult.Ok("Payment deleted successfully.") : ServiceResult.Fail("Unable to delete payment.");
        }

        public void PopulatePaymentDropdowns(PaymentFormViewModel vm, Guid tenantId)
        {
            vm.StudentOptions = GetMasterSelectList("Student", vm.PAY_StudentId.ToString());
            vm.PaymentMethodOptions = GetMasterSelectList("PaymentMethod", vm.PAY_PaymentMethodId.ToString());
        }

        // ==================== Helpers ====================
        private List<SelectListItem> GetMasterSelectList(string entityType, string selectedValue = null)
        {
            var items = _masterService.GetAll(entityType);
            var list = new List<SelectListItem>();
            if (items == null) return list;
            foreach (var item in items)
            {
                var keyEntry = item.FirstOrDefault(kvp => kvp.Key.EndsWith("_Id"));
                var id = keyEntry.Value?.ToString() ?? "";
                string displayName = null;
                var nameEntry = item.FirstOrDefault(kvp => kvp.Key.EndsWith("_Name"));
                if (nameEntry.Value != null) displayName = nameEntry.Value.ToString();
                if (string.IsNullOrEmpty(displayName))
                {
                    var firstName = item.FirstOrDefault(kvp => kvp.Key.EndsWith("_FirstName")).Value?.ToString() ?? "";
                    var lastName = item.FirstOrDefault(kvp => kvp.Key.EndsWith("_LastName")).Value?.ToString() ?? "";
                    displayName = $"{firstName} {lastName}".Trim();
                }
                if (string.IsNullOrEmpty(displayName)) displayName = item.Values.ElementAtOrDefault(1)?.ToString() ?? id;
                list.Add(new SelectListItem { Value = id, Text = displayName, Selected = id == selectedValue });
            }
            return list;
        }

        private static List<SelectListItem> GetInvoiceStatusSelectList(string selected = null)
        {
            var statuses = new[] { "Pending", "Partial", "Paid", "Overdue", "Cancelled" };
            return new List<SelectListItem>(Array.ConvertAll(statuses, s => new SelectListItem { Value = s, Text = s, Selected = s == selected }));
        }

        private static List<SelectListItem> GetPaymentStatusSelectList(string selected = null)
        {
            var statuses = new[] { "Completed", "Pending", "Failed", "Refunded" };
            return new List<SelectListItem>(Array.ConvertAll(statuses, s => new SelectListItem { Value = s, Text = s, Selected = s == selected }));
        }
    }
}
