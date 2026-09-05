using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IMS.Models.Entities;

namespace IMS.DAL.Interfaces
{
    public class LeaveApplyResult
    {
        public Guid TL_Id { get; set; }
        public int TotalDays { get; set; }
    }

    public interface ITeacherLeaveDAL
    {
        Task<LeaveApplyResult> ApplyAsync(TeacherLeave leave);
        Task<bool> ApproveAsync(Guid id, Guid tenantId, Guid approvedBy);
        Task<bool> RejectAsync(Guid id, Guid tenantId, Guid approvedBy, string rejectionReason);
        Task<bool> CancelAsync(Guid id, Guid tenantId, Guid requestingTeacherId);
        Task<(List<TeacherLeave> Items, int TotalCount)> GetAllPagedAsync(Guid tenantId, string status, int pageNumber, int pageSize);
        Task<(List<TeacherLeave> Items, int TotalCount)> GetByTeacherIdAsync(Guid tenantId, Guid teacherId, int pageNumber, int pageSize);
        Task<LeaveApplyResult> UpdateAsync(TeacherLeave leave, Guid requestingTeacherId);
    }
}
