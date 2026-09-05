using System;
using System.Threading.Tasks;
using IMS.Models.ViewModels;

namespace IMS.Services.Interfaces
{
    // NOTE: uses the ServiceResult class already declared in IStudentService.cs
    // (same namespace, IMS.Services.Interfaces) - not redefined here.

    public interface ITeacherLeaveService
    {
        Task<TeacherLeaveIndexViewModel> GetAllForAdminAsync(Guid tenantId, string status, int page, int pageSize);
        Task<MyLeaveViewModel> GetMyLeaveAsync(Guid tenantId, Guid teacherId, int page, int pageSize);
        Task<ServiceResult> ApplyAsync(Guid tenantId, Guid teacherId, string accessToken, TeacherLeaveApplyViewModel model);
        Task<ServiceResult> ApproveAsync(Guid tenantId, Guid leaveId, Guid approvedBy);
        Task<ServiceResult> RejectAsync(Guid tenantId, Guid leaveId, Guid approvedBy, string rejectionReason);
        Task<ServiceResult> CancelAsync(Guid tenantId, Guid leaveId, Guid requestingTeacherId);
        Task<ServiceResult> UpdateAsync(Guid tenantId, Guid leaveId, Guid requestingTeacherId, TeacherLeaveApplyViewModel model);

    }
}
