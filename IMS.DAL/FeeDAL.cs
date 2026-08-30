using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using IMS.DAL.Common;
using IMS.DAL.Interfaces;
using IMS.Models.Entities;

namespace IMS.DAL.Repositories
{
    public class FeeStructureDAL : IFeeStructureDAL
    {
        private readonly DBHelper _dbHelper;
        public FeeStructureDAL(DBHelper dbHelper) { _dbHelper = dbHelper; }

        public async Task<FeeStructure> GetByIdAsync(Guid id, Guid tenantId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_FeeStructures_FS", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "GetById";
            cmd.Parameters.Add("@FS_Id", SqlDbType.UniqueIdentifier).Value = id;
            cmd.Parameters.Add("@FS_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapFeeStructure(reader) : null;
        }

        public async Task<(List<FeeStructure> Items, int TotalCount)> GetPagedAsync(
            Guid tenantId, string searchTerm, Guid? academicYearId, int pageNumber, int pageSize)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_FeeStructures_FS", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "GetPaged";
            cmd.Parameters.Add("@FS_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;
            cmd.Parameters.Add("@SearchTerm", SqlDbType.NVarChar, 255).Value = (object)searchTerm ?? DBNull.Value;
            cmd.Parameters.Add("@AcademicYearId", SqlDbType.UniqueIdentifier).Value = (object)academicYearId ?? DBNull.Value;
            cmd.Parameters.Add("@PageNumber", SqlDbType.Int).Value = pageNumber;
            cmd.Parameters.Add("@PageSize", SqlDbType.Int).Value = pageSize;
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            int totalCount = 0;
            if (await reader.ReadAsync()) totalCount = reader.GetInt32(reader.GetOrdinal("TotalCount"));
            var items = new List<FeeStructure>();
            await reader.NextResultAsync();
            while (await reader.ReadAsync()) items.Add(MapFeeStructure(reader));
            return (items, totalCount);
        }

        public async Task<bool> IsCodeTakenAsync(Guid tenantId, string code, Guid? excludeId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_FeeStructures_FS", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "ExistsByCode";
            cmd.Parameters.Add("@FS_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;
            cmd.Parameters.Add("@FS_Code", SqlDbType.NVarChar, 50).Value = code;
            cmd.Parameters.Add("@ExcludeId", SqlDbType.UniqueIdentifier).Value = (object)excludeId ?? DBNull.Value;
            await conn.OpenAsync();
            return (int)await cmd.ExecuteScalarAsync() == 1;
        }

        public async Task<Guid> CreateAsync(FeeStructure fs)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_FeeStructures_FS", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "Insert";
            AddParameters(cmd, fs);
            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return Guid.TryParse(result?.ToString(), out var id) ? id : fs.FS_Id;
        }

        public async Task<bool> UpdateAsync(FeeStructure fs)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_FeeStructures_FS", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "Update";
            AddParameters(cmd, fs);
            await conn.OpenAsync();
            return (int)await cmd.ExecuteScalarAsync() > 0;
        }

        public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_FeeStructures_FS", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "Delete";
            cmd.Parameters.Add("@FS_Id", SqlDbType.UniqueIdentifier).Value = id;
            cmd.Parameters.Add("@FS_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;
            await conn.OpenAsync();
            return (int)await cmd.ExecuteScalarAsync() > 0;
        }

        private void AddParameters(SqlCommand cmd, FeeStructure fs)
        {
            cmd.Parameters.Add("@FS_Id", SqlDbType.UniqueIdentifier).Value = fs.FS_Id;
            cmd.Parameters.Add("@FS_TenantId", SqlDbType.UniqueIdentifier).Value = fs.FS_TenantId;
            cmd.Parameters.Add("@FS_Name", SqlDbType.NVarChar, 150).Value = fs.FS_Name;
            cmd.Parameters.Add("@FS_Code", SqlDbType.NVarChar, 50).Value = fs.FS_Code;
            cmd.Parameters.Add("@FS_CourseId", SqlDbType.UniqueIdentifier).Value = (object)fs.FS_CourseId ?? DBNull.Value;
            cmd.Parameters.Add("@FS_BatchId", SqlDbType.UniqueIdentifier).Value = (object)fs.FS_BatchId ?? DBNull.Value;
            cmd.Parameters.Add("@FS_AcademicYearId", SqlDbType.UniqueIdentifier).Value = fs.FS_AcademicYearId;
            cmd.Parameters.Add("@FS_Description", SqlDbType.NVarChar, -1).Value = (object)fs.FS_Description ?? DBNull.Value;
            cmd.Parameters.Add("@FS_IsActive", SqlDbType.Bit).Value = fs.FS_IsActive;
        }

