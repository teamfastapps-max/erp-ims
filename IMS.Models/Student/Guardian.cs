using System;

namespace IMS.Models.Entities
{
    /// <summary>Maps 1:1 to dbo.Guardians_G. A guardian can be linked to multiple students.</summary>
    public class Guardian
    {
        public Guid G_Id { get; set; }
        public Guid G_TenantId { get; set; }
        public string G_FirstName { get; set; }
        public string G_LastName { get; set; }
        public string G_Phone { get; set; }
        public string G_Email { get; set; }
        public string G_Occupation { get; set; }
        public DateTime G_CreatedAt { get; set; }
        public DateTime G_UpdatedAt { get; set; }
        public DateTime? G_DeletedAt { get; set; }
    }

    /// <summary>Row shape returned by SP_StudentGuardians_GetByStudentId (join of link + guardian).</summary>
    public class StudentGuardianRecord
    {
        public Guid SG_Id { get; set; }
        public Guid SG_StudentId { get; set; }
        public Guid SG_GuardianId { get; set; }
        public string SG_Relation { get; set; }
        public bool SG_IsPrimary { get; set; }
        public string G_FirstName { get; set; }
        public string G_LastName { get; set; }
        public string G_Phone { get; set; }
        public string G_Email { get; set; }
        public string G_Occupation { get; set; }
    }

    /// <summary>Row shape returned by SP_Guardians_Search (lightweight, for autocomplete).</summary>
    public class GuardianSearchResult
    {
        public Guid G_Id { get; set; }
        public string G_FirstName { get; set; }
        public string G_LastName { get; set; }
        public string G_Phone { get; set; }
        public string G_Email { get; set; }
        public string G_Occupation { get; set; }
    }
}
