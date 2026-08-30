using System;
using System.Threading.Tasks;
using IMS.Models.ViewModels;

namespace IMS.Services.Interfaces
{
    public interface IEnrollmentService
    {
        Task<EnrollmentIndexViewModel> GetListAsync(Guid tenantId, string searchTerm, Guid? academicYearId,
            Guid? courseId, Guid? batchId, string status, int page, int pageSize);
        Task<EnrollmentDetailsViewModel> GetDetailsAsync(Guid id, Guid tenantId);
        Task<EnrollmentFormViewModel> GetForEditAsync(Guid id, Guid tenantId);
        Task<ServiceResult> CreateAsync(EnrollmentFormViewModel model, Guid tenantId);
        Task<ServiceResult> UpdateAsync(EnrollmentFormViewModel model, Guid tenantId);
        Task<ServiceResult> DeleteAsync(Guid id, Guid tenantId);

        void PopulateDropdowns(EnrollmentFormViewModel vm);
    }
}
