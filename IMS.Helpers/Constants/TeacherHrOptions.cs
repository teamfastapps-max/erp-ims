using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IMS.Helpers.Constants
{
    public static class TeacherHrOptions
    {
        public static readonly List<string> LeaveTypes = new() { "Sick", "Casual", "Earned", "Unpaid" };

        // "OnLeave" intentionally excluded from the manual-select list - it's
        // only ever set automatically by SP_TeacherAttendance_AddEdit when an
        // approved leave covers the date, never chosen directly by an admin.
        public static readonly List<string> AttendanceStatuses = new() { "Present", "Absent", "Late", "HalfDay" };

        public static List<SelectListItem> GetLeaveTypeSelectList(string selected = null) =>
            LeaveTypes.ConvertAll(t => new SelectListItem { Value = t, Text = t, Selected = t == selected });

        public static List<SelectListItem> GetAttendanceStatusSelectList(string selected = null) =>
            AttendanceStatuses.ConvertAll(s => new SelectListItem { Value = s, Text = s, Selected = s == selected });
    }
}
