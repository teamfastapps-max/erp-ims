using System;

namespace IMS.Models.Entities
{
    /// <summary>
    /// Maps 1:1 to dbo.Students_S. Column prefix S_ kept in DB only;
    /// property names are the friendly CLR equivalents.
    /// </summary>
    public class Student
    {
        public Guid S_Id { get; set; }
        public Guid S_TenantId { get; set; }
        public Guid S_BranchId { get; set; }
        public Guid? S_UserId { get; set; }
        public string S_StudentCode { get; set; }
        public string S_AdmissionNumber { get; set; }
        public string S_FirstName { get; set; }
        public string S_MiddleName { get; set; }
        public string S_LastName { get; set; }
        public DateTime? S_DateOfBirth { get; set; }
        public string S_Gender { get; set; }
        public string S_Email { get; set; }
        public string S_Phone { get; set; }
        public DateTime? S_AdmissionDate { get; set; }
        public string S_Status { get; set; }
        public DateTime S_CreatedAt { get; set; }
        public DateTime S_UpdatedAt { get; set; }
        public DateTime? S_DeletedAt { get; set; }
    }
}
