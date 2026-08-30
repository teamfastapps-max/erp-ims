using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IMS.Models.ViewModels
{
    public class EnrollmentListItemViewModel
    {
        public Guid E_Id { get; set; }
        public string E_EnrollmentNumber { get; set; }
        public string StudentName { get; set; }
        public string CourseName { get; set; }
        public string BatchName { get; set; }
        public string AcademicYearName { get; set; }
        public DateTime E_EnrollmentDate { get; set; }
        public string E_Status { get; set; }
    }

    public class EnrollmentIndexViewModel
    {
        public List<EnrollmentListItemViewModel> Enrollments { get; set; } = new();
        public string SearchTerm { get; set; }
        public Guid? AcademicYearFilter { get; set; }
        public Guid? CourseFilter { get; set; }
        public Guid? BatchFilter { get; set; }
        public string StatusFilter { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public List<SelectListItem> AcademicYearOptions { get; set; } = new();
        public List<SelectListItem> CourseOptions { get; set; } = new();
        public List<SelectListItem> BatchOptions { get; set; } = new();
        public List<SelectListItem> StatusOptions { get; set; } = new();
    }

    public class EnrollmentFormViewModel
    {
        public Guid? E_Id { get; set; }
        public Guid E_StudentId { get; set; }
        public Guid E_AcademicYearId { get; set; }
        public Guid E_CourseId { get; set; }
        public Guid E_BatchId { get; set; }
        public string E_EnrollmentNumber { get; set; }
        public DateTime E_EnrollmentDate { get; set; } = DateTime.Today;
        public string E_Status { get; set; } = "Active";
        public DateTime? E_CompletionDate { get; set; }

        public List<SelectListItem> StudentOptions { get; set; } = new();
        public List<SelectListItem> AcademicYearOptions { get; set; } = new();
        public List<SelectListItem> CourseOptions { get; set; } = new();
        public List<SelectListItem> BatchOptions { get; set; } = new();
        public List<SelectListItem> StatusOptions { get; set; } = new();
    }

    public class EnrollmentDetailsViewModel
    {
        public Guid E_Id { get; set; }
        public string E_EnrollmentNumber { get; set; }
        public string StudentName { get; set; }
        public string CourseName { get; set; }
        public string BatchName { get; set; }
        public string AcademicYearName { get; set; }
        public DateTime E_EnrollmentDate { get; set; }
        public string E_Status { get; set; }
        public DateTime? E_CompletionDate { get; set; }
        public DateTime E_CreatedAt { get; set; }
        public DateTime E_UpdatedAt { get; set; }
    }
}
