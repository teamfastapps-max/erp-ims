using System;
using System.Threading.Tasks;
using IMS.Models.ViewModels;

namespace IMS.Services.Interfaces
{
    public interface IFeeService
    {
        Task<FeeStructureIndexViewModel> GetFeeStructureListAsync(Guid tenantId, string searchTerm, Guid? academicYearId, int pageNumber, int pageSize);
        Task<FeeStructureFormViewModel> GetFeeStructureForEditAsync(Guid id, Guid tenantId);
        Task<ServiceResult> CreateFeeStructureAsync(FeeStructureFormViewModel model, Guid tenantId);
        Task<ServiceResult> UpdateFeeStructureAsync(FeeStructureFormViewModel model, Guid tenantId);
        Task<ServiceResult> DeleteFeeStructureAsync(Guid id, Guid tenantId);
        void PopulateFeeStructureDropdowns(FeeStructureFormViewModel vm, Guid tenantId);

        Task<FeeInvoiceIndexViewModel> GetFeeInvoiceListAsync(Guid tenantId, string searchTerm, string status, int pageNumber, int pageSize);
        Task<FeeInvoiceFormViewModel> GetFeeInvoiceForEditAsync(Guid id, Guid tenantId);
        Task<ServiceResult> CreateFeeInvoiceAsync(FeeInvoiceFormViewModel model, Guid tenantId);
        Task<ServiceResult> DeleteFeeInvoiceAsync(Guid id, Guid tenantId);
        void PopulateFeeInvoiceDropdowns(FeeInvoiceFormViewModel vm, Guid tenantId);

        Task<PaymentIndexViewModel> GetPaymentListAsync(Guid tenantId, string searchTerm, string status, int pageNumber, int pageSize);
        Task<PaymentFormViewModel> GetPaymentForEditAsync(Guid id, Guid tenantId);
        Task<ServiceResult> CreatePaymentAsync(PaymentFormViewModel model, Guid tenantId, Guid createdBy);
        Task<ServiceResult> DeletePaymentAsync(Guid id, Guid tenantId);
        void PopulatePaymentDropdowns(PaymentFormViewModel vm, Guid tenantId);
    }
}
