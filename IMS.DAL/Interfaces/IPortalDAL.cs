using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IMS.Models.Portal;

namespace IMS.DAL.Interfaces
{
    public interface IPortalDAL
    {
        Task<PortalAuthResult> AuthenticateByEmailAsync(string email);
        Task<PortalDashboardViewModel> GetDashboardAsync(Guid studentId, Guid tenantId);
        Task<StudentProfileViewModel> GetStudentProfileAsync(Guid studentId, Guid tenantId);
        Task<StudentIdCardViewModel> GetStudentIdCardDataAsync(Guid studentId, Guid tenantId);
        Task<GuardianIdCardViewModel> GetGuardianIdCardDataAsync(Guid guardianId, Guid tenantId);
        Task<PortalAttendanceViewModel> GetAttendanceCalendarAsync(Guid studentId, Guid tenantId, int month, int year);
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
        Task<bool> ApplyLeaveAsync(Guid leaveId, Guid tenantId, Guid studentId, DateTime fromDate, DateTime toDate, string leaveType, string reason, string appliedBy);
        Task<List<StudentLeaveDto>> GetLeavesAsync(Guid studentId, Guid tenantId);
        Task<PortalTransportViewModel> GetTransportDetailsAsync(Guid studentId, Guid tenantId);
        Task<bool> ApplyTCAsync(Guid tcId, Guid tenantId, Guid studentId, string reason, DateTime expectedLeavingDate);
        Task<List<TransferCertificateDto>> GetTCStatusAsync(Guid studentId, Guid tenantId);
        Task<List<PortalNoticeDto>> GetNoticesAsync(Guid tenantId);
        Task<bool> UpdatePasswordAsync(Guid userId, string userType, string newPassword);
        Task<(bool Success, string Message, string? Token, string? FullName, string? UserType)> GeneratePasswordResetTokenAsync(string email, string token, string? otp, int expiryMinutes);
        Task<(bool Success, string Message)> ResetPasswordWithTokenAsync(string token, string newPasswordHash);
        Task<bool> SetUserPasswordAsync(Guid userId, string userType, string passwordHash);
    }
}
