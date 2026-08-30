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
    public class ExpenseService : IExpenseService
    {
        private readonly IExpenseDAL _repo;
        private readonly IExpenseCategoryDAL _expenseCategoryDAL;
        private readonly IMasterService _masterService;

        public ExpenseService(IExpenseDAL repo, IExpenseCategoryDAL expenseCategoryDAL, IMasterService masterService)
        {
            _repo = repo;
            _expenseCategoryDAL = expenseCategoryDAL;
            _masterService = masterService;
        }

        public async Task<ExpenseIndexViewModel> GetExpenseListAsync(
            Guid tenantId, string searchTerm, Guid? branchId, Guid? expenseCategoryId,
            int pageNumber, int pageSize)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize is < 1 or > 100 ? 10 : pageSize;

            var (items, totalCount) = await _repo.GetPagedAsync(
                tenantId, searchTerm, branchId, expenseCategoryId, pageNumber, pageSize);

            var vm = new ExpenseIndexViewModel
            {
                SearchTerm = searchTerm,
                BranchFilter = branchId,
                ExpenseCategoryFilter = expenseCategoryId,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                BranchOptions = HardcodedMasterData.GetBranchSelectList(branchId),
                ExpenseCategoryOptions = new()
            };

            var categories = await _expenseCategoryDAL.GetAllAsync(tenantId);
            vm.ExpenseCategoryOptions = categories.Select(c => new SelectListItem
            {
                Value = c.EC_Id.ToString(),
                Text = c.EC_Name,
                Selected = c.EC_Id == expenseCategoryId
            }).ToList();

            foreach (var e in items)
            {
                vm.Expenses.Add(new ExpenseListItemViewModel
                {
                    EXP_Id = e.EXP_Id,
                    EXP_ExpenseNumber = e.EXP_ExpenseNumber,
                    EXP_ExpenseDate = e.EXP_ExpenseDate,
                    BranchName = HardcodedMasterData.GetBranchName(e.EXP_BranchId),
                    ExpenseCategoryName = e.ExpenseCategoryName ?? "-",
                    VendorName = e.VendorName ?? "-",
                    EXP_Amount = e.EXP_Amount,
                    EXP_Description = e.EXP_Description,
                    PaymentMethodName = e.PaymentMethodName ?? "-"
                });
            }

            return vm;
        }

        public async Task<ExpenseDetailsViewModel> GetExpenseDetailsAsync(Guid id, Guid tenantId)
        {
            var e = await _repo.GetByIdAsync(id, tenantId);
            if (e == null) return null;

            return new ExpenseDetailsViewModel
            {
                EXP_Id = e.EXP_Id,
                EXP_ExpenseNumber = e.EXP_ExpenseNumber,
                EXP_ExpenseDate = e.EXP_ExpenseDate,
                BranchName = HardcodedMasterData.GetBranchName(e.EXP_BranchId),
                ExpenseCategoryName = e.ExpenseCategoryName ?? "-",
                VendorName = e.VendorName ?? "-",
                EXP_Amount = e.EXP_Amount,
                EXP_Description = e.EXP_Description,
                PaymentMethodName = e.PaymentMethodName ?? "-",
                EXP_CreatedAt = e.EXP_CreatedAt,
                EXP_UpdatedAt = e.EXP_UpdatedAt
            };
        }

        public async Task<ExpenseFormViewModel> GetExpenseForEditAsync(Guid id, Guid tenantId)
        {
            var e = await _repo.GetByIdAsync(id, tenantId);
            if (e == null) return null;

            var vm = new ExpenseFormViewModel
            {
                EXP_Id = e.EXP_Id,
                EXP_BranchId = e.EXP_BranchId,
                EXP_ExpenseCategoryId = e.EXP_ExpenseCategoryId,
                EXP_VendorId = e.EXP_VendorId,
                EXP_ExpenseNumber = e.EXP_ExpenseNumber,
                EXP_ExpenseDate = e.EXP_ExpenseDate,
                EXP_Amount = e.EXP_Amount,
                EXP_Description = e.EXP_Description,
                EXP_PaymentMethodId = e.EXP_PaymentMethodId
            };

            PopulateDropdowns(vm, tenantId);
            return vm;
        }

        public async Task<ServiceResult> CreateExpenseAsync(ExpenseFormViewModel model, Guid tenantId, Guid createdBy)
        {
            var expenseNumber = await _repo.GetNextExpenseNumberAsync(tenantId);

            var entity = MapToEntity(model, tenantId, Guid.NewGuid(), createdBy);
            entity.EXP_ExpenseNumber = expenseNumber;
            var id = await _repo.CreateAsync(entity);
            return ServiceResult.Ok("Expense created successfully.", id);
        }

        public async Task<ServiceResult> UpdateExpenseAsync(ExpenseFormViewModel model, Guid tenantId)
        {
            if (!model.EXP_Id.HasValue)
                return ServiceResult.Fail("Expense Id is required for update.");

            var entity = MapToEntity(model, tenantId, model.EXP_Id.Value, Guid.Empty);
            var success = await _repo.UpdateAsync(entity);
            return success
                ? ServiceResult.Ok("Expense updated successfully.", model.EXP_Id)
                : ServiceResult.Fail("Expense not found.");
        }

        public async Task<ServiceResult> DeleteExpenseAsync(Guid id, Guid tenantId)
        {
            var success = await _repo.DeleteAsync(id, tenantId);
            return success
                ? ServiceResult.Ok("Expense deleted successfully.")
                : ServiceResult.Fail("Unable to delete expense.");
        }

        public void PopulateDropdowns(ExpenseFormViewModel vm, Guid tenantId)
        {
            vm.BranchOptions = HardcodedMasterData.GetBranchSelectList(vm.EXP_BranchId);

            var categories = _expenseCategoryDAL.GetAllAsync(tenantId).GetAwaiter().GetResult();
            vm.ExpenseCategoryOptions = categories.Select(c => new SelectListItem
            {
                Value = c.EC_Id.ToString(),
                Text = c.EC_Name,
                Selected = c.EC_Id == vm.EXP_ExpenseCategoryId
            }).ToList();

            vm.VendorOptions = GetMasterSelectList("Vendor", vm.EXP_VendorId?.ToString());
            vm.PaymentMethodOptions = GetMasterSelectList("PaymentMethod", vm.EXP_PaymentMethodId?.ToString());
        }

        private List<SelectListItem> GetMasterSelectList(string entityType, string selectedValue = null)
        {
            var items = _masterService.GetAll(entityType);
            var list = new List<SelectListItem>();
            if (items == null) return list;
            foreach (var item in items)
            {
                var keyEntry = item.FirstOrDefault(kvp => kvp.Key.EndsWith("_Id"));
                var id = keyEntry.Value?.ToString() ?? "";
                var nameEntry = item.FirstOrDefault(kvp => kvp.Key.EndsWith("_Name"));
                var displayName = nameEntry.Value?.ToString() ?? id;
                list.Add(new SelectListItem { Value = id, Text = displayName, Selected = id == selectedValue });
            }
            return list;
        }

        private static Expense MapToEntity(ExpenseFormViewModel m, Guid tenantId, Guid id, Guid createdBy) => new()
        {
            EXP_Id = id,
            EXP_TenantId = tenantId,
            EXP_BranchId = m.EXP_BranchId,
            EXP_ExpenseCategoryId = m.EXP_ExpenseCategoryId,
            EXP_VendorId = m.EXP_VendorId,
            EXP_ExpenseNumber = m.EXP_ExpenseNumber,
            EXP_ExpenseDate = m.EXP_ExpenseDate,
            EXP_Amount = m.EXP_Amount,
            EXP_Description = m.EXP_Description,
            EXP_PaymentMethodId = m.EXP_PaymentMethodId,
            EXP_CreatedBy = createdBy
        };
    }
}