        private static FeeStructure MapFeeStructure(SqlDataReader r) => new()
        {
            FS_Id = r.GetGuid(r.GetOrdinal("FS_Id")),
            FS_TenantId = r.GetGuid(r.GetOrdinal("FS_TenantId")),
            FS_Name = r["FS_Name"] as string,
            FS_Code = r["FS_Code"] as string,
            FS_CourseId = r["FS_CourseId"] as Guid?,
            FS_BatchId = r["FS_BatchId"] as Guid?,
            FS_AcademicYearId = r.GetGuid(r.GetOrdinal("FS_AcademicYearId")),
            FS_Description = r["FS_Description"] as string,
            FS_IsActive = r.GetBoolean(r.GetOrdinal("FS_IsActive")),
            FS_CreatedAt = r.GetDateTime(r.GetOrdinal("FS_CreatedAt")),
            FS_UpdatedAt = r.GetDateTime(r.GetOrdinal("FS_UpdatedAt")),
            CourseName = r["CourseName"] as string,
            BatchName = r["BatchName"] as string,
            AcademicYearName = r["AcademicYearName"] as string
        };
    }

    public class FeeStructureItemDAL : IFeeStructureItemDAL
    {
        private readonly DBHelper _dbHelper;
        public FeeStructureItemDAL(DBHelper dbHelper) { _dbHelper = dbHelper; }

        public async Task<List<FeeStructureItem>> GetByFeeStructureIdAsync(Guid feeStructureId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_FeeStructureItems_FSI", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "GetByFeeStructure";
            cmd.Parameters.Add("@FSI_FeeStructureId", SqlDbType.UniqueIdentifier).Value = feeStructureId;
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            var items = new List<FeeStructureItem>();
            while (await reader.ReadAsync())
            {
                items.Add(new FeeStructureItem
                {
                    FSI_Id = reader.GetGuid(reader.GetOrdinal("FSI_Id")),
                    FSI_FeeStructureId = reader.GetGuid(reader.GetOrdinal("FSI_FeeStructureId")),
                    FSI_FeeCategoryId = reader.GetGuid(reader.GetOrdinal("FSI_FeeCategoryId")),
                    FSI_Amount = reader.GetDecimal(reader.GetOrdinal("FSI_Amount")),
                    FSI_DueDays = reader["FSI_DueDays"] as int?,
                    FSI_IsMandatory = reader.GetBoolean(reader.GetOrdinal("FSI_IsMandatory")),
                    FeeCategoryName = reader["FeeCategoryName"] as string
                });
            }
            return items;
        }

        public async Task<bool> SaveItemsAsync(Guid feeStructureId, List<FeeStructureItem> items)
        {
            using var conn = _dbHelper.GetConnection();
            await conn.OpenAsync();
            using var deleteCmd = _dbHelper.CreateCommand("USP_FeeStructureItems_FSI", conn);
            deleteCmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "DeleteByFeeStructure";
            deleteCmd.Parameters.Add("@FSI_FeeStructureId", SqlDbType.UniqueIdentifier).Value = feeStructureId;
            await deleteCmd.ExecuteNonQueryAsync();

            foreach (var item in items)
            {
                using var cmd = _dbHelper.CreateCommand("USP_FeeStructureItems_FSI", conn);
                cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "Insert";
                cmd.Parameters.Add("@FSI_Id", SqlDbType.UniqueIdentifier).Value = item.FSI_Id;
                cmd.Parameters.Add("@FSI_FeeStructureId", SqlDbType.UniqueIdentifier).Value = feeStructureId;
                cmd.Parameters.Add("@FSI_FeeCategoryId", SqlDbType.UniqueIdentifier).Value = item.FSI_FeeCategoryId;
                cmd.Parameters.Add("@FSI_Amount", SqlDbType.Decimal).Value = item.FSI_Amount;
                cmd.Parameters.Add("@FSI_DueDays", SqlDbType.Int).Value = (object)item.FSI_DueDays ?? DBNull.Value;
                cmd.Parameters.Add("@FSI_IsMandatory", SqlDbType.Bit).Value = item.FSI_IsMandatory;
                await cmd.ExecuteNonQueryAsync();
            }
            return true;
        }
    }

