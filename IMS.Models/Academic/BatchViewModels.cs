using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IMS.Models.ViewModels
{
    public class BatchListItemViewModel
    {
        public Guid BT_Id { get; set; }
        public string BT_Name { get; set; }
        public string BT_Code { get; set; }
        public string BranchName { get; set; }
        public string CourseName { get; set; }
        public string AcademicYearName { get; set; }
        public DateTime BT_StartDate { get; set; }
        public DateTime? BT_EndDate { get; set; }
        public int? BT_Capacity { get; set; }
        public int EnrolledCount { get; set; }
        public string BT_Status { get; set; }
    }

    public class BatchIndexViewModel
    {
        public List<BatchListItemViewModel> Batches { get; set; } = new();
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

    public class BatchFormViewModel
    {
        public Guid? BT_Id { get; set; }
        public Guid BT_BranchId { get; set; }
        public Guid BT_CourseId { get; set; }
        public Guid BT_AcademicYearId { get; set; }
        public string BT_Name { get; set; }
        public string BT_Code { get; set; }
        public DateTime BT_StartDate { get; set; } = DateTime.Today;
        public DateTime? BT_EndDate { get; set; }
        public int? BT_Capacity { get; set; }
        public string BT_Status { get; set; } = "Active";

        public List<SelectListItem> BranchOptions { get; set; } = new();
        public List<SelectListItem> CourseOptions { get; set; } = new();
        public List<SelectListItem> AcademicYearOptions { get; set; } = new();
        public List<SelectListItem> StatusOptions { get; set; } = new();
    }

    public class BatchDetailsViewModel
    {
        public Guid BT_Id { get; set; }
        public string BT_Name { get; set; }
        public string BT_Code { get; set; }
        public string BranchName { get; set; }
        public string CourseName { get; set; }
        public string AcademicYearName { get; set; }
        public DateTime BT_StartDate { get; set; }
        public DateTime? BT_EndDate { get; set; }
        public int? BT_Capacity { get; set; }
        public int EnrolledCount { get; set; }
        public string BT_Status { get; set; }
        public DateTime BT_CreatedAt { get; set; }
        public DateTime BT_UpdatedAt { get; set; }
    }
}
