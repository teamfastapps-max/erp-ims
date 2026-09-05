using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IMS.Models.ViewModels
{
    public class TeacherLeaveListItemViewModel
    {
        public Guid TL_Id { get; set; }
        public string TeacherName { get; set; }
        public string LeaveType { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TotalDays { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }
        public DateTime AppliedAt { get; set; }
        public string RejectionReason { get; set; }
    }

    /// <summary>Admin view: all teachers' requests, paged, filterable by status.</summary>
    public class TeacherLeaveIndexViewModel
    {
        public List<TeacherLeaveListItemViewModel> Requests { get; set; } = new();
        public string StatusFilter { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public List<SelectListItem> StatusOptions { get; set; } = new();
    }

    /// <summary>Self-service view: current user's own requests + the Apply form.</summary>
    public class MyLeaveViewModel
    {
        public List<TeacherLeaveListItemViewModel> MyRequests { get; set; } = new();
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public List<SelectListItem> LeaveTypeOptions { get; set; } = new();
    }

    /// <summary>Apply form payload (AJAX). No DataAnnotations - validated in teacherLeave.js.</summary>
    public class TeacherLeaveApplyViewModel
    {
        public string LeaveType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string Reason { get; set; }
    }
}
