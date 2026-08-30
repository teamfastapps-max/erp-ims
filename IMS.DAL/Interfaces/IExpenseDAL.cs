using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IMS.Models.Entities;

namespace IMS.DAL.Interfaces
{
    public interface IExpenseDAL
    {
        Task<Expense> GetByIdAsync(Guid id, Guid tenantId);
        Task<(List<Expense> Items, int TotalCount)> GetPagedAsync(
            Guid tenantId, string searchTerm, Guid? branchId, Guid? expenseCategoryId,
            int pageNumber, int pageSize);
        Task<string> GetNextExpenseNumberAsync(Guid tenantId);
        Task<Guid> CreateAsync(Expense expense);
        Task<bool> UpdateAsync(Expense expense);
        Task<bool> DeleteAsync(Guid id, Guid tenantId);
    }
}
