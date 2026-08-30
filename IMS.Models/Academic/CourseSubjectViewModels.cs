using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IMS.Models.ViewModels
{
    public class CourseSubjectListItemViewModel
    {
        public Guid CS_CourseId { get; set; }
        public Guid CS_SubjectId { get; set; }
        public string CourseName { get; set; }
        public string SubjectName { get; set; }
        public int CS_SequenceNo { get; set; }
        public bool CS_IsMandatory { get; set; }
        public decimal? CS_MaxMarks { get; set; }
        public decimal? CS_PassMarks { get; set; }
    }

    public class CourseSubjectIndexViewModel
    {
        public List<CourseSubjectListItemViewModel> Items { get; set; } = new();
        public Guid? CourseFilter { get; set; }
        public List<SelectListItem> CourseOptions { get; set; } = new();
    }

    public class CourseSubjectFormViewModel
    {
        public Guid CS_CourseId { get; set; }
        public Guid CS_SubjectId { get; set; }
        public int CS_SequenceNo { get; set; }
        public bool CS_IsMandatory { get; set; } = true;
        public decimal? CS_MaxMarks { get; set; }
        public decimal? CS_PassMarks { get; set; }

        public List<SelectListItem> CourseOptions { get; set; } = new();
        public List<SelectListItem> SubjectOptions { get; set; } = new();
    }
}
