using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using IMS.DAL.Interfaces;
using IMS.Models.Portal;
using IMS.Services.Interfaces;

namespace IMS.Services
{
    public class PortalService : IPortalService
    {
        private readonly IPortalDAL _dal;

        public PortalService(IPortalDAL dal)
        {
            _dal = dal;
        }

        public async Task<PortalAuthResult> AuthenticateAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return new PortalAuthResult { IsAuthenticated = false, ErrorMessage = "Email and password are required." };
            }

            var auth = await _dal.AuthenticateByEmailAsync(email);
            if (!auth.IsAuthenticated)
            {
                return auth;
            }

            // Verify password
            bool passwordValid = false;
            if (string.IsNullOrEmpty(auth.StoredPassword))
            {
                // Initial password fallback: Allow "Welcome@123" if no password is set yet
                if (password == "Welcome@123" || (!string.IsNullOrEmpty(auth.StudentCode) && password == auth.StudentCode))
                {
                    passwordValid = true;
                    // Automatically hash and persist the password
                    var newHash = HashPassword(password);
                    await _dal.UpdatePasswordAsync(auth.UserId, auth.UserType, newHash);
                }
            }
            else
            {
                passwordValid = VerifyPassword(password, auth.StoredPassword);
            }

            if (!passwordValid)
            {
                return new PortalAuthResult
                {
                    IsAuthenticated = false,
                    ErrorMessage = "Invalid password. Please verify your credentials."
                };
            }

