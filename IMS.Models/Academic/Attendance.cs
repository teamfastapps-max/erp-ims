using System;

namespace IMS.Models.Entities
{
    public class AttendanceSession
    {
        public Guid AS_Id { get; set; }
        public Guid AS_TenantId { get; set; }
        public Guid AS_BranchId { get; set; }
        public Guid AS_BatchId { get; set; }
        public Guid? AS_SubjectId { get; set; }
        public Guid? AS_StaffId { get; set; }
        public DateTime AS_AttendanceDate { get; set; }
        public TimeSpan? AS_StartTime { get; set; }
        public TimeSpan? AS_EndTime { get; set; }
        public string AS_Remarks { get; set; }
        public DateTime AS_CreatedAt { get; set; }
        public DateTime AS_UpdatedAt { get; set; }

        public string BranchName { get; set; }
        public string BatchName { get; set; }
        public string SubjectName { get; set; }
        public string StaffName { get; set; }
    }

    public class AttendanceRecord
    {
        public Guid AR_Id { get; set; }
        public Guid AR_AttendanceSessionId { get; set; }
        public Guid AR_StudentId { get; set; }
        public string AR_Status { get; set; }
        public string AR_Remarks { get; set; }
        public DateTime AR_CreatedAt { get; set; }
        public DateTime AR_UpdatedAt { get; set; }

        public string StudentName { get; set; }
        public string StudentCode { get; set; }
    }
}
