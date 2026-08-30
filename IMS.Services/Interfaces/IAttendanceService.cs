using System;
using System.Threading.Tasks;
using IMS.Models.ViewModels;

namespace IMS.Services.Interfaces
{
    public interface IAttendanceService
    {
        Task<AttendanceSessionIndexViewModel> GetSessionListAsync(
            Guid tenantId, string searchTerm, Guid? branchId, Guid? batchId,
            DateTime? date, int pageNumber, int pageSize);

        Task<AttendanceSessionFormViewModel> GetSessionForEditAsync(Guid id, Guid tenantId);

        Task<ServiceResult> CreateSessionAsync(AttendanceSessionFormViewModel model, Guid tenantId);

        Task<ServiceResult> UpdateSessionAsync(AttendanceSessionFormViewModel model, Guid tenantId);

        Task<ServiceResult> DeleteSessionAsync(Guid id, Guid tenantId);

        Task<AttendanceMarkViewModel> GetMarkAttendanceAsync(Guid sessionId, Guid tenantId);

        Task<ServiceResult> SaveAttendanceAsync(AttendanceRecordSaveViewModel model, Guid tenantId);

        void PopulateSessionDropdowns(AttendanceSessionFormViewModel vm);
    }
}
