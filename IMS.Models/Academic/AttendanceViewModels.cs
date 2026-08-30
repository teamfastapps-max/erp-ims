using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IMS.Models.ViewModels
{
    public class AttendanceSessionListItemViewModel
    {
        public Guid AS_Id { get; set; }
        public string BatchName { get; set; }
        public string SubjectName { get; set; }
        public string StaffName { get; set; }
        public DateTime AS_AttendanceDate { get; set; }
        public string AS_StartTime { get; set; }
        public string AS_EndTime { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int TotalStudents { get; set; }
    }

    public class AttendanceSessionIndexViewModel
    {
        public List<AttendanceSessionListItemViewModel> Sessions { get; set; } = new();
        public string SearchTerm { get; set; }
        public Guid? BranchFilter { get; set; }
        public Guid? BatchFilter { get; set; }
        public DateTime? DateFilter { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public List<SelectListItem> BranchOptions { get; set; } = new();
        public List<SelectListItem> BatchOptions { get; set; } = new();
    }

    public class AttendanceSessionFormViewModel
    {
        public Guid? AS_Id { get; set; }
        public Guid AS_BranchId { get; set; }
        public Guid AS_BatchId { get; set; }
        public Guid? AS_SubjectId { get; set; }
        public Guid? AS_StaffId { get; set; }
        public DateTime AS_AttendanceDate { get; set; } = DateTime.Today;
        public TimeSpan? AS_StartTime { get; set; }
        public TimeSpan? AS_EndTime { get; set; }
        public string AS_Remarks { get; set; }

        public List<SelectListItem> BranchOptions { get; set; } = new();
        public List<SelectListItem> BatchOptions { get; set; } = new();
        public List<SelectListItem> SubjectOptions { get; set; } = new();
        public List<SelectListItem> StaffOptions { get; set; } = new();
    }

    public class AttendanceMarkViewModel
    {
        public Guid SessionId { get; set; }
        public string BatchName { get; set; }
        public string SubjectName { get; set; }
        public DateTime AttendanceDate { get; set; }
        public List<AttendanceStudentItemViewModel> Students { get; set; } = new();
    }

    public class AttendanceStudentItemViewModel
    {
        public Guid StudentId { get; set; }
        public string StudentCode { get; set; }
        public string StudentName { get; set; }
        public string Status { get; set; } = "present";
        public string Remarks { get; set; }
    }

    public class AttendanceRecordSaveViewModel
    {
        public Guid SessionId { get; set; }
        public List<AttendanceRecordItemViewModel> Records { get; set; } = new();
    }

    public class AttendanceRecordItemViewModel
    {
        public Guid StudentId { get; set; }
        public string Status { get; set; }
        public string Remarks { get; set; }
    }
}
