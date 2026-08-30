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
    public class ExpenseDAL : IExpenseDAL
    {
        private readonly DBHelper _dbHelper;

        public ExpenseDAL(DBHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task<Expense> GetByIdAsync(Guid id, Guid tenantId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_Expenses_EXP", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "GetById";
            cmd.Parameters.Add("@EXP_Id", SqlDbType.UniqueIdentifier).Value = id;
            cmd.Parameters.Add("@EXP_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapExpense(reader) : null;
        }

        public async Task<(List<Expense> Items, int TotalCount)> GetPagedAsync(
            Guid tenantId, string searchTerm, Guid? branchId, Guid? expenseCategoryId,
            int pageNumber, int pageSize)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_Expenses_EXP", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "GetPaged";
            cmd.Parameters.Add("@EXP_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;
            cmd.Parameters.Add("@SearchTerm", SqlDbType.NVarChar, 255).Value = (object)searchTerm ?? DBNull.Value;
            cmd.Parameters.Add("@BranchId", SqlDbType.UniqueIdentifier).Value = (object)branchId ?? DBNull.Value;
            cmd.Parameters.Add("@ExpenseCategoryId", SqlDbType.UniqueIdentifier).Value = (object)expenseCategoryId ?? DBNull.Value;
            cmd.Parameters.Add("@PageNumber", SqlDbType.Int).Value = pageNumber;
            cmd.Parameters.Add("@PageSize", SqlDbType.Int).Value = pageSize;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            int totalCount = 0;
            if (await reader.ReadAsync())
                totalCount = reader.GetInt32(reader.GetOrdinal("TotalCount"));

            var items = new List<Expense>();
            await reader.NextResultAsync();
            while (await reader.ReadAsync())
                items.Add(MapExpense(reader));

            return (items, totalCount);
        }

        public async Task<string> GetNextExpenseNumberAsync(Guid tenantId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_Expenses_EXP", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "GetNextNumber";
            cmd.Parameters.Add("@EXP_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return result?.ToString() ?? "EXP-0001";
        }

        public async Task<Guid> CreateAsync(Expense expense)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_Expenses_EXP", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "Insert";
            AddParameters(cmd, expense);

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return Guid.TryParse(result?.ToString(), out var id) ? id : expense.EXP_Id;
        }

        public async Task<bool> UpdateAsync(Expense expense)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_Expenses_EXP", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "Update";
            AddParameters(cmd, expense);

            await conn.OpenAsync();
            return (int)await cmd.ExecuteScalarAsync() > 0;
        }

        public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_Expenses_EXP", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "Delete";
            cmd.Parameters.Add("@EXP_Id", SqlDbType.UniqueIdentifier).Value = id;
            cmd.Parameters.Add("@EXP_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;

            await conn.OpenAsync();
            return (int)await cmd.ExecuteScalarAsync() > 0;
        }

        private void AddParameters(SqlCommand cmd, Expense e)
        {
            cmd.Parameters.Add("@EXP_Id", SqlDbType.UniqueIdentifier).Value = e.EXP_Id;
            cmd.Parameters.Add("@EXP_TenantId", SqlDbType.UniqueIdentifier).Value = e.EXP_TenantId;
            cmd.Parameters.Add("@EXP_BranchId", SqlDbType.UniqueIdentifier).Value = e.EXP_BranchId;
            cmd.Parameters.Add("@EXP_ExpenseCategoryId", SqlDbType.UniqueIdentifier).Value = e.EXP_ExpenseCategoryId;
            cmd.Parameters.Add("@EXP_VendorId", SqlDbType.UniqueIdentifier).Value = (object)e.EXP_VendorId ?? DBNull.Value;
            cmd.Parameters.Add("@EXP_ExpenseNumber", SqlDbType.NVarChar, 50).Value = e.EXP_ExpenseNumber;
            cmd.Parameters.Add("@EXP_ExpenseDate", SqlDbType.Date).Value = e.EXP_ExpenseDate;
            cmd.Parameters.Add("@EXP_Amount", SqlDbType.Decimal).Value = e.EXP_Amount;
            cmd.Parameters.Add("@EXP_Description", SqlDbType.NVarChar, -1).Value = (object)e.EXP_Description ?? DBNull.Value;
            cmd.Parameters.Add("@EXP_PaymentMethodId", SqlDbType.UniqueIdentifier).Value = (object)e.EXP_PaymentMethodId ?? DBNull.Value;
            cmd.Parameters.Add("@EXP_CreatedBy", SqlDbType.UniqueIdentifier).Value = e.EXP_CreatedBy;
        }

        private static Expense MapExpense(SqlDataReader r) => new()
        {
            EXP_Id = r.GetGuid(r.GetOrdinal("EXP_Id")),
            EXP_TenantId = r.GetGuid(r.GetOrdinal("EXP_TenantId")),
            EXP_BranchId = r.GetGuid(r.GetOrdinal("EXP_BranchId")),
            EXP_ExpenseCategoryId = r.GetGuid(r.GetOrdinal("EXP_ExpenseCategoryId")),
            EXP_VendorId = r["EXP_VendorId"] as Guid?,
            EXP_ExpenseNumber = r["EXP_ExpenseNumber"] as string,
            EXP_ExpenseDate = r.GetDateTime(r.GetOrdinal("EXP_ExpenseDate")),
            EXP_Amount = r.GetDecimal(r.GetOrdinal("EXP_Amount")),
            EXP_Description = r["EXP_Description"] as string,
            EXP_PaymentMethodId = r["EXP_PaymentMethodId"] as Guid?,
            EXP_CreatedBy = r.GetGuid(r.GetOrdinal("EXP_CreatedBy")),
            EXP_CreatedAt = r.GetDateTime(r.GetOrdinal("EXP_CreatedAt")),
            EXP_UpdatedAt = r.GetDateTime(r.GetOrdinal("EXP_UpdatedAt")),
            BranchName = r["BranchName"] as string,
            ExpenseCategoryName = r["ExpenseCategoryName"] as string,
            VendorName = r["VendorName"] as string,
            PaymentMethodName = r["PaymentMethodName"] as string
        };
    }
}