    public class FeeInvoiceDAL : IFeeInvoiceDAL
    {
        private readonly DBHelper _dbHelper;
        public FeeInvoiceDAL(DBHelper dbHelper) { _dbHelper = dbHelper; }

        public async Task<FeeInvoice> GetByIdAsync(Guid id, Guid tenantId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_FeeInvoices_FI", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "GetById";
            cmd.Parameters.Add("@FI_Id", SqlDbType.UniqueIdentifier).Value = id;
            cmd.Parameters.Add("@FI_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapInvoice(reader) : null;
        }

        public async Task<(List<FeeInvoice> Items, int TotalCount)> GetPagedAsync(
            Guid tenantId, string searchTerm, string status, int pageNumber, int pageSize)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_FeeInvoices_FI", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "GetPaged";
            cmd.Parameters.Add("@FI_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;
            cmd.Parameters.Add("@SearchTerm", SqlDbType.NVarChar, 255).Value = (object)searchTerm ?? DBNull.Value;
            cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = (object)status ?? DBNull.Value;
            cmd.Parameters.Add("@PageNumber", SqlDbType.Int).Value = pageNumber;
            cmd.Parameters.Add("@PageSize", SqlDbType.Int).Value = pageSize;
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            int totalCount = 0;
            if (await reader.ReadAsync()) totalCount = reader.GetInt32(reader.GetOrdinal("TotalCount"));
            var items = new List<FeeInvoice>();
            await reader.NextResultAsync();
            while (await reader.ReadAsync()) items.Add(MapInvoice(reader));
            return (items, totalCount);
        }

        public async Task<string> GetNextInvoiceNumberAsync(Guid tenantId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_FeeInvoices_FI", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "GetNextNumber";
            cmd.Parameters.Add("@FI_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;
            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return result?.ToString() ?? "INV-0001";
        }

        public async Task<Guid> CreateAsync(FeeInvoice invoice)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_FeeInvoices_FI", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "Insert";
            AddParameters(cmd, invoice);
            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return Guid.TryParse(result?.ToString(), out var id) ? id : invoice.FI_Id;
        }

        public async Task<bool> UpdateAsync(FeeInvoice invoice)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_FeeInvoices_FI", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "Update";
            AddParameters(cmd, invoice);
            await conn.OpenAsync();
            return (int)await cmd.ExecuteScalarAsync() > 0;
        }

        public async Task<bool> UpdatePaidAmountAsync(Guid invoiceId, decimal paidAmount)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_FeeInvoices_FI", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "UpdatePaidAmount";
            cmd.Parameters.Add("@FI_Id", SqlDbType.UniqueIdentifier).Value = invoiceId;
            cmd.Parameters.Add("@FI_PaidAmount", SqlDbType.Decimal).Value = paidAmount;
            await conn.OpenAsync();
            return (int)await cmd.ExecuteScalarAsync() > 0;
        }

        public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_FeeInvoices_FI", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "Delete";
            cmd.Parameters.Add("@FI_Id", SqlDbType.UniqueIdentifier).Value = id;
            cmd.Parameters.Add("@FI_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;
            await conn.OpenAsync();
            return (int)await cmd.ExecuteScalarAsync() > 0;
        }

