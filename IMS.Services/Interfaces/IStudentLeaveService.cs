using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IMS.Models.Portal;

namespace IMS.Services.Interfaces
{
    public interface IStudentLeaveService
    {
        Task<(List<StudentLeaveDto> Items, int TotalCount)> GetPagedAsync(Guid tenantId, string? status, string? search, int pageNumber, int pageSize);
        Task<ServiceResult> ReviewAsync(Guid leaveId, Guid tenantId, Guid approvedBy, string status, string? rejectionReason);
        Task<ServiceResult> DeleteAsync(Guid leaveId, Guid tenantId);
    }
}
