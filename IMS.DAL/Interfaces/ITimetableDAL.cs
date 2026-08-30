using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IMS.Models.Entities;

namespace IMS.DAL.Interfaces
{
    public interface ITimetableDAL
    {
        Task<Timetable> GetByIdAsync(Guid id, Guid tenantId);
        Task<List<Timetable>> GetAllAsync(Guid tenantId, Guid? batchId, Guid? branchId);
        Task<bool> HasConflictAsync(Guid tenantId, Guid batchId, int dayOfWeek, TimeSpan start, TimeSpan end, Guid? excludeId);
        Task<Guid> CreateAsync(Timetable t);
        Task<bool> UpdateAsync(Timetable t);
        Task<bool> DeleteAsync(Guid id, Guid tenantId);
    }
}
