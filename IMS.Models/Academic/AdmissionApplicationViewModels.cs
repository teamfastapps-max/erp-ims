using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IMS.Models.ViewModels
{
    public class AdmissionApplicationListItemViewModel
    {
        public Guid AA_Id { get; set; }
        public string AA_ApplicationNumber { get; set; }
        public string FullName => string.Join(" ", new[] { AA_FirstName, AA_LastName }.Where(p => !string.IsNullOrWhiteSpace(p)));
        public string AA_FirstName { get; set; }
        public string AA_LastName { get; set; }
        public string CourseName { get; set; }
        public string AcademicYearName { get; set; }
        public DateTime? AA_SubmittedAt { get; set; }
        public string AA_Status { get; set; }
    }

    public class AdmissionApplicationIndexViewModel
    {
        public List<AdmissionApplicationListItemViewModel> Applications { get; set; } = new();
        public string SearchTerm { get; set; }
        public Guid? BranchFilter { get; set; }
        public Guid? CourseFilter { get; set; }
        public Guid? AcademicYearFilter { get; set; }
        public string StatusFilter { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public List<SelectListItem> BranchOptions { get; set; } = new();
        public List<SelectListItem> CourseOptions { get; set; } = new();
        public List<SelectListItem> AcademicYearOptions { get; set; } = new();
        public List<SelectListItem> StatusOptions { get; set; } = new();
    }

    public class AdmissionApplicationFormViewModel
    {
        public Guid? AA_Id { get; set; }
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
        public string AA_Status { get; set; } = "Submitted";
        public string AA_Notes { get; set; }

        public List<SelectListItem> BranchOptions { get; set; } = new();
        public List<SelectListItem> CourseOptions { get; set; } = new();
        public List<SelectListItem> AcademicYearOptions { get; set; } = new();
        public List<SelectListItem> GenderOptions { get; set; } = new();
        public List<SelectListItem> StatusOptions { get; set; } = new();
    }

    public class AdmissionApplicationDetailsViewModel
    {
        public Guid AA_Id { get; set; }
        public string AA_ApplicationNumber { get; set; }
        public string FullName => string.Join(" ", new[] { AA_FirstName, AA_LastName }.Where(p => !string.IsNullOrWhiteSpace(p)));
        public string AA_FirstName { get; set; }
        public string AA_LastName { get; set; }
        public DateTime? AA_DateOfBirth { get; set; }
        public string AA_Gender { get; set; }
        public string AA_Email { get; set; }
        public string AA_Phone { get; set; }
        public string CourseName { get; set; }
        public string AcademicYearName { get; set; }
        public string AA_Status { get; set; }
        public DateTime? AA_SubmittedAt { get; set; }
        public DateTime? AA_ReviewedAt { get; set; }
        public string AA_Notes { get; set; }
        public DateTime AA_CreatedAt { get; set; }
        public DateTime AA_UpdatedAt { get; set; }
    }

    public class AdmissionReviewViewModel
    {
        public Guid AA_Id { get; set; }
        public string AA_Status { get; set; }
        public string AA_Notes { get; set; }
    }
}
