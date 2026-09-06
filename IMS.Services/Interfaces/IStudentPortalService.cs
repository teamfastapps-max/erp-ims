using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IMS.Models.Portal;

namespace IMS.Services.Interfaces
{
    public interface IStuddentPortalService
    {
        Task<PortalAuthResult> AuthenticateAsync(string email, string password);
        Task<PortalDashboardViewModel> GetDashboardAsync(Guid studentId, Guid tenantId);
        Task<StudentProfileViewModel> GetStudentProfileAsync(Guid studentId, Guid tenantId);
        Task<StudentIdCardViewModel> GetStudentIdCardAsync(Guid studentId, Guid tenantId);
        Task<GuardianIdCardViewModel> GetGuardianIdCardAsync(Guid? guardianId, Guid? studentId, Guid tenantId);
        Task<PortalAttendanceViewModel> GetAttendanceCalendarAsync(Guid studentId, Guid tenantId, int? month, int? year);
        Task<PortalTimetableViewModel> GetTimetableAsync(Guid studentId, Guid tenantId);
        Task<PortalClassDetailsViewModel> GetClassDetailsAsync(Guid studentId, Guid tenantId);
        Task<PortalHomeTaskViewModel> GetHomeTasksAsync(Guid studentId, Guid tenantId);
        Task<bool> SubmitHomeTaskAsync(Guid taskId, Guid studentId, string? content, string? attachmentUrl);
        Task<PortalSyllabusViewModel> GetSyllabusAsync(Guid studentId, Guid tenantId);
        Task<PortalMockTestViewModel> GetMockTestsAsync(Guid studentId, Guid tenantId);
        Task<PortalAdmitCardViewModel> GetAdmitCardAsync(Guid studentId, Guid tenantId);
        Task<PortalMarkSheetViewModel> GetMarkSheetAsync(Guid studentId, Guid tenantId);
        Task<PortalFeeTransactionViewModel> GetFeeTransactionsAsync(Guid studentId, Guid tenantId);
        Task<PortalReceiptViewModel?> GetReceiptDetailsAsync(Guid paymentId, Guid tenantId);
        Task<(bool Success, string Message)> ApplyLeaveAsync(Guid tenantId, Guid studentId, DateTime fromDate, DateTime toDate, string leaveType, string reason, string appliedBy);
        Task<List<StudentLeaveDto>> GetLeavesAsync(Guid studentId, Guid tenantId);
        Task<PortalTransportViewModel> GetTransportDetailsAsync(Guid studentId, Guid tenantId);
        Task<(bool Success, string Message)> ApplyTCAsync(Guid tenantId, Guid studentId, string reason, DateTime expectedLeavingDate);
        Task<List<TransferCertificateDto>> GetTCStatusAsync(Guid studentId, Guid tenantId);
        Task<(bool Success, string Message)> DeleteLeaveAsync(Guid leaveId, Guid studentId, Guid tenantId);
        Task<(bool Success, string Message)> DeleteTCAsync(Guid tcId, Guid studentId, Guid tenantId);
        Task<TransferCertificatePrintViewModel?> GetTCForPrintAsync(Guid tcId, Guid studentId, Guid tenantId);
        Task<List<PortalNoticeDto>> GetNoticesAsync(Guid tenantId);
        Task<(bool Success, string Message)> ChangePasswordAsync(Guid userId, string userType, string currentPassword, string newPassword);
        Task<bool> SetPasswordDirectAsync(Guid userId, string userType, string plainPassword);
        Task<(bool Success, string Message, string? DemoResetUrl)> ForgotPasswordAsync(string email, string baseUrl);
        Task<(bool Success, string Message)> ResetPasswordWithTokenAsync(string token, string newPassword);
    }
}
