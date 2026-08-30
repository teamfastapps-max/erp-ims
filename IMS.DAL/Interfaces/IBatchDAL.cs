using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IMS.Models.Entities;

namespace IMS.DAL.Interfaces
{
    public interface IBatchDAL
    {
        Task<Batch> GetByIdAsync(Guid id, Guid tenantId);
        Task<(List<Batch> Items, int TotalCount)> GetPagedAsync(
            Guid tenantId, string searchTerm, Guid? branchId, Guid? courseId,
            Guid? academicYearId, string status, int pageNumber, int pageSize);
        Task<bool> IsCodeTakenAsync(Guid tenantId, string code, Guid? excludeId);
        Task<Guid> CreateAsync(Batch batch);
        Task<bool> UpdateAsync(Batch batch);
        Task<bool> DeleteAsync(Guid id, Guid tenantId);
    }
}
