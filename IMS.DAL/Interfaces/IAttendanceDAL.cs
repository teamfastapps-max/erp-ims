using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IMS.Models.Entities;

namespace IMS.DAL.Interfaces
{
    public interface IAttendanceSessionDAL
    {
        Task<AttendanceSession> GetByIdAsync(Guid id, Guid tenantId);
        Task<(List<AttendanceSession> Items, int TotalCount)> GetPagedAsync(
            Guid tenantId, string searchTerm, Guid? branchId, Guid? batchId,
            DateTime? date, int pageNumber, int pageSize);
        Task<Guid> CreateAsync(AttendanceSession session);
        Task<bool> UpdateAsync(AttendanceSession session);
        Task<bool> DeleteAsync(Guid id, Guid tenantId);
    }

    public interface IAttendanceRecordDAL
    {
        Task<List<AttendanceRecord>> GetBySessionIdAsync(Guid sessionId);
        Task<bool> SaveRecordsAsync(Guid sessionId, List<AttendanceRecord> records);
    }
}
