using System;
using System.Threading.Tasks;
using IMS.Models.ViewModels;

namespace IMS.Services.Interfaces
{
    public interface ICourseSubjectService
    {
        Task<CourseSubjectIndexViewModel> GetListAsync(Guid tenantId, Guid? courseId);
        Task<CourseSubjectFormViewModel> GetForEditAsync(Guid courseId, Guid subjectId, Guid tenantId);
        Task<ServiceResult> CreateAsync(CourseSubjectFormViewModel model, Guid tenantId);
        Task<ServiceResult> UpdateAsync(CourseSubjectFormViewModel model, Guid tenantId);
        Task<ServiceResult> DeleteAsync(Guid courseId, Guid subjectId, Guid tenantId);

        void PopulateDropdowns(CourseSubjectFormViewModel vm, Guid tenantId);
    }
}
