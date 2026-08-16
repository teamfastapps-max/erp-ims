using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Models;
using IMS.Services.Interfaces;
using IMS.DAL.Interfaces;
using Microsoft.AspNetCore.Http;

namespace IMS.Services.Services
{
    public class VendorService : IVendorService
    {
        private readonly IVendorDAL _vendorDAL;
        //private readonly IWebHostEnvironment _env;
        //private readonly ILogger<VendorService> _logger;

        private static readonly string[] AllowedDocExtensions = { ".pdf", ".jpg", ".jpeg", ".png", ".docx", ".xlsx" };
        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

        //public VendorService(IVendorDAL vendorDAL, IWebHostEnvironment env, ILogger<VendorService> logger)
        //{
        //    _vendorDAL = vendorDAL;
        //    _env = env;
        //    _logger = logger;
        //}
        public VendorService(IVendorDAL vendorDAL)
        {
            _vendorDAL = vendorDAL;
        }

        // ============================================================================
        // VENDOR MASTER
        // ============================================================================

        public async Task<VendorFilterModel> GetAllVendorsAsync(Guid tenantId, VendorFilterModel filter)
        {
            try
            {
                var result = await _vendorDAL.GetAllVendorsAsync(tenantId, filter);
                result.Categories = await _vendorDAL.GetCategoriesAsync(tenantId);
                return result;
            }
            catch (Exception ex)
            {
               // _logger.LogError(ex, "Error fetching vendor list");
                return filter;
            }
        }

        public async Task<VendorModel> GetVendorByIdAsync(int vendorId, Guid tenantId)
        {
            try
            {
                var vendor = await _vendorDAL.GetVendorByIdAsync(vendorId, tenantId);
                if (vendor != null)
                {
                    vendor.Categories = await _vendorDAL.GetCategoriesAsync(tenantId);
                    vendor.Currencies = await _vendorDAL.GetCurrenciesAsync();
                }
                return vendor;
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error fetching vendor {VendorId}", vendorId);
                return null;
            }
        }

        public async Task<(bool Success, string Message, int VendorId)> CreateVendorAsync(
            VendorModel model, Guid tenantId, int? createdBy)
        {
            try
            {
                int newId = await _vendorDAL.CreateVendorAsync(model, tenantId, createdBy);
                return (true, "Vendor created successfully.", newId);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error creating vendor");
                string msg = ex.Message.Contains("already exists")
                    ? "Vendor code already exists. Please use a different code."
                    : "An error occurred while creating the vendor.";
                return (false, msg, 0);
            }
        }

        public async Task<(bool Success, string Message)> UpdateVendorAsync(VendorModel model, Guid tenantId)
        {
            try
            {
                bool ok = await _vendorDAL.UpdateVendorAsync(model, tenantId);
                return ok
                    ? (true, "Vendor updated successfully.")
                    : (false, "Vendor not found.");
            }
            catch (Exception ex)
            {
               // _logger.LogError(ex, "Error updating vendor {VendorId}", model.VendorId);
                string msg = ex.Message.Contains("already exists")
                    ? "Vendor code already exists. Please use a different code."
                    : "An error occurred while updating the vendor.";
                return (false, msg);
            }
        }

        public async Task<(bool Success, string Message)> DeleteVendorAsync(int vendorId, Guid tenantId)
        {
            try
            {
                bool ok = await _vendorDAL.DeleteVendorAsync(vendorId, tenantId);
                return ok
                    ? (true, "Vendor deactivated successfully.")
                    : (false, "Vendor not found.");
            }
            catch (Exception ex)
            {
               // _logger.LogError(ex, "Error deleting vendor {VendorId}", vendorId);
                return (false, "An error occurred while deactivating the vendor.");
            }
        }

        // ============================================================================
        // ADDRESSES
        // ============================================================================

        public async Task<(bool Success, string Message)> UpsertAddressAsync(VendorAddressModel model, Guid tenantId)
        {
            try
            {
                await _vendorDAL.UpsertAddressAsync(model, tenantId);
                return (true, "Address saved successfully.");
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error upserting address");
                return (false, "An error occurred while saving the address.");
            }
        }

        public async Task<(bool Success, string Message)> DeleteAddressAsync(int vendorAddressId, Guid tenantId)
        {
            try
            {
                await _vendorDAL.DeleteAddressAsync(vendorAddressId, tenantId);
                return (true, "Address deleted successfully.");
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error deleting address {Id}", vendorAddressId);
                return (false, "An error occurred while deleting the address.");
            }
        }

        // ============================================================================
        // CONTACTS
        // ============================================================================

        public async Task<(bool Success, string Message)> UpsertContactAsync(VendorContactModel model, Guid tenantId)
        {
            try
            {
                await _vendorDAL.UpsertContactAsync(model, tenantId);
                return (true, "Contact saved successfully.");
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error upserting contact");
                return (false, "An error occurred while saving the contact.");
            }
        }

