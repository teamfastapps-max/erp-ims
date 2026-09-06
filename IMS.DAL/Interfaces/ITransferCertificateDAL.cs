using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IMS.Models.Portal;

namespace IMS.DAL.Interfaces
{
    public interface ITransferCertificateDAL
    {
        Task<(List<TransferCertificateDto> Items, int TotalCount)> GetPagedAsync(Guid tenantId, string? status, string? search, int pageNumber, int pageSize);
        Task<bool> ReviewAsync(Guid tcId, Guid tenantId, bool libraryClearance, bool feeClearance, bool labClearance, string status, string? remarks);
        Task<bool> DeleteAsync(Guid tcId, Guid tenantId);
        Task<TransferCertificatePrintViewModel?> GetByIdAsync(Guid tcId, Guid tenantId);
    }
}
