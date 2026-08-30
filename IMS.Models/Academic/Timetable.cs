using System;

namespace IMS.Models.Entities
{
    public class Timetable
    {
        public Guid TT_Id { get; set; }
        public Guid TT_TenantId { get; set; }
        public Guid TT_BranchId { get; set; }
        public Guid TT_BatchId { get; set; }
        public Guid TT_SubjectId { get; set; }
        public Guid TT_StaffId { get; set; }
        public Guid? TT_ClassroomId { get; set; }
        public int TT_DayOfWeek { get; set; }
        public TimeSpan TT_StartTime { get; set; }
        public TimeSpan TT_EndTime { get; set; }
        public DateTime? TT_EffectiveFrom { get; set; }
        public DateTime? TT_EffectiveTo { get; set; }
        public DateTime TT_CreatedAt { get; set; }
        public DateTime TT_UpdatedAt { get; set; }

        public string SubjectName { get; set; }
        public string StaffName { get; set; }
        public string ClassroomName { get; set; }
        public string BatchName { get; set; }
    }
}
