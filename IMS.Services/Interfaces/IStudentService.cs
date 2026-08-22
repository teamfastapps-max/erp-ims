using System;
using System.Threading.Tasks;
using IMS.Models.ViewModels;

namespace IMS.Services.Interfaces
{
    public interface IStudentService
    {
        Task<StudentIndexViewModel> GetStudentListAsync(
            Guid tenantId, string searchTerm, string status, Guid? branchId, int pageNumber, int pageSize);

        Task<StudentDetailsViewModel> GetStudentDetailsAsync(Guid id, Guid tenantId);

        Task<StudentFormViewModel> GetStudentForEditAsync(Guid id, Guid tenantId);

        /// <returns>New student Id, or null + error message on validation failure</returns>
        Task<(Guid? Id, string Error)> CreateStudentAsync(StudentFormViewModel model, Guid tenantId);

        Task<(bool Success, string Error)> UpdateStudentAsync(StudentFormViewModel model, Guid tenantId);

        Task<bool> DeleteStudentAsync(Guid id, Guid tenantId);
    }
}
