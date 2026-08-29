using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IMS.Models.ViewModels
{
    public class StudentListItemViewModel
    {
        public Guid S_Id { get; set; }
        public string S_StudentCode { get; set; }
        public string S_AdmissionNumber { get; set; }
        public string FullName { get; set; }
        public string S_Gender { get; set; }
        public string BranchName { get; set; }
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public DateTime? S_AdmissionDate { get; set; }
        public string S_Status { get; set; }
    }

    public class StudentIndexViewModel
    {
        public List<StudentListItemViewModel> Students { get; set; } = new();

        public string SearchTerm { get; set; }
        public string StatusFilter { get; set; }
        public Guid? BranchFilter { get; set; }
        public Guid? ClassFilter { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

        public List<SelectListItem> BranchOptions { get; set; } = new();
        public List<SelectListItem> StatusOptions { get; set; } = new();
        public List<SelectListItem> ClassOptions { get; set; } = new();
    }

    /// <summary>
    /// NOTE: intentionally has NO [Required]/[StringLength]/etc. attributes.
    /// All field-level and form-level validation is done client-side in
    /// wwwroot/js/students.js before the AJAX submit fires. The service
    /// layer still enforces business rules (uniqueness) server-side.
    /// </summary>
    public class StudentFormViewModel
    {
        public Guid? S_Id { get; set; }

        public Guid S_BranchId { get; set; }
        public string S_StudentCode { get; set; }
        public string S_AdmissionNumber { get; set; }
        public string S_FirstName { get; set; }
        public string S_MiddleName { get; set; }
        public string S_LastName { get; set; }
        public DateTime? S_DateOfBirth { get; set; }
        public string S_Gender { get; set; }
        public string S_Email { get; set; }
        public string S_Phone { get; set; }
        public DateTime? S_AdmissionDate { get; set; } = DateTime.Today;
        public string S_Status { get; set; } = "Admitted";

        public Guid? S_ClassId { get; set; }
        public Guid? S_SectionId { get; set; }
        public string S_BloodGroup { get; set; }
        public string S_AddressLine1 { get; set; }
        public string S_AddressLine2 { get; set; }
        public string S_City { get; set; }
        public string S_State { get; set; }
        public string S_PostalCode { get; set; }
        public string S_Country { get; set; }

        public List<GuardianRowViewModel> Guardians { get; set; } = new();

        // Dropdown sources (populated server-side, rendered by the view)
        public List<SelectListItem> BranchOptions { get; set; } = new();
        public List<SelectListItem> GenderOptions { get; set; } = new();
        public List<SelectListItem> StatusOptions { get; set; } = new();
        public List<SelectListItem> ClassOptions { get; set; } = new();
        public List<SelectListItem> SectionOptions { get; set; } = new();
        public List<SelectListItem> BloodGroupOptions { get; set; } = new();
        public List<SelectListItem> RelationOptions { get; set; } = new();
    }

    public class StudentDetailsViewModel
    {
        public Guid S_Id { get; set; }
        public string S_StudentCode { get; set; }
        public string S_AdmissionNumber { get; set; }
        public string FullName { get; set; }
        public DateTime? S_DateOfBirth { get; set; }
        public string S_Gender { get; set; }
        public string S_Email { get; set; }
        public string S_Phone { get; set; }
        public string BranchName { get; set; }
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public string S_BloodGroup { get; set; }
        public string FullAddress { get; set; }
        public DateTime? S_AdmissionDate { get; set; }
        public string S_Status { get; set; }
        public DateTime S_CreatedAt { get; set; }
        public DateTime S_UpdatedAt { get; set; }
        public List<GuardianRowViewModel> Guardians { get; set; } = new();
    }
}
