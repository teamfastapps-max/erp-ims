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
    public class AttendanceService : IAttendanceService
    {
        private readonly IAttendanceSessionDAL _sessionDAL;
        private readonly IAttendanceRecordDAL _recordDAL;
        private readonly IMasterService _masterService;

        public AttendanceService(IAttendanceSessionDAL sessionDAL, IAttendanceRecordDAL recordDAL, IMasterService masterService)
        {
            _sessionDAL = sessionDAL;
            _recordDAL = recordDAL;
            _masterService = masterService;
        }

        public async Task<AttendanceSessionIndexViewModel> GetSessionListAsync(
            Guid tenantId, string searchTerm, Guid? branchId, Guid? batchId,
            DateTime? date, int pageNumber, int pageSize)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize is < 1 or > 100 ? 10 : pageSize;

            var (items, totalCount) = await _sessionDAL.GetPagedAsync(
                tenantId, searchTerm, branchId, batchId, date, pageNumber, pageSize);

            var vm = new AttendanceSessionIndexViewModel
            {
                SearchTerm = searchTerm,
                BranchFilter = branchId,
                BatchFilter = batchId,
                DateFilter = date,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                BranchOptions = HardcodedMasterData.GetBranchSelectList(branchId),
                BatchOptions = GetMasterSelectList("Batch", batchId?.ToString())
            };

            foreach (var s in items)
            {
                var records = await _recordDAL.GetBySessionIdAsync(s.AS_Id);
                vm.Sessions.Add(new AttendanceSessionListItemViewModel
                {
                    AS_Id = s.AS_Id,
                    BatchName = s.BatchName ?? "-",
                    SubjectName = s.SubjectName ?? "-",
                    StaffName = s.StaffName ?? "-",
                    AS_AttendanceDate = s.AS_AttendanceDate,
                    AS_StartTime = s.AS_StartTime?.ToString(@"hh\:mm") ?? "-",
                    AS_EndTime = s.AS_EndTime?.ToString(@"hh\:mm") ?? "-",
                    PresentCount = records.Count(r => r.AR_Status == "present"),
                    AbsentCount = records.Count(r => r.AR_Status == "absent"),
                    TotalStudents = records.Count
                });
            }

            return vm;
        }

        public async Task<AttendanceSessionFormViewModel> GetSessionForEditAsync(Guid id, Guid tenantId)
        {
            var s = await _sessionDAL.GetByIdAsync(id, tenantId);
            if (s == null) return null;

            var vm = new AttendanceSessionFormViewModel
            {
                AS_Id = s.AS_Id,
                AS_BranchId = s.AS_BranchId,
                AS_BatchId = s.AS_BatchId,
                AS_SubjectId = s.AS_SubjectId,
                AS_StaffId = s.AS_StaffId,
                AS_AttendanceDate = s.AS_AttendanceDate,
                AS_StartTime = s.AS_StartTime,
                AS_EndTime = s.AS_EndTime,
                AS_Remarks = s.AS_Remarks
            };

            PopulateSessionDropdowns(vm);
            return vm;
        }

        public async Task<ServiceResult> CreateSessionAsync(AttendanceSessionFormViewModel model, Guid tenantId)
        {
            var entity = MapToEntity(model, tenantId, Guid.NewGuid());
            var id = await _sessionDAL.CreateAsync(entity);
            return ServiceResult.Ok("Attendance session created successfully.", id);
        }

        public async Task<ServiceResult> UpdateSessionAsync(AttendanceSessionFormViewModel model, Guid tenantId)
        {
            if (!model.AS_Id.HasValue)
                return ServiceResult.Fail("Session Id is required for update.");

            var entity = MapToEntity(model, tenantId, model.AS_Id.Value);
            var success = await _sessionDAL.UpdateAsync(entity);
            return success
                ? ServiceResult.Ok("Attendance session updated successfully.", model.AS_Id)
                : ServiceResult.Fail("Session not found.");
        }

        public async Task<ServiceResult> DeleteSessionAsync(Guid id, Guid tenantId)
        {
            var success = await _sessionDAL.DeleteAsync(id, tenantId);
            return success
                ? ServiceResult.Ok("Attendance session deleted successfully.")
                : ServiceResult.Fail("Unable to delete session.");
        }

        public async Task<AttendanceMarkViewModel> GetMarkAttendanceAsync(Guid sessionId, Guid tenantId)
        {
            var session = await _sessionDAL.GetByIdAsync(sessionId, tenantId);
            if (session == null) return null;

            var existingRecords = await _recordDAL.GetBySessionIdAsync(sessionId);
            var existingDict = existingRecords.ToDictionary(r => r.AR_StudentId, r => r);

            var students = _masterService.GetAll("Student") ?? new List<Dictionary<string, object>>();
            var batchStudentItems = _masterService.GetAll("Student");

            var vm = new AttendanceMarkViewModel
            {
                SessionId = sessionId,
                BatchName = session.BatchName ?? "-",
                SubjectName = session.SubjectName ?? "-",
                AttendanceDate = session.AS_AttendanceDate
            };

            foreach (var student in batchStudentItems)
            {
                var idEntry = student.FirstOrDefault(kvp => kvp.Key.EndsWith("_Id"));
                if (idEntry.Value == null) continue;
                var studentId = Guid.Parse(idEntry.Value.ToString());

                var firstName = student.FirstOrDefault(kvp => kvp.Key.EndsWith("_FirstName")).Value?.ToString() ?? "";
                var lastName = student.FirstOrDefault(kvp => kvp.Key.EndsWith("_LastName")).Value?.ToString() ?? "";
                var studentName = $"{firstName} {lastName}".Trim();
                var studentCode = student.FirstOrDefault(kvp => kvp.Key.EndsWith("_StudentCode")).Value?.ToString() ?? "";

                existingDict.TryGetValue(studentId, out var existing);

                vm.Students.Add(new AttendanceStudentItemViewModel
                {
                    StudentId = studentId,
                    StudentCode = studentCode,
                    StudentName = studentName,
                    Status = existing?.AR_Status ?? "present",
                    Remarks = existing?.AR_Remarks
                });
            }

            return vm;
        }

        public async Task<ServiceResult> SaveAttendanceAsync(AttendanceRecordSaveViewModel model, Guid tenantId)
        {
            var records = model.Records.Select(r => new AttendanceRecord
            {
                AR_Id = Guid.NewGuid(),
                AR_AttendanceSessionId = model.SessionId,
                AR_StudentId = r.StudentId,
                AR_Status = r.Status,
                AR_Remarks = r.Remarks
            }).ToList();

            var success = await _recordDAL.SaveRecordsAsync(model.SessionId, records);
            return success
                ? ServiceResult.Ok("Attendance saved successfully.")
                : ServiceResult.Fail("Failed to save attendance.");
        }

        public void PopulateSessionDropdowns(AttendanceSessionFormViewModel vm)
        {
            vm.BranchOptions = HardcodedMasterData.GetBranchSelectList(vm.AS_BranchId);
            vm.BatchOptions = GetMasterSelectList("Batch", vm.AS_BatchId.ToString());
            vm.SubjectOptions = GetMasterSelectList("Subject", vm.AS_SubjectId?.ToString());
            vm.StaffOptions = GetMasterSelectList("Staff", vm.AS_StaffId?.ToString());
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

        private static AttendanceSession MapToEntity(AttendanceSessionFormViewModel m, Guid tenantId, Guid id) => new()
        {
            AS_Id = id,
            AS_TenantId = tenantId,
            AS_BranchId = m.AS_BranchId,
            AS_BatchId = m.AS_BatchId,
            AS_SubjectId = m.AS_SubjectId,
            AS_StaffId = m.AS_StaffId,
            AS_AttendanceDate = m.AS_AttendanceDate,
            AS_StartTime = m.AS_StartTime,
            AS_EndTime = m.AS_EndTime,
            AS_Remarks = m.AS_Remarks
        };
    }
}
