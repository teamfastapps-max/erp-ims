using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IMS.Models.Entities;

namespace IMS.DAL.Interfaces
{
    public interface IStudentRepository
    {
        Task<Guid> CreateAsync(Student student);
        Task<bool> UpdateAsync(Student student);
        Task<bool> SoftDeleteAsync(Guid id, Guid tenantId);
        Task<Student> GetByIdAsync(Guid id, Guid tenantId);

        Task<(List<Student> Items, int TotalCount)> GetPagedAsync(
            Guid tenantId,
            string searchTerm,
            string status,
            Guid? branchId,
            int pageNumber,
            int pageSize);

        Task<bool> IsAdmissionNumberTakenAsync(Guid tenantId, string admissionNumber, Guid? excludeId);
        Task<bool> IsStudentCodeTakenAsync(Guid tenantId, string studentCode, Guid? excludeId);
    }
}
