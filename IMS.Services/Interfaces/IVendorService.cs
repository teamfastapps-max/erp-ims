using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Models;

namespace IMS.Services.Interfaces
{
    public interface IVendorService
    {
        //int AddVendor(VendorModel model);
        Task<VendorFilterModel> GetAllVendorsAsync(Guid tenantId, VendorFilterModel filter);
        Task<VendorModel> GetVendorByIdAsync(int vendorId, Guid tenantId);
        Task<(bool Success, string Message, int VendorId)> CreateVendorAsync(VendorModel model, Guid tenantId, int? createdBy);
        Task<(bool Success, string Message)> UpdateVendorAsync(VendorModel model, Guid tenantId);
        Task<(bool Success, string Message)> DeleteVendorAsync(int vendorId, Guid tenantId);

        // Address
        Task<(bool Success, string Message)> UpsertAddressAsync(VendorAddressModel model, Guid tenantId);
        Task<(bool Success, string Message)> DeleteAddressAsync(int vendorAddressId, Guid tenantId);

        // Contact
        Task<(bool Success, string Message)> UpsertContactAsync(VendorContactModel model, Guid tenantId);
        Task<(bool Success, string Message)> DeleteContactAsync(int vendorContactId, Guid tenantId);

        // Bank
        Task<(bool Success, string Message)> UpsertBankAsync(VendorBankModel model, Guid tenantId);
        Task<(bool Success, string Message)> DeleteBankAsync(int vendorBankId, Guid tenantId);

        // Documents
        Task<(bool Success, string Message, int DocumentId)> UploadDocumentAsync(int vendorId, string documentType, string documentNumber, IFormFile file, Guid tenantId, int? uploadedBy);
        Task<(bool Success, string Message)> DeleteDocumentAsync(int vendorDocumentId, Guid tenantId);

        // Lookups
        Task<List<VendorCategoryModel>> GetCategoriesAsync(Guid tenantId);
        Task<List<CurrencyModel>> GetCurrenciesAsync();

        // Stats
        Task<VendorStatsModel> GetVendorStatsAsync(Guid tenantId);

    }
}
