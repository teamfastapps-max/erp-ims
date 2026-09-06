using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using IMS.DAL.Interfaces;
using IMS.Models.Portal;
using IMS.Services.Interfaces;

namespace IMS.Services
{
    public class StudentLeaveService : IStudentLeaveService
    {
        private static readonly string[] AllowedReviewStatuses = { "Approved", "Rejected" };
        private readonly IStudentLeaveDAL _dal;
        private readonly ILogger<StudentLeaveService> _logger;

        public StudentLeaveService(IStudentLeaveDAL dal, ILogger<StudentLeaveService> logger)
        {
            _dal = dal;
            _logger = logger;
        }

        public async Task<(List<StudentLeaveDto> Items, int TotalCount)> GetPagedAsync(Guid tenantId, string? status, string? search, int pageNumber, int pageSize)
        {
            try
            {
                return await _dal.GetPagedAsync(tenantId, status, search, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged student leaves for tenant {TenantId}", tenantId);
                return (new List<StudentLeaveDto>(), 0);
            }
        }

        public async Task<ServiceResult> ReviewAsync(Guid leaveId, Guid tenantId, Guid approvedBy, string status, string? rejectionReason)
        {
            if (Array.IndexOf(AllowedReviewStatuses, status) < 0)
                return ServiceResult.Fail("Invalid status.");

            if (status == "Rejected" && string.IsNullOrWhiteSpace(rejectionReason))
                return ServiceResult.Fail("A rejection reason is required.");

            try
            {
                var success = await _dal.ReviewAsync(leaveId, tenantId, approvedBy, status, rejectionReason);
                return success
                    ? ServiceResult.Ok($"Leave application {status.ToLowerInvariant()} successfully.", leaveId)
                    : ServiceResult.Fail("This request is no longer pending (already actioned or not found).");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reviewing student leave {LeaveId}", leaveId);
                return ServiceResult.Fail("Failed to update leave status. Please try again.");
            }
        }

        public async Task<ServiceResult> DeleteAsync(Guid leaveId, Guid tenantId)
        {
            try
            {
                var success = await _dal.DeleteAsync(leaveId, tenantId);
                return success
                    ? ServiceResult.Ok("Leave application deleted successfully.", leaveId)
                    : ServiceResult.Fail("Leave application not found or could not be deleted.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting student leave {LeaveId}", leaveId);
                return ServiceResult.Fail("Failed to delete leave application.");
            }
        }
    }
}