        private void AddParameters(SqlCommand cmd, FeeInvoice fi)
        {
            cmd.Parameters.Add("@FI_Id", SqlDbType.UniqueIdentifier).Value = fi.FI_Id;
            cmd.Parameters.Add("@FI_TenantId", SqlDbType.UniqueIdentifier).Value = fi.FI_TenantId;
            cmd.Parameters.Add("@FI_StudentId", SqlDbType.UniqueIdentifier).Value = fi.FI_StudentId;
            cmd.Parameters.Add("@FI_InvoiceNumber", SqlDbType.NVarChar, 50).Value = fi.FI_InvoiceNumber;
            cmd.Parameters.Add("@FI_InvoiceDate", SqlDbType.Date).Value = fi.FI_InvoiceDate;
            cmd.Parameters.Add("@FI_DueDate", SqlDbType.Date).Value = fi.FI_DueDate;
            cmd.Parameters.Add("@FI_Subtotal", SqlDbType.Decimal).Value = fi.FI_Subtotal;
            cmd.Parameters.Add("@FI_DiscountAmount", SqlDbType.Decimal).Value = fi.FI_DiscountAmount;
            cmd.Parameters.Add("@FI_TaxAmount", SqlDbType.Decimal).Value = fi.FI_TaxAmount;
            cmd.Parameters.Add("@FI_TotalAmount", SqlDbType.Decimal).Value = fi.FI_TotalAmount;
            cmd.Parameters.Add("@FI_PaidAmount", SqlDbType.Decimal).Value = fi.FI_PaidAmount;
            cmd.Parameters.Add("@FI_BalanceAmount", SqlDbType.Decimal).Value = fi.FI_BalanceAmount;
            cmd.Parameters.Add("@FI_Status", SqlDbType.NVarChar, 20).Value = fi.FI_Status;
            cmd.Parameters.Add("@FI_Notes", SqlDbType.NVarChar, -1).Value = (object)fi.FI_Notes ?? DBNull.Value;
        }

