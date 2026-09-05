using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using IMS.DAL.Interfaces;
using IMS.Helpers.Constants;
using IMS.Models.Entities;
using IMS.Models.ViewModels;
using IMS.Services.Interfaces;

namespace IMS.Services
{
    public class TeacherLeaveService : ITeacherLeaveService
    {
        private readonly ITeacherLeaveDAL _repo;
        private readonly ITeacherDirectoryLookup _directory;

        public TeacherLeaveService(ITeacherLeaveDAL repo, ITeacherDirectoryLookup directory)
        {
            _repo = repo;
            _directory = directory;
        }

        public async Task<TeacherLeaveIndexViewModel> GetAllForAdminAsync(Guid tenantId, string status, int page, int pageSize)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize is < 1 or > 100 ? 10 : pageSize;

            var (items, total) = await _repo.GetAllPagedAsync(tenantId, status, page, pageSize);

            return new TeacherLeaveIndexViewModel
            {
                StatusFilter = status,
                PageNumber = page,
                PageSize = pageSize,
                TotalCount = total,
                StatusOptions = new System.Collections.Generic.List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>
                {
                    new() { Value = "Pending", Text = "Pending", Selected = status == "Pending" },
                    new() { Value = "Approved", Text = "Approved", Selected = status == "Approved" },
                    new() { Value = "Rejected", Text = "Rejected", Selected = status == "Rejected" },
                    new() { Value = "Cancelled", Text = "Cancelled", Selected = status == "Cancelled" }
                },
                Requests = items.Select(MapToListItem).ToList()
            };
        }

        public async Task<MyLeaveViewModel> GetMyLeaveAsync(Guid tenantId, Guid teacherId, int page, int pageSize)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize is < 1 or > 100 ? 10 : pageSize;

            var (items, total) = await _repo.GetByTeacherIdAsync(tenantId, teacherId, page, pageSize);

            return new MyLeaveViewModel
            {
                PageNumber = page,
                PageSize = pageSize,
                TotalCount = total,
                LeaveTypeOptions = TeacherHrOptions.GetLeaveTypeSelectList(),
                MyRequests = items.Select(MapToListItem).ToList()
            };
        }

        public async Task<ServiceResult> ApplyAsync(Guid tenantId, Guid teacherId, string accessToken, TeacherLeaveApplyViewModel model)
        {
            if (!model.FromDate.HasValue || !model.ToDate.HasValue)
                return ServiceResult.Fail("From and To dates are required.");

            try
            {
                var teacherName = await _directory.GetTeacherNameAsync(tenantId, teacherId, accessToken);

                // Both Teachers_T FKs (TeacherLeaves_TL and TeacherAttendance_TA)
                // mean a "pending setup" teacher (no local Teachers_T row) can't
                // have a leave request inserted at all - catch this explicitly
                // rather than letting it surface as a raw FK-violation SqlException.
                if (teacherName == null)
                    return ServiceResult.Fail("Your teacher profile setup isn't complete yet. Please contact the administrator.");

                var entity = new TeacherLeave
                {
                    TL_Id = Guid.NewGuid(),
                    TL_TenantId = tenantId,
                    TL_TeacherId = teacherId,
                    TL_TeacherName = teacherName,
                    TL_LeaveType = model.LeaveType,
                    TL_FromDate = model.FromDate.Value.Date,
                    TL_ToDate = model.ToDate.Value.Date,
                    TL_Reason = model.Reason,
                    TL_AppliedBy = teacherId // self-service: applicant == teacher
                };

                var result = await _repo.ApplyAsync(entity);
                return ServiceResult.Ok($"Leave request submitted ({result.TotalDays} day(s)).", result.TL_Id);
            }
            catch (SqlException ex)
            {
                // THROW 51001 / 51002 inside SP_TeacherLeaves_Apply surface here
                // with the exact business-rule message set in T-SQL.
                return ServiceResult.Fail(ex.Message);
            }
            catch (Exception)
            {
                return ServiceResult.Fail("Something went wrong while submitting the leave request. Please try again.");
            }
        }

        public async Task<ServiceResult> ApproveAsync(Guid tenantId, Guid leaveId, Guid approvedBy)
        {
            try
            {
                var success = await _repo.ApproveAsync(leaveId, tenantId, approvedBy);
                return success
                    ? ServiceResult.Ok("Leave request approved.")
                    : ServiceResult.Fail("This request is no longer pending (already actioned or not found).");
            }
            catch (Exception)
            {
                return ServiceResult.Fail("Something went wrong while approving the request. Please try again.");
            }
        }

        public async Task<ServiceResult> RejectAsync(Guid tenantId, Guid leaveId, Guid approvedBy, string rejectionReason)
        {
            if (string.IsNullOrWhiteSpace(rejectionReason))
                return ServiceResult.Fail("A rejection reason is required.");

            try
            {
                var success = await _repo.RejectAsync(leaveId, tenantId, approvedBy, rejectionReason);
                return success
                    ? ServiceResult.Ok("Leave request rejected.")
                    : ServiceResult.Fail("This request is no longer pending (already actioned or not found).");
            }
            catch (Exception)
            {
                return ServiceResult.Fail("Something went wrong while rejecting the request. Please try again.");
            }
        }

        public async Task<ServiceResult> CancelAsync(Guid tenantId, Guid leaveId, Guid requestingTeacherId)
        {
            try
            {
                var success = await _repo.CancelAsync(leaveId, tenantId, requestingTeacherId);
                return success
                    ? ServiceResult.Ok("Leave request cancelled.")
                    : ServiceResult.Fail("This request can no longer be cancelled (already actioned, or it isn't yours).");
            }
            catch (Exception)
            {
                return ServiceResult.Fail("Something went wrong while cancelling the request. Please try again.");
            }
        }

        private static TeacherLeaveListItemViewModel MapToListItem(TeacherLeave l) => new()
        {
            TL_Id = l.TL_Id,
            TeacherName = l.TL_TeacherName,
            LeaveType = l.TL_LeaveType,
            FromDate = l.TL_FromDate,
            ToDate = l.TL_ToDate,
            TotalDays = l.TL_TotalDays,
            Reason = l.TL_Reason,
            Status = l.TL_Status,
            AppliedAt = l.TL_AppliedAt,
            RejectionReason = l.TL_RejectionReason
        };

        public async Task<ServiceResult> UpdateAsync(Guid tenantId, Guid leaveId, Guid requestingTeacherId, TeacherLeaveApplyViewModel model)
        {
            if (!model.FromDate.HasValue || !model.ToDate.HasValue)
                return ServiceResult.Fail("From and To dates are required.");

            try
            {
                var entity = new TeacherLeave
                {
                    TL_Id = leaveId,
                    TL_TenantId = tenantId,
                    TL_LeaveType = model.LeaveType,
                    TL_FromDate = model.FromDate.Value.Date,
                    TL_ToDate = model.ToDate.Value.Date,
                    TL_Reason = model.Reason
                };

                var result = await _repo.UpdateAsync(entity, requestingTeacherId);

                return result.TL_Id != Guid.Empty
                    ? ServiceResult.Ok($"Leave request updated ({result.TotalDays} day(s)).")
                    : ServiceResult.Fail("This request can no longer be edited (already actioned, or it isn't yours).");
            }
            catch (SqlException ex)
            {
                return ServiceResult.Fail(ex.Message);
            }
            catch (Exception)
            {
                return ServiceResult.Fail("Something went wrong while updating the request. Please try again.");
            }
        }
    }
}
