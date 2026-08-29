using System;

namespace IMS.Models.ViewModels
{
    /// <summary>
    /// One row in the "Guardians" section of the student form. Can represent
    /// either a brand-new guardian (ExistingGuardianId is null) or a link to
    /// an existing guardian picked via the search/autocomplete (ExistingGuardianId set).
    /// No DataAnnotations - validated client-side.
    /// </summary>
    public class GuardianRowViewModel
    {
        public Guid? ExistingGuardianId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Occupation { get; set; }
        public string Relation { get; set; }
        public bool IsPrimary { get; set; }
    }

    public class GuardianSearchResultViewModel
    {
        public Guid G_Id { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Occupation { get; set; }
    }
}
