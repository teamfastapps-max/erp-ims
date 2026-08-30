using System;
using System.Threading.Tasks;
using IMS.Models.ViewModels;

namespace IMS.Services.Interfaces
{
    public interface IExpenseService
    {
        Task<ExpenseIndexViewModel> GetExpenseListAsync(
            Guid tenantId, string searchTerm, Guid? branchId, Guid? expenseCategoryId,
            int pageNumber, int pageSize);

        Task<ExpenseDetailsViewModel> GetExpenseDetailsAsync(Guid id, Guid tenantId);

        Task<ExpenseFormViewModel> GetExpenseForEditAsync(Guid id, Guid tenantId);

        Task<ServiceResult> CreateExpenseAsync(ExpenseFormViewModel model, Guid tenantId, Guid createdBy);

        Task<ServiceResult> UpdateExpenseAsync(ExpenseFormViewModel model, Guid tenantId);

        Task<ServiceResult> DeleteExpenseAsync(Guid id, Guid tenantId);

        void PopulateDropdowns(ExpenseFormViewModel vm, Guid tenantId);
    }
}