        private static FeeInvoice MapInvoice(SqlDataReader r) => new()
        {
            FI_Id = r.GetGuid(r.GetOrdinal("FI_Id")),
            FI_TenantId = r.GetGuid(r.GetOrdinal("FI_TenantId")),
            FI_StudentId = r.GetGuid(r.GetOrdinal("FI_StudentId")),
            FI_InvoiceNumber = r["FI_InvoiceNumber"] as string,
            FI_InvoiceDate = r.GetDateTime(r.GetOrdinal("FI_InvoiceDate")),
            FI_DueDate = r.GetDateTime(r.GetOrdinal("FI_DueDate")),
            FI_Subtotal = r.GetDecimal(r.GetOrdinal("FI_Subtotal")),
            FI_DiscountAmount = r.GetDecimal(r.GetOrdinal("FI_DiscountAmount")),
            FI_TaxAmount = r.GetDecimal(r.GetOrdinal("FI_TaxAmount")),
            FI_TotalAmount = r.GetDecimal(r.GetOrdinal("FI_TotalAmount")),
            FI_PaidAmount = r.GetDecimal(r.GetOrdinal("FI_PaidAmount")),
            FI_BalanceAmount = r.GetDecimal(r.GetOrdinal("FI_BalanceAmount")),
            FI_Status = r["FI_Status"] as string,
            FI_Notes = r["FI_Notes"] as string,
            FI_CreatedAt = r.GetDateTime(r.GetOrdinal("FI_CreatedAt")),
            FI_UpdatedAt = r.GetDateTime(r.GetOrdinal("FI_UpdatedAt")),
            StudentName = r["StudentName"] as string,
            StudentCode = r["StudentCode"] as string
        };
    }

    public class PaymentDAL : IPaymentDAL
    {
        private readonly DBHelper _dbHelper;
        public PaymentDAL(DBHelper dbHelper) { _dbHelper = dbHelper; }

        public async Task<Payment> GetByIdAsync(Guid id, Guid tenantId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_Payments_PAY", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "GetById";
            cmd.Parameters.Add("@PAY_Id", SqlDbType.UniqueIdentifier).Value = id;
            cmd.Parameters.Add("@PAY_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapPayment(reader) : null;
        }

        public async Task<(List<Payment> Items, int TotalCount)> GetPagedAsync(
            Guid tenantId, string searchTerm, string status, int pageNumber, int pageSize)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_Payments_PAY", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "GetPaged";
            cmd.Parameters.Add("@PAY_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;
            cmd.Parameters.Add("@SearchTerm", SqlDbType.NVarChar, 255).Value = (object)searchTerm ?? DBNull.Value;
            cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = (object)status ?? DBNull.Value;
            cmd.Parameters.Add("@PageNumber", SqlDbType.Int).Value = pageNumber;
            cmd.Parameters.Add("@PageSize", SqlDbType.Int).Value = pageSize;
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            int totalCount = 0;
            if (await reader.ReadAsync()) totalCount = reader.GetInt32(reader.GetOrdinal("TotalCount"));
            var items = new List<Payment>();
            await reader.NextResultAsync();
            while (await reader.ReadAsync()) items.Add(MapPayment(reader));
            return (items, totalCount);
        }

        public async Task<string> GetNextPaymentNumberAsync(Guid tenantId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_Payments_PAY", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "GetNextNumber";
            cmd.Parameters.Add("@PAY_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;
            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return result?.ToString() ?? "PAY-0001";
        }

        public async Task<Guid> CreateAsync(Payment payment)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_Payments_PAY", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "Insert";
            AddParameters(cmd, payment);
            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return Guid.TryParse(result?.ToString(), out var id) ? id : payment.PAY_Id;
        }

        public async Task<bool> UpdateAsync(Payment payment)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_Payments_PAY", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "Update";
            AddParameters(cmd, payment);
            await conn.OpenAsync();
            return (int)await cmd.ExecuteScalarAsync() > 0;
        }

        public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_Payments_PAY", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "Delete";
            cmd.Parameters.Add("@PAY_Id", SqlDbType.UniqueIdentifier).Value = id;
            cmd.Parameters.Add("@PAY_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;
            await conn.OpenAsync();
            return (int)await cmd.ExecuteScalarAsync() > 0;
        }

        private void AddParameters(SqlCommand cmd, Payment p)
        {
            cmd.Parameters.Add("@PAY_Id", SqlDbType.UniqueIdentifier).Value = p.PAY_Id;
            cmd.Parameters.Add("@PAY_TenantId", SqlDbType.UniqueIdentifier).Value = p.PAY_TenantId;
            cmd.Parameters.Add("@PAY_StudentId", SqlDbType.UniqueIdentifier).Value = p.PAY_StudentId;
            cmd.Parameters.Add("@PAY_PaymentNumber", SqlDbType.NVarChar, 50).Value = p.PAY_PaymentNumber;
            cmd.Parameters.Add("@PAY_PaymentDate", SqlDbType.DateTime2).Value = p.PAY_PaymentDate;
            cmd.Parameters.Add("@PAY_Amount", SqlDbType.Decimal).Value = p.PAY_Amount;
            cmd.Parameters.Add("@PAY_PaymentMethodId", SqlDbType.UniqueIdentifier).Value = p.PAY_PaymentMethodId;
            cmd.Parameters.Add("@PAY_Status", SqlDbType.NVarChar, 20).Value = p.PAY_Status;
            cmd.Parameters.Add("@PAY_TransactionReference", SqlDbType.NVarChar, 150).Value = (object)p.PAY_TransactionReference ?? DBNull.Value;
            cmd.Parameters.Add("@PAY_Notes", SqlDbType.NVarChar, -1).Value = (object)p.PAY_Notes ?? DBNull.Value;
            cmd.Parameters.Add("@PAY_CreatedBy", SqlDbType.UniqueIdentifier).Value = p.PAY_CreatedBy;
        }

        private static Payment MapPayment(SqlDataReader r) => new()
        {
            PAY_Id = r.GetGuid(r.GetOrdinal("PAY_Id")),
            PAY_TenantId = r.GetGuid(r.GetOrdinal("PAY_TenantId")),
            PAY_StudentId = r.GetGuid(r.GetOrdinal("PAY_StudentId")),
            PAY_PaymentNumber = r["PAY_PaymentNumber"] as string,
            PAY_PaymentDate = r.GetDateTime(r.GetOrdinal("PAY_PaymentDate")),
            PAY_Amount = r.GetDecimal(r.GetOrdinal("PAY_Amount")),
            PAY_PaymentMethodId = r.GetGuid(r.GetOrdinal("PAY_PaymentMethodId")),
            PAY_Status = r["PAY_Status"] as string,
            PAY_TransactionReference = r["PAY_TransactionReference"] as string,
            PAY_Notes = r["PAY_Notes"] as string,
            PAY_CreatedBy = r.GetGuid(r.GetOrdinal("PAY_CreatedBy")),
            PAY_CreatedAt = r.GetDateTime(r.GetOrdinal("PAY_CreatedAt")),
            PAY_UpdatedAt = r.GetDateTime(r.GetOrdinal("PAY_UpdatedAt")),
            StudentName = r["StudentName"] as string,
            StudentCode = r["StudentCode"] as string,
            PaymentMethodName = r["PaymentMethodName"] as string
        };
    }
}
