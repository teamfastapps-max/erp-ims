using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IMS.Models.ViewModels
{
    public class ExamListItemViewModel
    {
        public Guid EX_Id { get; set; }
        public string EX_Code { get; set; }
        public string EX_Name { get; set; }
        public string ExamTypeName { get; set; }
        public string CourseName { get; set; }
        public string BatchName { get; set; }
        public string AcademicYearName { get; set; }
        public DateTime EX_StartDate { get; set; }
        public DateTime EX_EndDate { get; set; }
        public string EX_Status { get; set; }
    }

    public class ExamIndexViewModel
    {
        public List<ExamListItemViewModel> Exams { get; set; } = new();
        public string SearchTerm { get; set; }
        public Guid? CourseFilter { get; set; }
        public Guid? BatchFilter { get; set; }
        public string StatusFilter { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public List<SelectListItem> CourseOptions { get; set; } = new();
        public List<SelectListItem> BatchOptions { get; set; } = new();
        public List<SelectListItem> StatusOptions { get; set; } = new();
    }

    public class ExamFormViewModel
    {
        public Guid? EX_Id { get; set; }
        public Guid EX_AcademicYearId { get; set; }
        public Guid EX_CourseId { get; set; }
        public Guid EX_BatchId { get; set; }
        public Guid EX_ExamTypeId { get; set; }
        public string EX_Name { get; set; }
        public string EX_Code { get; set; }
        public DateTime EX_StartDate { get; set; } = DateTime.Today;
        public DateTime EX_EndDate { get; set; } = DateTime.Today;
        public string EX_Status { get; set; } = "Draft";

        public List<SelectListItem> AcademicYearOptions { get; set; } = new();
        public List<SelectListItem> CourseOptions { get; set; } = new();
        public List<SelectListItem> BatchOptions { get; set; } = new();
        public List<SelectListItem> ExamTypeOptions { get; set; } = new();
        public List<SelectListItem> StatusOptions { get; set; } = new();
    }

    public class ExamDetailsViewModel
    {
        public Guid EX_Id { get; set; }
        public string EX_Code { get; set; }
        public string EX_Name { get; set; }
        public string ExamTypeName { get; set; }
        public string CourseName { get; set; }
        public string BatchName { get; set; }
        public string AcademicYearName { get; set; }
        public DateTime EX_StartDate { get; set; }
        public DateTime EX_EndDate { get; set; }
        public string EX_Status { get; set; }
        public DateTime EX_CreatedAt { get; set; }
        public DateTime EX_UpdatedAt { get; set; }
        public List<ExamSubjectViewModel> Subjects { get; set; } = new();
    }

    public class ExamSubjectViewModel
    {
        public Guid? ES_Id { get; set; }
        public Guid ES_SubjectId { get; set; }
        public decimal ES_MaxMarks { get; set; }
        public decimal ES_PassMarks { get; set; }
        public decimal? ES_Weightage { get; set; }

        public List<SelectListItem> SubjectOptions { get; set; } = new();
    }

    public class MarksEntryViewModel
    {
        public Guid ExamId { get; set; }
        public string ExamName { get; set; }
        public string ExamCode { get; set; }
        public List<ExamSubjectEntryViewModel> Subjects { get; set; } = new();
    }

    public class ExamSubjectEntryViewModel
    {
        public Guid ExamSubjectId { get; set; }
        public string SubjectName { get; set; }
        public decimal MaxMarks { get; set; }
        public decimal PassMarks { get; set; }
        public List<StudentMarkEntryViewModel> Students { get; set; } = new();
    }

    public class StudentMarkEntryViewModel
    {
        public Guid StudentId { get; set; }
        public string StudentCode { get; set; }
        public string StudentName { get; set; }
        public decimal? MarksObtained { get; set; }
        public string Remarks { get; set; }
    }
}
