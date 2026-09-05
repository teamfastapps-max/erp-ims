using System;

namespace IMS.Models.Entities
{
    public class TeacherAttendance
    {
        public Guid TA_Id { get; set; }
        public Guid TA_TenantId { get; set; }
        public Guid TA_TeacherId { get; set; }
        public DateTime TA_Date { get; set; }
        public string TA_Status { get; set; }
        public string TA_Remarks { get; set; }
        public Guid TA_MarkedBy { get; set; }
        public DateTime TA_CreatedAt { get; set; }
        public DateTime TA_UpdatedAt { get; set; }
    }
}
