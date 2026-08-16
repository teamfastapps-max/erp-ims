using System.Data;
using System.Data.SqlClient;
using IMS.DAL.Common;
using IMS.DAL.Interfaces;
using IMS.Models;

namespace IMS.DAL.Repositories
{
    public class VendorDAL : IVendorDAL
    {
        private readonly DBHelper _dbHelper;

        public VendorDAL(DBHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        //public int AddVendor(VendorModel model)
        //{
        //    int vendorId = 0;

        //    using (SqlConnection con = _dbHelper._dbHelper._dbHelper.GetConnection())
        //    {
        //        using (SqlCommand cmd = _dbHelper.CreateCommand("sp_Vendor_Insert", con))
        //        {
        //            cmd.Parameters.AddWithValue("@VendorCode", model.VendorCode);
        //            cmd.Parameters.AddWithValue("@VendorName", model.VendorName);
        //            cmd.Parameters.AddWithValue("@ContactPerson", model.ContactPerson);
        //            cmd.Parameters.AddWithValue("@Email", model.Email);
        //            cmd.Parameters.AddWithValue("@MobileNo", model.MobileNo);
        //            cmd.Parameters.AddWithValue("@GSTNo", model.GSTNo);
        //            cmd.Parameters.AddWithValue("@PANNo", model.PANNo);
        //            cmd.Parameters.AddWithValue("@Address", model.Address);
        //            cmd.Parameters.AddWithValue("@City", model.City);
        //            cmd.Parameters.AddWithValue("@State", model.State);
        //            cmd.Parameters.AddWithValue("@Country", model.Country);
        //            cmd.Parameters.AddWithValue("@PinCode", model.PinCode);
        //            cmd.Parameters.AddWithValue("@CreatedBy", 1);

        //            con.Open();

        //            object result = cmd.ExecuteScalar();

        //            if (result != null)
        //            {
        //                vendorId = Convert.ToInt32(result);
        //            }
        //        }
        //    }

        //    return vendorId;
        //}

                // ============================================================================
        // VENDOR MASTER
        // ============================================================================
 
        public async Task<VendorFilterModel> GetAllVendorsAsync(Guid tenantId, VendorFilterModel filter)
        {
            var result = filter;
            result.Vendors = new List<VendorListItemModel>();
 
            using var con = _dbHelper.GetConnection();
            using var cmd = new SqlCommand("sp_Vendor_GetAll", con)
            {
                CommandType = CommandType.StoredProcedure
            };
 
            cmd.Parameters.AddWithValue("@TenantId",           tenantId);
            cmd.Parameters.AddWithValue("@SearchTerm",         (object)filter.SearchTerm ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@VendorCategoryId",   (object)filter.VendorCategoryId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsActive",           (object)filter.IsActive ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PageNumber",         filter.PageNumber);
            cmd.Parameters.AddWithValue("@PageSize",           filter.PageSize);
 
            await con.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
 
            while (await reader.ReadAsync())
            {
                result.Vendors.Add(MapListItem(reader));
            }
 
            if (result.Vendors.Count > 0)
                result.TotalCount = result.Vendors[0].TotalCount;
 
            return result;
        }
 
        public async Task<VendorModel> GetVendorByIdAsync(int vendorId, Guid tenantId)
        {
            VendorModel vendor = null;
 
            using var con = _dbHelper.GetConnection();
            using var cmd = new SqlCommand("sp_Vendor_GetById", con)
            {
                CommandType = CommandType.StoredProcedure
            };
 
            cmd.Parameters.AddWithValue("@VendorId",  vendorId);
            cmd.Parameters.AddWithValue("@TenantId",  tenantId);
 
            await con.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
 
            // ResultSet 1: Vendor Master
            if (await reader.ReadAsync())
                vendor = MapVendor(reader);
 
            if (vendor == null) return null;
 
            // ResultSet 2: Addresses
            await reader.NextResultAsync();
            while (await reader.ReadAsync())
                vendor.Addresses.Add(MapAddress(reader));
 
            // ResultSet 3: Contacts
            await reader.NextResultAsync();
            while (await reader.ReadAsync())
                vendor.Contacts.Add(MapContact(reader));
 
            // ResultSet 4: Bank Details
            await reader.NextResultAsync();
            while (await reader.ReadAsync())
                vendor.BankDetails.Add(MapBank(reader));
 
            // ResultSet 5: Documents
            await reader.NextResultAsync();
            while (await reader.ReadAsync())
                vendor.Documents.Add(MapDocument(reader));
 
            return vendor;
        }
 
        public async Task<int> CreateVendorAsync(VendorModel model, Guid tenantId, int? createdBy)
        {
            using var con = _dbHelper.GetConnection();
            using var cmd = new SqlCommand("sp_Vendor_Create", con)
            {
                CommandType = CommandType.StoredProcedure
            };
 
            cmd.Parameters.AddWithValue("@TenantId",               tenantId);
            cmd.Parameters.AddWithValue("@VendorCode",             model.VendorCode);
            cmd.Parameters.AddWithValue("@VendorName",             model.VendorName);
            cmd.Parameters.AddWithValue("@VendorCategoryId",       (object)model.VendorCategoryId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@TaxRegistrationNumber",  (object)model.TaxRegistrationNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CurrencyCode",           (object)model.CurrencyCode ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsActive",               model.IsActive);
            cmd.Parameters.AddWithValue("@CreatedBy",              (object)createdBy ?? DBNull.Value);
 
            var outParam = new SqlParameter("@NewVendorId", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(outParam);
 
            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
 
            return (int)outParam.Value;
        }
 
        public async Task<bool> UpdateVendorAsync(VendorModel model, Guid tenantId)
        {
            using var con = _dbHelper.GetConnection();
            using var cmd = new SqlCommand("sp_Vendor_Update", con)
            {
                CommandType = CommandType.StoredProcedure
            };
 
            cmd.Parameters.AddWithValue("@VendorId",              model.VendorId);
            cmd.Parameters.AddWithValue("@TenantId",              tenantId);
            cmd.Parameters.AddWithValue("@VendorCode",            model.VendorCode);
            cmd.Parameters.AddWithValue("@VendorName",            model.VendorName);
            cmd.Parameters.AddWithValue("@VendorCategoryId",      (object)model.VendorCategoryId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@TaxRegistrationNumber", (object)model.TaxRegistrationNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CurrencyCode",          (object)model.CurrencyCode ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsActive",              model.IsActive);
 
            await con.OpenAsync();
            int rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }
 
        public async Task<bool> DeleteVendorAsync(int vendorId, Guid tenantId)
        {
            using var con = _dbHelper.GetConnection();
            using var cmd = new SqlCommand("sp_Vendor_Delete", con)
            {
                CommandType = CommandType.StoredProcedure
            };
 
            cmd.Parameters.AddWithValue("@VendorId", vendorId);
            cmd.Parameters.AddWithValue("@TenantId", tenantId);
 
            await con.OpenAsync();
            int rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }
 
        // ============================================================================
        // ADDRESSES
        // ============================================================================
 
        public async Task<bool> UpsertAddressAsync(VendorAddressModel model, Guid tenantId)
        {
            using var con = _dbHelper.GetConnection();
            using var cmd = new SqlCommand("sp_VendorAddress_Upsert", con)
            {
                CommandType = CommandType.StoredProcedure
            };
 
            cmd.Parameters.AddWithValue("@VendorAddressId", model.VendorAddressId == 0 ? (object)DBNull.Value : model.VendorAddressId);
            cmd.Parameters.AddWithValue("@TenantId",        tenantId);
            cmd.Parameters.AddWithValue("@VendorId",        model.VendorId);
            cmd.Parameters.AddWithValue("@AddressType",     model.AddressType ?? "Office");
            cmd.Parameters.AddWithValue("@AddressLine1",    (object)model.AddressLine1 ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@AddressLine2",    (object)model.AddressLine2 ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@City",            (object)model.City ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@State",           (object)model.State ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Country",         (object)model.Country ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PostalCode",      (object)model.PostalCode ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsPrimary",       model.IsPrimary);
 
            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            return true;
        }
 
        public async Task<bool> DeleteAddressAsync(int vendorAddressId, Guid tenantId)
        {
            using var con = _dbHelper.GetConnection();
            using var cmd = new SqlCommand("sp_VendorAddress_Delete", con)
            {
                CommandType = CommandType.StoredProcedure
            };
 
            cmd.Parameters.AddWithValue("@VendorAddressId", vendorAddressId);
            cmd.Parameters.AddWithValue("@TenantId",        tenantId);
 
            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            return true;
        }
 
        // ============================================================================
        // CONTACTS
        // ============================================================================
 
        public async Task<bool> UpsertContactAsync(VendorContactModel model, Guid tenantId)
        {
            using var con = _dbHelper.GetConnection();
            using var cmd = new SqlCommand("sp_VendorContact_Upsert", con)
            {
                CommandType = CommandType.StoredProcedure
            };
 
            cmd.Parameters.AddWithValue("@VendorContactId", model.VendorContactId == 0 ? (object)DBNull.Value : model.VendorContactId);
            cmd.Parameters.AddWithValue("@TenantId",        tenantId);
            cmd.Parameters.AddWithValue("@VendorId",        model.VendorId);
            cmd.Parameters.AddWithValue("@ContactName",     model.ContactName);
            cmd.Parameters.AddWithValue("@Designation",     (object)model.Designation ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Email",           (object)model.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Phone",           (object)model.Phone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsPrimary",       model.IsPrimary);
 
            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            return true;
        }
 
        public async Task<bool> DeleteContactAsync(int vendorContactId, Guid tenantId)
        {
            using var con = _dbHelper.GetConnection();
            using var cmd = new SqlCommand("sp_VendorContact_Delete", con)
            {
                CommandType = CommandType.StoredProcedure
            };
 
            cmd.Parameters.AddWithValue("@VendorContactId", vendorContactId);
            cmd.Parameters.AddWithValue("@TenantId",        tenantId);
 
            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            return true;
        }
 
        // ============================================================================
        // BANK DETAILS
        // ============================================================================
 
        public async Task<bool> UpsertBankAsync(VendorBankModel model, Guid tenantId)
        {
            using var con = _dbHelper.GetConnection();
            using var cmd = new SqlCommand("sp_VendorBank_Upsert", con)
            {
                CommandType = CommandType.StoredProcedure
            };
 
            cmd.Parameters.AddWithValue("@VendorBankId",       model.VendorBankId == 0 ? (object)DBNull.Value : model.VendorBankId);
            cmd.Parameters.AddWithValue("@TenantId",           tenantId);
            cmd.Parameters.AddWithValue("@VendorId",           model.VendorId);
            cmd.Parameters.AddWithValue("@BankName",           model.BankName);
            cmd.Parameters.AddWithValue("@AccountHolderName",  model.AccountHolderName);
            cmd.Parameters.AddWithValue("@AccountNumber",      model.AccountNumber);
            cmd.Parameters.AddWithValue("@IFSCOrSwiftCode",    (object)model.IFSCOrSwiftCode ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@BranchName",         (object)model.BranchName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsPrimary",          model.IsPrimary);
 
            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            return true;
        }
 
        public async Task<bool> DeleteBankAsync(int vendorBankId, Guid tenantId)
        {
            using var con = _dbHelper.GetConnection();
            using var cmd = new SqlCommand("sp_VendorBank_Delete", con)
            {
                CommandType = CommandType.StoredProcedure
            };
 
            cmd.Parameters.AddWithValue("@VendorBankId", vendorBankId);
            cmd.Parameters.AddWithValue("@TenantId",     tenantId);
 
            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            return true;
        }
 
        // ============================================================================
        // DOCUMENTS
        // ============================================================================
 
        public async Task<int> AddDocumentAsync(VendorDocumentModel model, Guid tenantId, int? uploadedBy)
        {
            using var con = _dbHelper.GetConnection();
            using var cmd = new SqlCommand("sp_VendorDocument_Add", con)
            {
                CommandType = CommandType.StoredProcedure
            };
 
            cmd.Parameters.AddWithValue("@TenantId",       tenantId);
            cmd.Parameters.AddWithValue("@VendorId",       model.VendorId);
            cmd.Parameters.AddWithValue("@DocumentType",   model.DocumentType);
            cmd.Parameters.AddWithValue("@DocumentNumber", (object)model.DocumentNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@FilePath",       model.FilePath);
            cmd.Parameters.AddWithValue("@UploadedBy",     (object)uploadedBy ?? DBNull.Value);
 
            await con.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }
 
        public async Task<bool> DeleteDocumentAsync(int vendorDocumentId, Guid tenantId)
        {
            using var con = _dbHelper.GetConnection();
            using var cmd = new SqlCommand("sp_VendorDocument_Delete", con)
            {
                CommandType = CommandType.StoredProcedure
            };
 
            cmd.Parameters.AddWithValue("@VendorDocumentId", vendorDocumentId);
            cmd.Parameters.AddWithValue("@TenantId",         tenantId);
 
            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            return true;
        }
 
        // ============================================================================
        // LOOKUPS
        // ============================================================================
 
        public async Task<List<VendorCategoryModel>> GetCategoriesAsync(Guid tenantId)
        {
            var list = new List<VendorCategoryModel>();
 
            using var con = _dbHelper.GetConnection();
            using var cmd = new SqlCommand("sp_VendorCategory_GetAll", con)
            {
                CommandType = CommandType.StoredProcedure
            };
 
            cmd.Parameters.AddWithValue("@TenantId", tenantId);
 
            await con.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
 
            while (await reader.ReadAsync())
            {
                list.Add(new VendorCategoryModel
                {
                    VendorCategoryId = reader.GetInt32(reader.GetOrdinal("VendorCategoryId")),
                    CategoryName     = reader.GetString(reader.GetOrdinal("CategoryName"))
                });
            }
 
            return list;
        }
 
        public async Task<List<CurrencyModel>> GetCurrenciesAsync()
        {
            var list = new List<CurrencyModel>();
 
            using var con = _dbHelper.GetConnection();
            using var cmd = new SqlCommand("sp_Currency_GetAll", con)
            {
                CommandType = CommandType.StoredProcedure
            };
 
            await con.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
 
            while (await reader.ReadAsync())
            {
                list.Add(new CurrencyModel
                {
                    CurrencyCode = reader.GetString(reader.GetOrdinal("CurrencyCode")),
                    CurrencyName = reader.GetString(reader.GetOrdinal("CurrencyName")),
                    Symbol       = reader.GetString(reader.GetOrdinal("Symbol"))
                });
            }
 
            return list;
        }
 
        public async Task<VendorStatsModel> GetVendorStatsAsync(Guid tenantId)
        {
            using var con = _dbHelper.GetConnection();
            using var cmd = new SqlCommand("sp_Vendor_GetStats", con)
            {
                CommandType = CommandType.StoredProcedure
            };
 
            cmd.Parameters.AddWithValue("@TenantId", tenantId);
 
            await con.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
 
            if (await reader.ReadAsync())
            {
                return new VendorStatsModel
                {
                    TotalVendors    = reader.GetInt32(reader.GetOrdinal("TotalVendors")),
                    ActiveVendors   = reader.GetInt32(reader.GetOrdinal("ActiveVendors")),
                    InactiveVendors = reader.GetInt32(reader.GetOrdinal("InactiveVendors")),
                    AverageRating   = reader.IsDBNull(reader.GetOrdinal("AverageRating"))
                                        ? null
                                        : (decimal?)reader.GetDecimal(reader.GetOrdinal("AverageRating")),
                    NewThisMonth    = reader.GetInt32(reader.GetOrdinal("NewThisMonth"))
                };
            }
 
            return new VendorStatsModel();
        }
 


        // ============================================================================
        // PRIVATE MAPPING HELPERS
        // ============================================================================
 
        private static VendorListItemModel MapListItem(SqlDataReader r) => new()
        {
            VendorId                = r.GetInt32(r.GetOrdinal("VendorId")),
            VendorCode              = r.GetString(r.GetOrdinal("VendorCode")),
            VendorName              = r.GetString(r.GetOrdinal("VendorName")),
            VendorCategory          = r.IsDBNull(r.GetOrdinal("VendorCategory"))    ? null : r.GetString(r.GetOrdinal("VendorCategory")),
            TaxRegistrationNumber   = r.IsDBNull(r.GetOrdinal("TaxRegistrationNumber")) ? null : r.GetString(r.GetOrdinal("TaxRegistrationNumber")),
            CurrencyCode            = r.IsDBNull(r.GetOrdinal("CurrencyCode"))      ? null : r.GetString(r.GetOrdinal("CurrencyCode")),
            OverallRating           = r.IsDBNull(r.GetOrdinal("OverallRating"))     ? null : (decimal?)r.GetDecimal(r.GetOrdinal("OverallRating")),
            IsActive                = r.GetBoolean(r.GetOrdinal("IsActive")),
            CreatedDate             = r.GetDateTime(r.GetOrdinal("CreatedDate")),
            PrimaryContactName      = r.IsDBNull(r.GetOrdinal("PrimaryContactName"))  ? null : r.GetString(r.GetOrdinal("PrimaryContactName")),
            PrimaryContactPhone     = r.IsDBNull(r.GetOrdinal("PrimaryContactPhone")) ? null : r.GetString(r.GetOrdinal("PrimaryContactPhone")),
            PrimaryContactEmail     = r.IsDBNull(r.GetOrdinal("PrimaryContactEmail")) ? null : r.GetString(r.GetOrdinal("PrimaryContactEmail")),
            City                    = r.IsDBNull(r.GetOrdinal("City"))              ? null : r.GetString(r.GetOrdinal("City")),
            Country                 = r.IsDBNull(r.GetOrdinal("Country"))           ? null : r.GetString(r.GetOrdinal("Country")),
            TotalCount              = r.GetInt32(r.GetOrdinal("TotalCount"))
        };
 
        private static VendorModel MapVendor(SqlDataReader r) => new()
        {
            VendorId                = r.GetInt32(r.GetOrdinal("VendorId")),
            VendorCode              = r.GetString(r.GetOrdinal("VendorCode")),
            VendorName              = r.GetString(r.GetOrdinal("VendorName")),
            VendorCategoryId        = r.IsDBNull(r.GetOrdinal("VendorCategoryId"))   ? null : (int?)r.GetInt32(r.GetOrdinal("VendorCategoryId")),
            VendorCategory          = r.IsDBNull(r.GetOrdinal("VendorCategory"))     ? null : r.GetString(r.GetOrdinal("VendorCategory")),
            TaxRegistrationNumber   = r.IsDBNull(r.GetOrdinal("TaxRegistrationNumber")) ? null : r.GetString(r.GetOrdinal("TaxRegistrationNumber")),
            CurrencyCode            = r.IsDBNull(r.GetOrdinal("CurrencyCode"))       ? null : r.GetString(r.GetOrdinal("CurrencyCode")),
            CurrencyName            = r.IsDBNull(r.GetOrdinal("CurrencyName"))       ? null : r.GetString(r.GetOrdinal("CurrencyName")),
            CurrencySymbol          = r.IsDBNull(r.GetOrdinal("CurrencySymbol"))     ? null : r.GetString(r.GetOrdinal("CurrencySymbol")),
            OverallRating           = r.IsDBNull(r.GetOrdinal("OverallRating"))      ? null : (decimal?)r.GetDecimal(r.GetOrdinal("OverallRating")),
            IsActive                = r.GetBoolean(r.GetOrdinal("IsActive")),
            CreatedBy               = r.IsDBNull(r.GetOrdinal("CreatedBy"))          ? null : (int?)r.GetInt32(r.GetOrdinal("CreatedBy")),
            CreatedDate             = r.GetDateTime(r.GetOrdinal("CreatedDate")),
            ModifiedDate            = r.IsDBNull(r.GetOrdinal("ModifiedDate"))       ? null : (DateTime?)r.GetDateTime(r.GetOrdinal("ModifiedDate"))
        };
 
        private static VendorAddressModel MapAddress(SqlDataReader r) => new()
        {
            VendorAddressId = r.GetInt32(r.GetOrdinal("VendorAddressId")),
            VendorId        = r.GetInt32(r.GetOrdinal("VendorId")),
            AddressType     = r.GetString(r.GetOrdinal("AddressType")),
            AddressLine1    = r.IsDBNull(r.GetOrdinal("AddressLine1")) ? null : r.GetString(r.GetOrdinal("AddressLine1")),
            AddressLine2    = r.IsDBNull(r.GetOrdinal("AddressLine2")) ? null : r.GetString(r.GetOrdinal("AddressLine2")),
            City            = r.IsDBNull(r.GetOrdinal("City"))         ? null : r.GetString(r.GetOrdinal("City")),
            State           = r.IsDBNull(r.GetOrdinal("State"))        ? null : r.GetString(r.GetOrdinal("State")),
            Country         = r.IsDBNull(r.GetOrdinal("Country"))      ? null : r.GetString(r.GetOrdinal("Country")),
            PostalCode      = r.IsDBNull(r.GetOrdinal("PostalCode"))   ? null : r.GetString(r.GetOrdinal("PostalCode")),
            IsPrimary       = r.GetBoolean(r.GetOrdinal("IsPrimary"))
        };
 
        private static VendorContactModel MapContact(SqlDataReader r) => new()
        {
            VendorContactId = r.GetInt32(r.GetOrdinal("VendorContactId")),
            VendorId        = r.GetInt32(r.GetOrdinal("VendorId")),
            ContactName     = r.GetString(r.GetOrdinal("ContactName")),
            Designation     = r.IsDBNull(r.GetOrdinal("Designation")) ? null : r.GetString(r.GetOrdinal("Designation")),
            Email           = r.IsDBNull(r.GetOrdinal("Email"))       ? null : r.GetString(r.GetOrdinal("Email")),
            Phone           = r.IsDBNull(r.GetOrdinal("Phone"))       ? null : r.GetString(r.GetOrdinal("Phone")),
            IsPrimary       = r.GetBoolean(r.GetOrdinal("IsPrimary"))
        };
 
        private static VendorBankModel MapBank(SqlDataReader r) => new()
        {
            VendorBankId        = r.GetInt32(r.GetOrdinal("VendorBankId")),
            VendorId            = r.GetInt32(r.GetOrdinal("VendorId")),
            BankName            = r.GetString(r.GetOrdinal("BankName")),
            AccountHolderName   = r.GetString(r.GetOrdinal("AccountHolderName")),
            AccountNumber       = r.GetString(r.GetOrdinal("AccountNumber")),
            IFSCOrSwiftCode     = r.IsDBNull(r.GetOrdinal("IFSCOrSwiftCode")) ? null : r.GetString(r.GetOrdinal("IFSCOrSwiftCode")),
            BranchName          = r.IsDBNull(r.GetOrdinal("BranchName"))      ? null : r.GetString(r.GetOrdinal("BranchName")),
            IsPrimary           = r.GetBoolean(r.GetOrdinal("IsPrimary"))
        };
 
        private static VendorDocumentModel MapDocument(SqlDataReader r) => new()
        {
            VendorDocumentId = r.GetInt32(r.GetOrdinal("VendorDocumentId")),
            VendorId         = r.GetInt32(r.GetOrdinal("VendorId")),
            DocumentType     = r.GetString(r.GetOrdinal("DocumentType")),
            DocumentNumber   = r.IsDBNull(r.GetOrdinal("DocumentNumber")) ? null : r.GetString(r.GetOrdinal("DocumentNumber")),
            FilePath         = r.GetString(r.GetOrdinal("FilePath")),
            UploadedBy       = r.IsDBNull(r.GetOrdinal("UploadedBy")) ? null : (int?)r.GetInt32(r.GetOrdinal("UploadedBy")),
            UploadedDate     = r.GetDateTime(r.GetOrdinal("UploadedDate"))
        };

    }
}
