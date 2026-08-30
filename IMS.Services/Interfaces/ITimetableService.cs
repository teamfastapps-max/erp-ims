using System;
using System.Threading.Tasks;
using IMS.Models.ViewModels;

namespace IMS.Services.Interfaces
{
    public interface ITimetableService
    {
        Task<TimetableIndexViewModel> GetListAsync(Guid tenantId, Guid? batchId, Guid? branchId);
        Task<TimetableDetailsViewModel> GetDetailsAsync(Guid id, Guid tenantId);
        Task<TimetableFormViewModel> GetForEditAsync(Guid id, Guid tenantId);
        Task<ServiceResult> CreateAsync(TimetableFormViewModel model, Guid tenantId);
        Task<ServiceResult> UpdateAsync(TimetableFormViewModel model, Guid tenantId);
        Task<ServiceResult> DeleteAsync(Guid id, Guid tenantId);
        Task<bool> CheckConflictAsync(Guid tenantId, Guid batchId, int dayOfWeek, string startTime, string endTime, Guid? excludeId);

        void PopulateDropdowns(TimetableFormViewModel vm);
    }
}
