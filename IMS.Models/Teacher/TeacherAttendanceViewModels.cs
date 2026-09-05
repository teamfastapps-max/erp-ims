using System;
using System.Collections.Generic;

namespace IMS.Models.ViewModels
{
    /// <summary>One row in the admin bulk "Mark Attendance" grid for a given date.</summary>
    public class TeacherAttendanceRowViewModel
    {
        public Guid TeacherId { get; set; }
        public string TeacherName { get; set; }
        public string CurrentStatus { get; set; }  // null/empty = not marked yet for this date
        public string Remarks { get; set; }
        public bool IsOnLeaveToday { get; set; }    // informational - UI can show a badge even before marking
    }

    public class TeacherAttendanceMarkTodayViewModel
    {
        public DateTime SelectedDate { get; set; } = DateTime.Today;
        public List<TeacherAttendanceRowViewModel> Rows { get; set; } = new();
    }

    public class TeacherAttendanceHistoryItemViewModel
    {
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public string Remarks { get; set; }
    }

    /// <summary>Self-service, read-only view of the current user's own attendance.</summary>
    public class TeacherAttendanceViewModel
    {
        public DateTime FromDate { get; set; } = DateTime.Today.AddDays(-30);
        public DateTime ToDate { get; set; } = DateTime.Today;
        public List<TeacherAttendanceHistoryItemViewModel> History { get; set; } = new();
        public string TodayStatus { get; set; }  
        public string TodayRemarks { get; set; }
    }
}
