using System;

namespace IMS.Models.Entities
{
    public class Batch
    {
        public Guid BT_Id { get; set; }
        public Guid BT_TenantId { get; set; }
        public Guid BT_BranchId { get; set; }
        public Guid BT_CourseId { get; set; }
        public Guid BT_AcademicYearId { get; set; }
        public string BT_Name { get; set; }
        public string BT_Code { get; set; }
        public DateTime BT_StartDate { get; set; }
        public DateTime? BT_EndDate { get; set; }
        public int? BT_Capacity { get; set; }
        public string BT_Status { get; set; }
        public DateTime BT_CreatedAt { get; set; }
        public DateTime BT_UpdatedAt { get; set; }

        public string CourseName { get; set; }
        public string AcademicYearName { get; set; }
        public int EnrolledCount { get; set; }
    }
}