        public async Task<(bool Success, string Message)> DeleteContactAsync(int vendorContactId, Guid tenantId)
        {
            try
            {
                await _vendorDAL.DeleteContactAsync(vendorContactId, tenantId);
                return (true, "Contact deleted successfully.");
            }
            catch (Exception ex)
            {
               // _logger.LogError(ex, "Error deleting contact {Id}", vendorContactId);
                return (false, "An error occurred while deleting the contact.");
            }
        }

        // ============================================================================
        // BANK DETAILS
        // ============================================================================

        public async Task<(bool Success, string Message)> UpsertBankAsync(VendorBankModel model, Guid tenantId)
        {
            try
            {
                await _vendorDAL.UpsertBankAsync(model, tenantId);
                return (true, "Bank details saved successfully.");
            }
            catch (Exception ex)
            {
               // _logger.LogError(ex, "Error upserting bank details");
                return (false, "An error occurred while saving bank details.");
            }
        }

        public async Task<(bool Success, string Message)> DeleteBankAsync(int vendorBankId, Guid tenantId)
        {
            try
            {
                await _vendorDAL.DeleteBankAsync(vendorBankId, tenantId);
                return (true, "Bank details deleted successfully.");
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error deleting bank {Id}", vendorBankId);
                return (false, "An error occurred while deleting bank details.");
            }
        }

        // ============================================================================
        // DOCUMENTS
        // ============================================================================

        public async Task<(bool Success, string Message, int DocumentId)> UploadDocumentAsync(
            int vendorId, string documentType, string documentNumber,
            IFormFile file, Guid tenantId, int? uploadedBy)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return (false, "No file selected.", 0);

                if (file.Length > MaxFileSizeBytes)
                    return (false, "File size exceeds the 5 MB limit.", 0);

                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!Array.Exists(AllowedDocExtensions, e => e == ext))
                    return (false, "Unsupported file type. Allowed: PDF, JPG, PNG, DOCX, XLSX.", 0);

                // Save to wwwroot/uploads/vendors/{vendorId}/
                var WebRootPath = "wwwroot/uploads/vendors/";
                string uploadFolder = Path.Combine(WebRootPath, "uploads", "vendors", vendorId.ToString());
                Directory.CreateDirectory(uploadFolder);

                string uniqueName = $"{Guid.NewGuid()}{ext}";
                string filePath = Path.Combine(uploadFolder, uniqueName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await file.CopyToAsync(stream);

                string relPath = $"/uploads/vendors/{vendorId}/{uniqueName}";

                var docModel = new VendorDocumentModel
                {
                    VendorId = vendorId,
                    DocumentType = documentType,
                    DocumentNumber = documentNumber,
                    FilePath = relPath
                };

                int docId = await _vendorDAL.AddDocumentAsync(docModel, tenantId, uploadedBy);
                return (true, "Document uploaded successfully.", docId);
            }
            catch (Exception ex)
            {
               // _logger.LogError(ex, "Error uploading document for vendor {VendorId}", vendorId);
                return (false, "An error occurred while uploading the document.", 0);
            }
        }

        public async Task<(bool Success, string Message)> DeleteDocumentAsync(int vendorDocumentId, Guid tenantId)
        {
            try
            {
                await _vendorDAL.DeleteDocumentAsync(vendorDocumentId, tenantId);
                return (true, "Document deleted successfully.");
            }
            catch (Exception ex)
            {
               // _logger.LogError(ex, "Error deleting document {Id}", vendorDocumentId);
                return (false, "An error occurred while deleting the document.");
            }
        }

        // ============================================================================
        // LOOKUPS & STATS
        // ============================================================================

        public async Task<List<VendorCategoryModel>> GetCategoriesAsync(Guid tenantId)
            => await _vendorDAL.GetCategoriesAsync(tenantId);

        public async Task<List<CurrencyModel>> GetCurrenciesAsync()
            => await _vendorDAL.GetCurrenciesAsync();

        public async Task<VendorStatsModel> GetVendorStatsAsync(Guid tenantId)
            => await _vendorDAL.GetVendorStatsAsync(tenantId);


        //private readonly IVendorDAL _vendorDAL;

        //public VendorService(IVendorDAL vendorDAL)
        //{
        //    _vendorDAL = vendorDAL;
        //}

        //public int AddVendor(VendorModel model)
        //{
        //    if (string.IsNullOrWhiteSpace(model.VendorName))
        //        throw new Exception("Vendor Name is required.");

        //    if (string.IsNullOrWhiteSpace(model.MobileNo))
        //        throw new Exception("Mobile Number is required.");

        //    if (string.IsNullOrWhiteSpace(model.Email))
        //        throw new Exception("Email is required.");

        //    model.VendorCode = GenerateVendorCode();

        //    return _vendorDAL.AddVendor(model);
        //}

        //private string GenerateVendorCode()
        //{
        //    return $"VEN-{DateTime.Now:yyyyMMddHHmmss}";
        //}
    }
}
