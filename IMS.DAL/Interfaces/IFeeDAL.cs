using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IMS.Models.Entities;

namespace IMS.DAL.Interfaces
{
    public interface IFeeStructureDAL
    {
        Task<FeeStructure> GetByIdAsync(Guid id, Guid tenantId);
        Task<(List<FeeStructure> Items, int TotalCount)> GetPagedAsync(
            Guid tenantId, string searchTerm, Guid? academicYearId,
            int pageNumber, int pageSize);
        Task<bool> IsCodeTakenAsync(Guid tenantId, string code, Guid? excludeId);
        Task<Guid> CreateAsync(FeeStructure feeStructure);
        Task<bool> UpdateAsync(FeeStructure feeStructure);
        Task<bool> DeleteAsync(Guid id, Guid tenantId);
    }

    public interface IFeeStructureItemDAL
    {
        Task<List<FeeStructureItem>> GetByFeeStructureIdAsync(Guid feeStructureId);
        Task<bool> SaveItemsAsync(Guid feeStructureId, List<FeeStructureItem> items);
    }

    public interface IFeeInvoiceDAL
    {
        Task<FeeInvoice> GetByIdAsync(Guid id, Guid tenantId);
        Task<(List<FeeInvoice> Items, int TotalCount)> GetPagedAsync(
            Guid tenantId, string searchTerm, string status,
            int pageNumber, int pageSize);
        Task<string> GetNextInvoiceNumberAsync(Guid tenantId);
        Task<Guid> CreateAsync(FeeInvoice invoice);
        Task<bool> UpdateAsync(FeeInvoice invoice);
        Task<bool> UpdatePaidAmountAsync(Guid invoiceId, decimal paidAmount);
        Task<bool> DeleteAsync(Guid id, Guid tenantId);
    }

    public interface IPaymentDAL
    {
        Task<Payment> GetByIdAsync(Guid id, Guid tenantId);
        Task<(List<Payment> Items, int TotalCount)> GetPagedAsync(
            Guid tenantId, string searchTerm, string status,
            int pageNumber, int pageSize);
        Task<string> GetNextPaymentNumberAsync(Guid tenantId);
        Task<Guid> CreateAsync(Payment payment);
        Task<bool> UpdateAsync(Payment payment);
        Task<bool> DeleteAsync(Guid id, Guid tenantId);
    }
}
