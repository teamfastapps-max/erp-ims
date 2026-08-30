using System;

namespace IMS.Models.Entities
{
    public class Enrollment
    {
        public Guid E_Id { get; set; }
        public Guid E_TenantId { get; set; }
        public Guid E_StudentId { get; set; }
        public Guid E_AcademicYearId { get; set; }
        public Guid E_CourseId { get; set; }
        public Guid E_BatchId { get; set; }
        public string E_EnrollmentNumber { get; set; }
        public DateTime E_EnrollmentDate { get; set; }
        public string E_Status { get; set; }
        public DateTime? E_CompletionDate { get; set; }
        public DateTime E_CreatedAt { get; set; }
        public DateTime E_UpdatedAt { get; set; }

        public string StudentName { get; set; }
        public string CourseName { get; set; }
        public string BatchName { get; set; }
        public string AcademicYearName { get; set; }
    }
}
