using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IMS.Models.Portal
{
    public class PortalLoginViewModel
    {
        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
        public string? ReturnUrl { get; set; }
    }

    public class PortalAuthResult
    {
        public bool IsAuthenticated { get; set; }
        public string? ErrorMessage { get; set; }
        public string UserType { get; set; } = "STUDENT"; // STUDENT or GUARDIAN
        public Guid UserId { get; set; }
        public Guid ActiveStudentId { get; set; }
        public Guid TenantId { get; set; }
        public Guid? BranchId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? StoredPassword { get; set; }
        public string? StudentCode { get; set; }
        public string? AdmissionNumber { get; set; }
        public string? BranchName { get; set; }
        public List<LinkedStudentDto> LinkedStudents { get; set; } = new();
    }

    public class LinkedStudentDto
    {
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string? StudentCode { get; set; }
        public string? AdmissionNumber { get; set; }
        public string? CourseName { get; set; }
        public string? BatchName { get; set; }
        public Guid? BranchId { get; set; }
    }

    public class PortalSessionModel
    {
        public Guid CurrentStudentId { get; set; }
        public string CurrentStudentName { get; set; } = string.Empty;
        public string? CurrentStudentCode { get; set; }
        public string? CurrentAdmissionNumber { get; set; }
        public string? CurrentBatchName { get; set; }
        public string? CurrentCourseName { get; set; }
        public string UserType { get; set; } = "STUDENT";
        public Guid UserId { get; set; }
        public Guid TenantId { get; set; }
        public Guid? BranchId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public string UserFullName { get; set; } = string.Empty;
        public List<LinkedStudentDto> LinkedStudents { get; set; } = new();
    }

    public class PortalDashboardViewModel
    {
        public StudentHeaderDto Student { get; set; } = new();
        public AttendanceMetricDto Attendance { get; set; } = new();
        public FinancialSummaryDto Finance { get; set; } = new();
        public List<TimetableSlotDto> TodaySchedule { get; set; } = new();
        public List<PortalNoticeDto> RecentNotices { get; set; } = new();
        public List<HomeTaskDto> ActiveHomeTasks { get; set; } = new();
    }

    public class StudentHeaderDto
    {
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string? StudentCode { get; set; }
        public string? AdmissionNumber { get; set; }
        public string? Gender { get; set; }
        public string? BloodGroup { get; set; }
        public string? BranchName { get; set; }
        public Guid? BatchId { get; set; }
        public string? BatchName { get; set; }
        public string? CourseName { get; set; }
        public string? AcademicYearName { get; set; }
    }

    public class AttendanceMetricDto
    {
        public int TotalSessions { get; set; }
        public int PresentSessions { get; set; }
        public int AbsentSessions { get; set; }
        public int HalfDaySessions { get; set; }
        public decimal AttendancePercentage { get; set; }
    }

    public class FinancialSummaryDto
    {
        public int TotalInvoices { get; set; }
        public decimal TotalBilled { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal OutstandingBalance { get; set; }
        public int PendingInvoicesCount { get; set; }
    }

    public class StudentProfileViewModel
    {
        public StudentHeaderDto Student { get; set; } = new();
        public string? DateOfBirth { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public string? AdmissionDate { get; set; }
        public string? Status { get; set; }
        public List<GuardianInfoDto> Guardians { get; set; } = new();
    }

    public class GuardianInfoDto
    {
        public Guid GuardianId { get; set; }
        public string GuardianName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Occupation { get; set; }
        public string Relationship { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
    }

    public class StudentIdCardViewModel
    {
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string? StudentCode { get; set; }
        public string? AdmissionNumber { get; set; }
        public string? DateOfBirth { get; set; }
        public string? BloodGroup { get; set; }
        public string? StudentPhone { get; set; }
        public string? StudentEmail { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? CourseName { get; set; }
        public string? BatchName { get; set; }
        public string? AcademicYearName { get; set; }
        public string? BranchName { get; set; }
        public string? BranchPhone { get; set; }
        public string? BranchEmail { get; set; }
        public string? BranchAddress { get; set; }
        public string? OrganizationName { get; set; }
        public string? OrganizationLogoUrl { get; set; }
        public string? EmergencyContact { get; set; }
    }

    public class GuardianIdCardViewModel
    {
        public Guid GuardianId { get; set; }
        public string GuardianName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Occupation { get; set; }
        public string? OrganizationName { get; set; }
        public string? OrganizationLogoUrl { get; set; }
        public List<LinkedStudentDto> Wards { get; set; } = new();
    }

    public class PortalAttendanceViewModel
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public int TotalDaysInMonth { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int HalfDayCount { get; set; }
        public int LateCount { get; set; }
        public int ExcusedCount { get; set; }
        public decimal Percentage { get; set; }
        public List<AttendanceRecordDto> Records { get; set; } = new();
    }

    public class AttendanceRecordDto
    {
        public DateTime AttendanceDate { get; set; }
        public string Status { get; set; } = string.Empty; // present, absent, half_day, late, excused
        public string? Remarks { get; set; }
        public string? SubjectName { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
    }

    public class PortalTimetableViewModel
    {
        public string BatchName { get; set; } = string.Empty;
        public List<TimetableSlotDto> Slots { get; set; } = new();
    }

    public class TimetableSlotDto
    {
        public Guid TimetableId { get; set; }
        public short DayOfWeek { get; set; } // 1=Sun, 2=Mon, 3=Tue, 4=Wed, 5=Thu, 6=Fri, 7=Sat
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string? SubjectCode { get; set; }
        public string? TeacherName { get; set; }
        public string? ClassroomName { get; set; }
        public string? ClassroomLocation { get; set; }
        public string DayName => DayOfWeek switch
        {
            1 => "Sunday",
            2 => "Monday",
            3 => "Tuesday",
            4 => "Wednesday",
            5 => "Thursday",
            6 => "Friday",
            7 => "Saturday",
            _ => "Unknown"
        };
    }

    public class PortalClassDetailsViewModel
    {
        public string BatchName { get; set; } = string.Empty;
        public string? BatchCode { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string? CourseCode { get; set; }
        public string? CourseDescription { get; set; }
        public string AcademicYearName { get; set; } = string.Empty;
        public string? BranchName { get; set; }
        public List<EnrolledSubjectDto> Subjects { get; set; } = new();
    }

    public class EnrolledSubjectDto
    {
        public Guid SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string? SubjectCode { get; set; }
        public decimal? Credits { get; set; }
        public decimal? MaxMarks { get; set; }
        public decimal? PassMarks { get; set; }
        public bool IsMandatory { get; set; }
    }

    public class PortalHomeTaskViewModel
    {
        public List<HomeTaskDto> Tasks { get; set; } = new();
    }

    public class HomeTaskDto
    {
        public Guid TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime AssignedDate { get; set; }
        public DateTime DueDate { get; set; }
        public string? TeacherAttachmentUrl { get; set; }
        public decimal? MaxMarks { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string? SubjectCode { get; set; }
        public Guid? SubmissionId { get; set; }
        public DateTime? SubmissionDate { get; set; }
        public string? SubmissionContent { get; set; }
        public string? StudentAttachmentUrl { get; set; }
        public decimal? MarksObtained { get; set; }
        public string? TeacherRemarks { get; set; }
        public string SubmissionStatus { get; set; } = "Pending";
        public bool IsSubmitted { get; set; }
        public bool IsOverdue { get; set; }
    }

    public class HomeTaskSubmitRequest
    {
        [Required]
        public Guid TaskId { get; set; }
        public string? Content { get; set; }
        public string? AttachmentUrl { get; set; }
    }

    public class PortalSyllabusViewModel
    {
        public List<SyllabusUnitDto> Units { get; set; } = new();
    }

    public class SyllabusUnitDto
    {
        public Guid SyllabusId { get; set; }
        public int UnitNumber { get; set; }
        public string UnitTitle { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? TotalHours { get; set; }
        public string? FileUrl { get; set; }
        public bool IsCompleted { get; set; }
        public Guid SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string? SubjectCode { get; set; }
    }

    public class PortalMockTestViewModel
    {
        public List<MockTestDto> Tests { get; set; } = new();
    }

    public class MockTestDto
    {
        public Guid TestId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime TestDate { get; set; }
        public int DurationMinutes { get; set; }
        public decimal TotalMarks { get; set; }
        public decimal PassMarks { get; set; }
        public string TestStatus { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public decimal? Score { get; set; }
        public decimal? Percentage { get; set; }
        public string? Grade { get; set; }
        public string? StudentResultStatus { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    public class PortalAdmitCardViewModel
    {
        public bool HasAdmitCard { get; set; }
        public string? ExamName { get; set; }
        public string? ExamCode { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string? StudentCode { get; set; }
        public string? RollNumber { get; set; }
        public string? CourseName { get; set; }
        public string? BatchName { get; set; }
        public string? CenterName { get; set; }
        public string? CenterAddress { get; set; }
        public string? OrganizationName { get; set; }
        public string? OrganizationLogoUrl { get; set; }
        public List<AdmitCardScheduleDto> Schedules { get; set; } = new();
    }

    public class AdmitCardScheduleDto
    {
        public Guid ExamSubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string? SubjectCode { get; set; }
        public decimal MaxMarks { get; set; }
        public decimal PassMarks { get; set; }
        public DateTime ExamDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string? RoomNumber { get; set; }
    }

    public class PortalMarkSheetViewModel
    {
        public List<ExamResultDto> ExamResults { get; set; } = new();
        public List<SubjectMarkDto> SubjectMarks { get; set; } = new();
    }

    public class ExamResultDto
    {
        public Guid ResultId { get; set; }
        public Guid ExamId { get; set; }
        public string ExamName { get; set; } = string.Empty;
        public string? ExamCode { get; set; }
        public decimal TotalMarks { get; set; }
        public decimal MarksObtained { get; set; }
        public decimal Percentage { get; set; }
        public string? OverallGrade { get; set; }
        public string ResultStatus { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public DateTime? PublishedAt { get; set; }
    }

    public class SubjectMarkDto
    {
        public Guid MarkId { get; set; }
        public Guid ExamId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string? SubjectCode { get; set; }
        public decimal MaxMarks { get; set; }
        public decimal PassMarks { get; set; }
        public decimal MarksObtained { get; set; }
        public decimal? Percentage { get; set; }
        public string? Grade { get; set; }
        public string? Remarks { get; set; }
    }

    public class PortalFeeTransactionViewModel
    {
        public decimal TotalBilled { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal OutstandingBalance { get; set; }
        public List<FeeInvoiceDto> Invoices { get; set; } = new();
        public List<FeePaymentDto> Payments { get; set; } = new();
    }

    public class FeeInvoiceDto
    {
        public Guid InvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Subtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal BalanceAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class FeePaymentDto
    {
        public Guid PaymentId { get; set; }
        public string PaymentNumber { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? TransactionReference { get; set; }
        public string? PaymentMethodName { get; set; }
        public string? Notes { get; set; }
    }

    public class PortalReceiptViewModel
    {
        public Guid PaymentId { get; set; }
        public string ReceiptNumber { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public decimal PaidAmount { get; set; }
        public string? TransactionRef { get; set; }
        public string? PaymentMode { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string? StudentCode { get; set; }
        public string? AdmissionNumber { get; set; }
        public string? CourseName { get; set; }
        public string? BatchName { get; set; }
        public string? BranchName { get; set; }
        public string? BranchAddress { get; set; }
        public string? BranchPhone { get; set; }
        public string? OrganizationName { get; set; }
        public string? OrganizationLogoUrl { get; set; }
        public List<ReceiptAllocationDto> AllocatedInvoices { get; set; } = new();
    }

    public class ReceiptAllocationDto
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public decimal AmountPaid { get; set; }
    }

    public class PortalLeaveApplyViewModel
    {
        [Required(ErrorMessage = "From date is required")]
        public DateTime? FromDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "To date is required")]
        public DateTime? ToDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Leave type is required")]
        public string LeaveType { get; set; } = "Sick"; // Sick, Casual, Medical, Other

        [Required(ErrorMessage = "Please state a reason")]
        [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
        public string Reason { get; set; } = string.Empty;

        public List<StudentLeaveDto> Leaves { get; set; } = new();
    }

    public class StudentLeaveDto
    {
        public Guid LeaveId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TotalDays { get; set; }
        public string LeaveType { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string AppliedBy { get; set; } = string.Empty;
        public string? RejectionReason { get; set; }
        public DateTime AppliedAt { get; set; }
    }

    public class PortalTransportViewModel
    {
        public bool IsAllocated { get; set; }
        public string? RouteName { get; set; }
        public string? RouteCode { get; set; }
        public string? VehicleNumber { get; set; }
        public string? DriverName { get; set; }
        public string? DriverPhone { get; set; }
        public string? HelperName { get; set; }
        public string? HelperPhone { get; set; }
        public string? StopName { get; set; }
        public TimeSpan? PickupTime { get; set; }
        public TimeSpan? DropTime { get; set; }
        public string? StartLocation { get; set; }
        public string? EndLocation { get; set; }
        public TimeSpan? RouteStartTime { get; set; }
        public TimeSpan? RouteEndTime { get; set; }
    }

    public class PortalTCApplyViewModel
    {
        [Required(ErrorMessage = "Reason for leaving is required")]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;

        [Required(ErrorMessage = "Expected leaving date is required")]
        public DateTime? ExpectedLeavingDate { get; set; } = DateTime.Today.AddDays(14);

        public List<TransferCertificateDto> Applications { get; set; } = new();
    }

    public class TransferCertificateDto
    {
        public Guid TCId { get; set; }
        public string ApplicationNumber { get; set; } = string.Empty;
        public DateTime ApplicationDate { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime ExpectedLeavingDate { get; set; }
        public bool LibraryClearance { get; set; }
        public bool FeeClearance { get; set; }
        public bool LabClearance { get; set; }
        public string Status { get; set; } = string.Empty; // Submitted, UnderReview, Approved, Issued, Rejected
        public string? CertificateNumber { get; set; }
        public DateTime? IssuedDate { get; set; }
        public string? Remarks { get; set; }
    }

    public class PortalNoticeDto
    {
        public Guid NoticeId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime? PublishedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Current password is required")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Registered email address is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; } = string.Empty;

        public bool EmailSent { get; set; }
        public string? InfoMessage { get; set; }
        public string? ResetUrlForDemo { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        public string? Email { get; set; }

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;

        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
    }

    public class AdminSetPasswordViewModel
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public string UserType { get; set; } = "STUDENT"; // STUDENT or GUARDIAN

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
