using System;

namespace IMS.Models.Entities
{
    public class AdmissionApplication
    {
        public Guid AA_Id { get; set; }
        public Guid AA_TenantId { get; set; }
        public Guid AA_BranchId { get; set; }
        public string AA_ApplicationNumber { get; set; }
        public string AA_FirstName { get; set; }
        public string AA_LastName { get; set; }
        public DateTime? AA_DateOfBirth { get; set; }
        public string AA_Gender { get; set; }
        public string AA_Email { get; set; }
        public string AA_Phone { get; set; }
        public Guid? AA_CourseId { get; set; }
        public Guid AA_AcademicYearId { get; set; }
        public string AA_Status { get; set; }
        public DateTime? AA_SubmittedAt { get; set; }
        public DateTime? AA_ReviewedAt { get; set; }
        public Guid? AA_ReviewedBy { get; set; }
        public string AA_Notes { get; set; }
        public DateTime AA_CreatedAt { get; set; }
        public DateTime AA_UpdatedAt { get; set; }

        public string CourseName { get; set; }
        public string AcademicYearName { get; set; }
    }
}
