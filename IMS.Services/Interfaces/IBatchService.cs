using System;
using System.Threading.Tasks;
using IMS.Models.ViewModels;

namespace IMS.Services.Interfaces
{
    public interface IBatchService
    {
        Task<BatchIndexViewModel> GetBatchListAsync(
            Guid tenantId, string searchTerm, Guid? branchId, Guid? courseId,
            Guid? academicYearId, string status, int pageNumber, int pageSize);

        Task<BatchDetailsViewModel> GetBatchDetailsAsync(Guid id, Guid tenantId);

        Task<BatchFormViewModel> GetBatchForEditAsync(Guid id, Guid tenantId);

        Task<ServiceResult> CreateBatchAsync(BatchFormViewModel model, Guid tenantId);

        Task<ServiceResult> UpdateBatchAsync(BatchFormViewModel model, Guid tenantId);

        Task<ServiceResult> DeleteBatchAsync(Guid id, Guid tenantId);

        void PopulateDropdowns(BatchFormViewModel vm);
    }
}
