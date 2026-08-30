using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IMS.Models.Entities;

namespace IMS.DAL.Interfaces
{
    public interface IEnrollmentDAL
    {
        Task<Enrollment> GetByIdAsync(Guid id, Guid tenantId);
        Task<(List<Enrollment> Items, int TotalCount)> GetPagedAsync(Guid tenantId, string searchTerm,
            Guid? academicYearId, Guid? courseId, Guid? batchId, string status, int page, int pageSize);
        Task<bool> IsDuplicateAsync(Guid tenantId, Guid studentId, Guid batchId, Guid? excludeId);
        Task<Guid> CreateAsync(Enrollment e);
        Task<bool> UpdateAsync(Enrollment e);
        Task<bool> DeleteAsync(Guid id, Guid tenantId);
    }
}
