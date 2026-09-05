using System;
using System.Threading.Tasks;
using IMS.Models.ViewModels;

namespace IMS.Services.Interfaces
{
    public interface ITeacherAttendanceService
    {
        /// <summary>Admin bulk-mark grid: full active-teacher roster merged with that date's existing records.</summary>
        Task<TeacherAttendanceMarkTodayViewModel> GetMarkGridAsync(Guid tenantId, string accessToken, DateTime date);

        Task<ServiceResult> MarkTeacherAttendanceAsync(Guid tenantId, Guid teacherId, DateTime date, string status, string remarks, Guid markedBy);
        Task<ServiceResult> MarkTeacherSelfAttendanceAsync(Guid tenantId, Guid teacherId, string status, string remarks);

        Task<TeacherAttendanceViewModel> GetTeacherAttendanceAsync(Guid tenantId, Guid teacherId, DateTime fromDate, DateTime toDate);
    }
}
