using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using IMS.DAL.Interfaces;
using IMS.Models.Portal;
using IMS.Services.Interfaces;

namespace IMS.Services
{
    public class TransferCertificateService : ITransferCertificateService
    {
        private readonly ITransferCertificateDAL _dal;
        private readonly ILogger<TransferCertificateService> _logger;

        public TransferCertificateService(ITransferCertificateDAL dal, ILogger<TransferCertificateService> logger)
        {
            _dal = dal;
            _logger = logger;
        }

        public async Task<(List<TransferCertificateDto> Items, int TotalCount)> GetPagedAsync(Guid tenantId, string? status, string? search, int pageNumber, int pageSize)
        {
            try
            {
                return await _dal.GetPagedAsync(tenantId, status, search, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transfer certificate list for tenant {TenantId}", tenantId);
                return (new List<TransferCertificateDto>(), 0);
            }
        }

        public async Task<ServiceResult> ReviewAsync(Guid tcId, Guid tenantId, bool libraryClearance, bool feeClearance, bool labClearance, string status, string? remarks)
        {
            try
            {
                var success = await _dal.ReviewAsync(tcId, tenantId, libraryClearance, feeClearance, labClearance, status, remarks);
                return success
                    ? ServiceResult.Ok($"Transfer Certificate status updated to {status}.", tcId)
                    : ServiceResult.Fail("Transfer Certificate record not found or update failed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reviewing transfer certificate {TCId}", tcId);
                return ServiceResult.Fail("Failed to update Transfer Certificate. Please try again.");
            }
        }

        public async Task<ServiceResult> DeleteAsync(Guid tcId, Guid tenantId)
        {
            try
            {
                var success = await _dal.DeleteAsync(tcId, tenantId);
                return success
                    ? ServiceResult.Ok("Transfer Certificate application deleted successfully.", tcId)
                    : ServiceResult.Fail("Transfer Certificate application not found or could not be deleted.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting transfer certificate {TCId}", tcId);
                return ServiceResult.Fail("Failed to delete Transfer Certificate application.");
            }
        }

        public async Task<TransferCertificatePrintViewModel?> GetByIdAsync(Guid tcId, Guid tenantId)
        {
            try
            {
                return await _dal.GetByIdAsync(tcId, tenantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Transfer Certificate {TCId} for print", tcId);
                return null;
            }
        }
    }
}
