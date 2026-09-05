using System;

namespace IMS.Models.Entities
{
    public class TeacherLeave
    {
        public Guid TL_Id { get; set; }
        public Guid TL_TenantId { get; set; }
        public Guid TL_TeacherId { get; set; }
        public string TL_TeacherName { get; set; }
        public string TL_LeaveType { get; set; }
        public DateTime TL_FromDate { get; set; }
        public DateTime TL_ToDate { get; set; }
        public int TL_TotalDays { get; set; }
        public string TL_Reason { get; set; }
        public string TL_Status { get; set; }
        public DateTime TL_AppliedAt { get; set; }
        public Guid TL_AppliedBy { get; set; }
        public Guid? TL_ApprovedBy { get; set; }
        public DateTime? TL_ApprovedAt { get; set; }
        public string TL_RejectionReason { get; set; }
        public DateTime TL_CreatedAt { get; set; }
        public DateTime TL_UpdatedAt { get; set; }
    }
}
