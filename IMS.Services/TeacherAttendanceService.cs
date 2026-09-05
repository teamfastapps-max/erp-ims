using System;
using System.Linq;
using System.Threading.Tasks;
using IMS.DAL.Interfaces;
using IMS.Models.Entities;
using IMS.Models.ViewModels;
using IMS.Services.Interfaces;

namespace IMS.Services
{
    public class TeacherAttendanceService : ITeacherAttendanceService
    {
        private static readonly string[] SelfMarkAllowedStatuses = { "Present", "Late", "HalfDay" };

        private readonly ITeacherAttendanceDAL _repo;
        private readonly ITeacherLeaveDAL _leaveRepo;
        private readonly ITeacherDirectoryLookup _directory;

        public TeacherAttendanceService(
            ITeacherAttendanceDAL repo,
            ITeacherLeaveDAL leaveRepo,
            ITeacherDirectoryLookup directory)
        {
            _repo = repo;
            _leaveRepo = leaveRepo;
            _directory = directory;
        }

        public async Task<TeacherAttendanceMarkTodayViewModel> GetMarkGridAsync(Guid tenantId, string accessToken, DateTime date)
        {
            date = date.Date;

            var roster = await _directory.GetActiveTeachersAsync(tenantId, accessToken);
            var existingRecords = await _repo.GetByDateAsync(tenantId, date);
            var existingByTeacher = existingRecords.ToDictionary(r => r.TA_TeacherId, r => r);

            var vm = new TeacherAttendanceMarkTodayViewModel { SelectedDate = date };

            foreach (var teacher in roster.OrderBy(t => t.FullName))
            {
                existingByTeacher.TryGetValue(teacher.Id, out var existing);

                vm.Rows.Add(new TeacherAttendanceRowViewModel
                {
                    TeacherId = teacher.Id,
                    TeacherName = teacher.FullName,
                    CurrentStatus = existing?.TA_Status,
                    Remarks = existing?.TA_Remarks,
                    IsOnLeaveToday = existing?.TA_Status == "OnLeave"
                });
            }

            return vm;
        }

        public async Task<ServiceResult> MarkTeacherAttendanceAsync(Guid tenantId, Guid teacherId, DateTime date, string status, string remarks, Guid markedBy)
        {
            if (string.IsNullOrWhiteSpace(status))
                return ServiceResult.Fail("Status is required.");

            try
            {
                var entity = new TeacherAttendance
                {
                    TA_Id = Guid.NewGuid(),
                    TA_TenantId = tenantId,
                    TA_TeacherId = teacherId,
                    TA_Date = date.Date,
                    TA_Status = status,
                    TA_Remarks = remarks,
                    TA_MarkedBy = markedBy
                };

                var result = await _repo.UpsertAsync(entity);

                var message = result.WasOverriddenToOnLeave
                    ? $"Marked as On Leave - this teacher has an approved leave covering {date:dd-MMM-yyyy}."
                    : "Attendance saved.";

                return ServiceResult.Ok(message);
            }
            catch (Exception)
            {
                return ServiceResult.Fail("Something went wrong while saving attendance. Please try again.");
            }
        }

        public async Task<ServiceResult> MarkTeacherSelfAttendanceAsync(Guid tenantId, Guid teacherId, string status, string remarks)
        {
            if (!SelfMarkAllowedStatuses.Contains(status))
                return ServiceResult.Fail("Select Present, Late, or Half-day.");

            // Date and markedBy are both hardcoded here, never taken from the
            // client - self-marking is only ever "today, by yourself."
            return await MarkTeacherAttendanceAsync(tenantId, teacherId, DateTime.Today, status, remarks, markedBy: teacherId);
        }

        public async Task<TeacherAttendanceViewModel> GetTeacherAttendanceAsync(Guid tenantId, Guid teacherId, DateTime fromDate, DateTime toDate)
        {
            var records = await _repo.GetByTeacherAndRangeAsync(tenantId, teacherId, fromDate, toDate);

            // Looked up independently of fromDate/toDate so the "Mark Today"
            // widget is accurate even if the person filtered history to a
            // range that doesn't include today.
            var todayRecords = await _repo.GetByTeacherAndRangeAsync(tenantId, teacherId, DateTime.Today, DateTime.Today);
            var today = todayRecords.FirstOrDefault();

            return new TeacherAttendanceViewModel
            {
                FromDate = fromDate,
                ToDate = toDate,
                TodayStatus = today?.TA_Status,
                TodayRemarks = today?.TA_Remarks,
                History = records.Select(r => new TeacherAttendanceHistoryItemViewModel
                {
                    Date = r.TA_Date,
                    Status = r.TA_Status,
                    Remarks = r.TA_Remarks
                }).ToList()
            };
        }
    }
}