            // Do not leak stored password outside service layer
            auth.StoredPassword = null;
            return auth;
        }

        public Task<PortalDashboardViewModel> GetDashboardAsync(Guid studentId, Guid tenantId)
        {
            return _dal.GetDashboardAsync(studentId, tenantId);
        }

        public Task<StudentProfileViewModel> GetStudentProfileAsync(Guid studentId, Guid tenantId)
        {
            return _dal.GetStudentProfileAsync(studentId, tenantId);
        }

        public Task<StudentIdCardViewModel> GetStudentIdCardAsync(Guid studentId, Guid tenantId)
        {
            return _dal.GetStudentIdCardDataAsync(studentId, tenantId);
        }

        public Task<GuardianIdCardViewModel> GetGuardianIdCardAsync(Guid guardianId, Guid tenantId)
        {
            return _dal.GetGuardianIdCardDataAsync(guardianId, tenantId);
        }

        public Task<PortalAttendanceViewModel> GetAttendanceCalendarAsync(Guid studentId, Guid tenantId, int? month, int? year)
        {
            var now = DateTime.UtcNow;
            int m = month.HasValue && month.Value >= 1 && month.Value <= 12 ? month.Value : now.Month;
            int y = year.HasValue && year.Value >= 2000 && year.Value <= 2100 ? year.Value : now.Year;
            return _dal.GetAttendanceCalendarAsync(studentId, tenantId, m, y);
        }

        public Task<PortalTimetableViewModel> GetTimetableAsync(Guid studentId, Guid tenantId)
        {
            return _dal.GetTimetableAsync(studentId, tenantId);
        }

        public Task<PortalClassDetailsViewModel> GetClassDetailsAsync(Guid studentId, Guid tenantId)
        {
            return _dal.GetClassDetailsAsync(studentId, tenantId);
        }

        public Task<PortalHomeTaskViewModel> GetHomeTasksAsync(Guid studentId, Guid tenantId)
        {
            return _dal.GetHomeTasksAsync(studentId, tenantId);
        }

        public Task<bool> SubmitHomeTaskAsync(Guid taskId, Guid studentId, string? content, string? attachmentUrl)
        {
            return _dal.SubmitHomeTaskAsync(taskId, studentId, content, attachmentUrl);
        }

        public Task<PortalSyllabusViewModel> GetSyllabusAsync(Guid studentId, Guid tenantId)
        {
            return _dal.GetSyllabusAsync(studentId, tenantId);
        }

        public Task<PortalMockTestViewModel> GetMockTestsAsync(Guid studentId, Guid tenantId)
        {
            return _dal.GetMockTestsAsync(studentId, tenantId);
        }

        public Task<PortalAdmitCardViewModel> GetAdmitCardAsync(Guid studentId, Guid tenantId)
        {
            return _dal.GetAdmitCardAsync(studentId, tenantId);
        }

        public Task<PortalMarkSheetViewModel> GetMarkSheetAsync(Guid studentId, Guid tenantId)
        {
            return _dal.GetMarkSheetAsync(studentId, tenantId);
        }

        public Task<PortalFeeTransactionViewModel> GetFeeTransactionsAsync(Guid studentId, Guid tenantId)
        {
            return _dal.GetFeeTransactionsAsync(studentId, tenantId);
        }

        public Task<PortalReceiptViewModel?> GetReceiptDetailsAsync(Guid paymentId, Guid tenantId)
        {
            return _dal.GetReceiptDetailsAsync(paymentId, tenantId);
        }

        public async Task<(bool Success, string Message)> ApplyLeaveAsync(Guid tenantId, Guid studentId, DateTime fromDate, DateTime toDate, string leaveType, string reason, string appliedBy)
        {
            if (toDate < fromDate)
            {
                return (false, "End date cannot be earlier than start date.");
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                return (false, "Please provide a reason for the leave application.");
            }

            var leaveId = Guid.NewGuid();
            var ok = await _dal.ApplyLeaveAsync(leaveId, tenantId, studentId, fromDate, toDate, leaveType, reason.Trim(), appliedBy);
            return ok ? (true, "Leave application submitted successfully.") : (false, "Failed to submit leave request.");
        }

        public Task<List<StudentLeaveDto>> GetLeavesAsync(Guid studentId, Guid tenantId)
        {
            return _dal.GetLeavesAsync(studentId, tenantId);
        }

        public Task<PortalTransportViewModel> GetTransportDetailsAsync(Guid studentId, Guid tenantId)
        {
            return _dal.GetTransportDetailsAsync(studentId, tenantId);
        }

        public async Task<(bool Success, string Message)> ApplyTCAsync(Guid tenantId, Guid studentId, string reason, DateTime expectedLeavingDate)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                return (false, "Reason for Transfer Certificate is required.");
            }

            var tcId = Guid.NewGuid();
            var ok = await _dal.ApplyTCAsync(tcId, tenantId, studentId, reason.Trim(), expectedLeavingDate);
            return ok ? (true, "Transfer Certificate application submitted successfully.") : (false, "Failed to submit TC application.");
        }

        public Task<List<TransferCertificateDto>> GetTCStatusAsync(Guid studentId, Guid tenantId)
        {
            return _dal.GetTCStatusAsync(studentId, tenantId);
        }

        public Task<List<PortalNoticeDto>> GetNoticesAsync(Guid tenantId)
        {
            return _dal.GetNoticesAsync(tenantId);
        }

        public async Task<(bool Success, string Message)> ChangePasswordAsync(Guid userId, string userType, string currentPassword, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                return (false, "New password must be at least 6 characters long.");
            }

            var hashed = HashPassword(newPassword);
            var ok = await _dal.UpdatePasswordAsync(userId, userType, hashed);
            return ok ? (true, "Password changed successfully.") : (false, "Failed to update password.");
        }

        public async Task<bool> SetPasswordDirectAsync(Guid userId, string userType, string plainPassword)
        {
            var hashed = HashPassword(plainPassword);
            return await _dal.SetUserPasswordAsync(userId, userType, hashed);
        }

        public async Task<(bool Success, string Message, string? DemoResetUrl)> ForgotPasswordAsync(string email, string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return (false, "Please provide a valid registered email address.", null);
            }

            var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
            var otp = RandomNumberGenerator.GetInt32(100000, 999999).ToString();

            var res = await _dal.GeneratePasswordResetTokenAsync(email.Trim(), token, otp, 30);
            if (!res.Success)
            {
                return (false, res.Message, null);
            }

            var cleanBase = string.IsNullOrEmpty(baseUrl) ? string.Empty : baseUrl.TrimEnd('/');
            var resetUrl = $"{cleanBase}/StudentPortal/Auth/ResetPassword?token={token}";

            return (true, res.Message, resetUrl);
        }

        public async Task<(bool Success, string Message)> ResetPasswordWithTokenAsync(string token, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return (false, "Reset token is missing or invalid.");
            }
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                return (false, "Password must be at least 6 characters long.");
            }

            var hashed = HashPassword(newPassword);
            return await _dal.ResetPasswordWithTokenAsync(token.Trim(), hashed);
        }

        #region Password Hashing Helper
        private static string HashPassword(string password)
        {
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(32);

            return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
        }

        private static bool VerifyPassword(string enteredPassword, string storedHash)
        {
            if (string.IsNullOrEmpty(storedHash)) return false;

            // Plaintext fallback check (if legacy or admin-seeded plain text)
            if (!storedHash.Contains(":"))
            {
                return enteredPassword == storedHash;
            }

            var parts = storedHash.Split(':');
            if (parts.Length != 2) return false;

            byte[] salt = Convert.FromBase64String(parts[0]);
            byte[] storedSubkey = Convert.FromBase64String(parts[1]);

            using var pbkdf2 = new Rfc2898DeriveBytes(enteredPassword, salt, 10000, HashAlgorithmName.SHA256);
            byte[] generatedSubkey = pbkdf2.GetBytes(32);

            return CryptographicOperations.FixedTimeEquals(storedSubkey, generatedSubkey);
        }
        #endregion
    }
}
