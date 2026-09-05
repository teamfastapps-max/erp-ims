using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IMS.Models.Entities;

namespace IMS.DAL.Interfaces
{
    public class AttendanceUpsertResult
    {
        public string FinalStatus { get; set; }
        public bool WasOverriddenToOnLeave { get; set; }
    }

    public interface ITeacherAttendanceDAL
    {
        Task<AttendanceUpsertResult> UpsertAsync(TeacherAttendance attendance);
        Task<List<TeacherAttendance>> GetByDateAsync(Guid tenantId, DateTime date);
        Task<List<TeacherAttendance>> GetByTeacherAndRangeAsync(Guid tenantId, Guid teacherId, DateTime fromDate, DateTime toDate);
    }
}
