using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IMS.Models.Entities;

namespace IMS.DAL.Interfaces
{
    public interface IAdmissionApplicationDAL
    {
        Task<AdmissionApplication> GetByIdAsync(Guid id, Guid tenantId);
        Task<(List<AdmissionApplication> Items, int TotalCount)> GetPagedAsync(Guid tenantId, string searchTerm,
            Guid? branchId, Guid? courseId, Guid? academicYearId, string status, int page, int pageSize);
        Task<bool> IsApplicationNumberTakenAsync(Guid tenantId, string number, Guid? excludeId);
        Task<Guid> CreateAsync(AdmissionApplication a);
        Task<bool> UpdateAsync(AdmissionApplication a);
        Task<bool> DeleteAsync(Guid id, Guid tenantId);
        Task<bool> ReviewAsync(Guid id, string status, string notes, Guid tenantId, Guid reviewedBy);
    }
}
