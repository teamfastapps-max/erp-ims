using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Models;

namespace IMS.DAL.Interfaces
{
    public interface IVendorDAL
    {
        //int AddVendor(VendorModel model);
        // Vendor Master
        Task<VendorFilterModel> GetAllVendorsAsync(Guid tenantId, VendorFilterModel filter);
        Task<VendorModel> GetVendorByIdAsync(int vendorId, Guid tenantId);
        Task<int> CreateVendorAsync(VendorModel model, Guid tenantId, int? createdBy);
        Task<bool> UpdateVendorAsync(VendorModel model, Guid tenantId);
        Task<bool> DeleteVendorAsync(int vendorId, Guid tenantId);

        // Addresses
        Task<bool> UpsertAddressAsync(VendorAddressModel model, Guid tenantId);
        Task<bool> DeleteAddressAsync(int vendorAddressId, Guid tenantId);

        // Contacts
        Task<bool> UpsertContactAsync(VendorContactModel model, Guid tenantId);
        Task<bool> DeleteContactAsync(int vendorContactId, Guid tenantId);

        // Bank Details
        Task<bool> UpsertBankAsync(VendorBankModel model, Guid tenantId);
        Task<bool> DeleteBankAsync(int vendorBankId, Guid tenantId);

        // Documents
        Task<int> AddDocumentAsync(VendorDocumentModel model, Guid tenantId, int? uploadedBy);
        Task<bool> DeleteDocumentAsync(int vendorDocumentId, Guid tenantId);

        // Lookups
        Task<List<VendorCategoryModel>> GetCategoriesAsync(Guid tenantId);
        Task<List<CurrencyModel>> GetCurrenciesAsync();

        // Stats
        Task<VendorStatsModel> GetVendorStatsAsync(Guid tenantId);

    }
}
