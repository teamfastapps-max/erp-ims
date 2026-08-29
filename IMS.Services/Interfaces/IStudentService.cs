using System;
using System.Threading.Tasks;
using IMS.Models.ViewModels;

namespace IMS.Services.Interfaces
{
    public class ServiceResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public Guid? Id { get; set; }

        public static ServiceResult Ok(string message = null, Guid? id = null) => new() { Success = true, Message = message, Id = id };
        public static ServiceResult Fail(string message) => new() { Success = false, Message = message };
    }

    public interface IStudentService
    {
        Task<StudentIndexViewModel> GetStudentListAsync(
            Guid tenantId, string searchTerm, string status, Guid? branchId, Guid? classId, int pageNumber, int pageSize);

        Task<StudentDetailsViewModel> GetStudentDetailsAsync(Guid id, Guid tenantId);

        Task<StudentFormViewModel> GetStudentForEditAsync(Guid id, Guid tenantId);

        Task<ServiceResult> CreateStudentAsync(StudentFormViewModel model, Guid tenantId);

        Task<ServiceResult> UpdateStudentAsync(StudentFormViewModel model, Guid tenantId);

        Task<ServiceResult> DeleteStudentAsync(Guid id, Guid tenantId);
    }
}
