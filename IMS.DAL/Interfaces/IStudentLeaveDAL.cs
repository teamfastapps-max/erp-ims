using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IMS.Models.Portal;

namespace IMS.DAL.Interfaces
{
    public interface IStudentLeaveDAL
    {
        Task<(List<StudentLeaveDto> Items, int TotalCount)> GetPagedAsync(Guid tenantId, string? status, string? search, int pageNumber, int pageSize);
        Task<bool> ReviewAsync(Guid leaveId, Guid tenantId, Guid approvedBy, string status, string? rejectionReason);
        Task<bool> DeleteAsync(Guid leaveId, Guid tenantId);
    }
}
