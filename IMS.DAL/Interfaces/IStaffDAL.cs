using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IMS.Models.Entities;

namespace IMS.DAL.Interfaces
{
    public interface IStaffDAL
    {
        Task<Staff> GetByIdAsync(Guid id, Guid tenantId);
        Task<(List<Staff> Items, int TotalCount)> GetPagedAsync(
            Guid tenantId, string searchTerm, Guid? branchId, string status,
            int pageNumber, int pageSize);
        Task<bool> IsEmployeeCodeTakenAsync(Guid tenantId, string code, Guid? excludeId);
        Task<Guid> CreateAsync(Staff staff);
        Task<bool> UpdateAsync(Staff staff);
        Task<bool> DeleteAsync(Guid id, Guid tenantId);
    }
}
