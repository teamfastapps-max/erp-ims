using System;
using System.Threading.Tasks;
using IMS.Models.ViewModels;

namespace IMS.Services.Interfaces
{
    public interface IStaffService
    {
        Task<StaffIndexViewModel> GetStaffListAsync(
            Guid tenantId, string searchTerm, Guid? branchId, string status,
            int pageNumber, int pageSize);

        Task<StaffDetailsViewModel> GetStaffDetailsAsync(Guid id, Guid tenantId);

        Task<StaffFormViewModel> GetStaffForEditAsync(Guid id, Guid tenantId);

        Task<ServiceResult> CreateStaffAsync(StaffFormViewModel model, Guid tenantId);

        Task<ServiceResult> UpdateStaffAsync(StaffFormViewModel model, Guid tenantId);

        Task<ServiceResult> DeleteStaffAsync(Guid id, Guid tenantId);

        void PopulateDropdowns(StaffFormViewModel vm);
    }
}
