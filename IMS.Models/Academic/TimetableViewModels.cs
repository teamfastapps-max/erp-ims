using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IMS.Models.ViewModels
{
    public class TimetableListItemViewModel
    {
        public Guid TT_Id { get; set; }
        public int TT_DayOfWeek { get; set; }
        public string DayName => System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetDayName((DayOfWeek)TT_DayOfWeek);
        public TimeSpan TT_StartTime { get; set; }
        public TimeSpan TT_EndTime { get; set; }
        public string SubjectName { get; set; }
        public string StaffName { get; set; }
        public string ClassroomName { get; set; }
    }

    public class TimetableIndexViewModel
    {
        public List<TimetableListItemViewModel> Entries { get; set; } = new();
        public Guid? BatchFilter { get; set; }
        public Guid? BranchFilter { get; set; }
        public List<SelectListItem> BatchOptions { get; set; } = new();
        public List<SelectListItem> BranchOptions { get; set; } = new();
    }

    public class TimetableFormViewModel
    {
        public Guid? TT_Id { get; set; }
        public Guid TT_BranchId { get; set; }
        public Guid TT_BatchId { get; set; }
        public Guid TT_SubjectId { get; set; }
        public Guid TT_StaffId { get; set; }
        public Guid? TT_ClassroomId { get; set; }
        public int TT_DayOfWeek { get; set; }
        public string TT_StartTime { get; set; }
        public string TT_EndTime { get; set; }
        public DateTime? TT_EffectiveFrom { get; set; }
        public DateTime? TT_EffectiveTo { get; set; }

        public List<SelectListItem> BranchOptions { get; set; } = new();
        public List<SelectListItem> BatchOptions { get; set; } = new();
        public List<SelectListItem> SubjectOptions { get; set; } = new();
        public List<SelectListItem> StaffOptions { get; set; } = new();
        public List<SelectListItem> ClassroomOptions { get; set; } = new();
        public List<SelectListItem> DayOfWeekOptions { get; set; } = new();
    }

    public class TimetableDetailsViewModel
    {
        public Guid TT_Id { get; set; }
        public string DayName { get; set; }
        public TimeSpan TT_StartTime { get; set; }
        public TimeSpan TT_EndTime { get; set; }
        public string SubjectName { get; set; }
        public string StaffName { get; set; }
        public string ClassroomName { get; set; }
        public string BatchName { get; set; }
        public DateTime? TT_EffectiveFrom { get; set; }
        public DateTime? TT_EffectiveTo { get; set; }
        public DateTime TT_CreatedAt { get; set; }
    }
}
