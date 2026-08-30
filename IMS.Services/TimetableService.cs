using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IMS.DAL.Interfaces;
using IMS.Helpers.Constants;
using IMS.Models.Entities;
using IMS.Models.ViewModels;
using IMS.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IMS.Services
{
    public class TimetableService : ITimetableService
    {
        private readonly ITimetableDAL _repo;
        private readonly IMasterService _masterService;
        public TimetableService(ITimetableDAL repo, IMasterService masterService) { _repo = repo; _masterService = masterService; }

        public async Task<TimetableIndexViewModel> GetListAsync(Guid tenantId, Guid? batchId, Guid? branchId)
        {
            var items = await _repo.GetAllAsync(tenantId, batchId, branchId);
            return new TimetableIndexViewModel
            {
                BatchFilter = batchId,
                BranchFilter = branchId,
                Entries = items.ConvertAll(t => new TimetableListItemViewModel
                {
                    TT_Id = t.TT_Id,
                    TT_DayOfWeek = t.TT_DayOfWeek,
                    TT_StartTime = t.TT_StartTime,
                    TT_EndTime = t.TT_EndTime,
                    SubjectName = t.SubjectName ?? "-",
                    StaffName = t.StaffName ?? "-",
                    ClassroomName = t.ClassroomName ?? "-"
                })
            };
        }

        public async Task<TimetableDetailsViewModel> GetDetailsAsync(Guid id, Guid tenantId)
        {
            var t = await _repo.GetByIdAsync(id, tenantId);
            if (t == null) return null;
            return new TimetableDetailsViewModel
            {
                TT_Id = t.TT_Id,
                DayName = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetDayName((DayOfWeek)t.TT_DayOfWeek),
                TT_StartTime = t.TT_StartTime,
                TT_EndTime = t.TT_EndTime,
                SubjectName = t.SubjectName ?? "-",
                StaffName = t.StaffName ?? "-",
                ClassroomName = t.ClassroomName ?? "-",
                BatchName = t.BatchName ?? "-",
                TT_EffectiveFrom = t.TT_EffectiveFrom,
                TT_EffectiveTo = t.TT_EffectiveTo,
                TT_CreatedAt = t.TT_CreatedAt
            };
        }

        public async Task<TimetableFormViewModel> GetForEditAsync(Guid id, Guid tenantId)
        {
            var t = await _repo.GetByIdAsync(id, tenantId);
            if (t == null) return null;
            return new TimetableFormViewModel
            {
                TT_Id = t.TT_Id,
                TT_BranchId = t.TT_BranchId,
                TT_BatchId = t.TT_BatchId,
                TT_SubjectId = t.TT_SubjectId,
                TT_StaffId = t.TT_StaffId,
                TT_ClassroomId = t.TT_ClassroomId,
                TT_DayOfWeek = t.TT_DayOfWeek,
                TT_StartTime = t.TT_StartTime.ToString(@"hh\:mm"),
                TT_EndTime = t.TT_EndTime.ToString(@"hh\:mm"),
                TT_EffectiveFrom = t.TT_EffectiveFrom,
                TT_EffectiveTo = t.TT_EffectiveTo
            };
        }

        public async Task<ServiceResult> CreateAsync(TimetableFormViewModel model, Guid tenantId)
        {
            if (!TimeSpan.TryParse(model.TT_StartTime, out var start) || !TimeSpan.TryParse(model.TT_EndTime, out var end))
                return ServiceResult.Fail("Invalid time format.");

            if (end <= start)
                return ServiceResult.Fail("End time must be after start time.");

            if (await _repo.HasConflictAsync(tenantId, model.TT_BatchId, model.TT_DayOfWeek, start, end, null))
                return ServiceResult.Fail("Time slot conflicts with an existing timetable entry.");

            var entity = MapToEntity(model, tenantId, Guid.NewGuid(), start, end);
            var id = await _repo.CreateAsync(entity);
            return ServiceResult.Ok("Timetable entry created.", id);
        }

        public async Task<ServiceResult> UpdateAsync(TimetableFormViewModel model, Guid tenantId)
        {
            if (!model.TT_Id.HasValue) return ServiceResult.Fail("Id required.");
            if (!TimeSpan.TryParse(model.TT_StartTime, out var start) || !TimeSpan.TryParse(model.TT_EndTime, out var end))
                return ServiceResult.Fail("Invalid time format.");
            if (end <= start) return ServiceResult.Fail("End time must be after start time.");

            if (await _repo.HasConflictAsync(tenantId, model.TT_BatchId, model.TT_DayOfWeek, start, end, model.TT_Id))
                return ServiceResult.Fail("Time slot conflicts with an existing entry.");

            var entity = MapToEntity(model, tenantId, model.TT_Id.Value, start, end);
            var success = await _repo.UpdateAsync(entity);
            return success ? ServiceResult.Ok("Updated.", model.TT_Id) : ServiceResult.Fail("Not found.");
        }

        public async Task<ServiceResult> DeleteAsync(Guid id, Guid tenantId)
        {
            var success = await _repo.DeleteAsync(id, tenantId);
            return success ? ServiceResult.Ok("Deleted.") : ServiceResult.Fail("Unable to delete.");
        }

        public async Task<bool> CheckConflictAsync(Guid tenantId, Guid batchId, int dayOfWeek, string startTime, string endTime, Guid? excludeId)
        {
            if (!TimeSpan.TryParse(startTime, out var start) || !TimeSpan.TryParse(endTime, out var end)) return false;
            return await _repo.HasConflictAsync(tenantId, batchId, dayOfWeek, start, end, excludeId);
        }

        private static Timetable MapToEntity(TimetableFormViewModel m, Guid tenantId, Guid id, TimeSpan start, TimeSpan end) => new()
        {
            TT_Id = id,
            TT_TenantId = tenantId,
            TT_BranchId = m.TT_BranchId,
            TT_BatchId = m.TT_BatchId,
            TT_SubjectId = m.TT_SubjectId,
            TT_StaffId = m.TT_StaffId,
            TT_ClassroomId = m.TT_ClassroomId,
            TT_DayOfWeek = m.TT_DayOfWeek,
            TT_StartTime = start,
            TT_EndTime = end,
            TT_EffectiveFrom = m.TT_EffectiveFrom,
            TT_EffectiveTo = m.TT_EffectiveTo
        };

        public void PopulateDropdowns(TimetableFormViewModel vm)
        {
            vm.BranchOptions = HardcodedMasterData.GetBranchSelectList(vm.TT_BranchId);
            vm.BatchOptions = GetMasterSelectList("Batch", vm.TT_BatchId.ToString());
            vm.SubjectOptions = GetMasterSelectList("Subject", vm.TT_SubjectId.ToString());
            vm.StaffOptions = GetMasterSelectList("Staff", vm.TT_StaffId.ToString());
            vm.ClassroomOptions = GetMasterSelectList("Classroom", vm.TT_ClassroomId?.ToString());
            vm.DayOfWeekOptions = GetDayOfWeekSelectList(vm.TT_DayOfWeek);
        }

        private List<SelectListItem> GetMasterSelectList(string entityType, string selectedValue = null)
        {
            var items = _masterService.GetAll(entityType);
            var list = new List<SelectListItem>();
            if (items == null) return list;
            foreach (var item in items)
            {
                var keyEntry = item.FirstOrDefault(kvp => kvp.Key.EndsWith("_Id"));
                var id = keyEntry.Value?.ToString() ?? "";

                string displayName = null;
                var nameEntry = item.FirstOrDefault(kvp => kvp.Key.EndsWith("_Name"));
                if (nameEntry.Value != null) displayName = nameEntry.Value.ToString();

                if (string.IsNullOrEmpty(displayName))
                {
                    var firstName = item.FirstOrDefault(kvp => kvp.Key.EndsWith("_FirstName")).Value?.ToString() ?? "";
                    var lastName = item.FirstOrDefault(kvp => kvp.Key.EndsWith("_LastName")).Value?.ToString() ?? "";
                    displayName = $"{firstName} {lastName}".Trim();
                }

                if (string.IsNullOrEmpty(displayName))
                    displayName = item.Values.ElementAtOrDefault(1)?.ToString() ?? id;

                list.Add(new SelectListItem { Value = id, Text = displayName, Selected = id == selectedValue });
            }
            return list;
        }

        private static List<SelectListItem> GetDayOfWeekSelectList(int selected = 0)
        {
            var days = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
            return days.Select((d, i) => new SelectListItem { Value = i.ToString(), Text = d, Selected = i == selected }).ToList();
        }
    }
}
