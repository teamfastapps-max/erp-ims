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
    public class ExpenseCategoryDAL : IExpenseCategoryDAL
    {
        private readonly DBHelper _dbHelper;

        public ExpenseCategoryDAL(DBHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task<List<ExpenseCategory>> GetAllAsync(Guid tenantId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_ExpenseCategories_EC", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "GetAll";
            cmd.Parameters.Add("@EC_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            var items = new List<ExpenseCategory>();
            while (await reader.ReadAsync())
            {
                items.Add(new ExpenseCategory
                {
                    EC_Id = reader.GetGuid(reader.GetOrdinal("EC_Id")),
                    EC_TenantId = reader.GetGuid(reader.GetOrdinal("EC_TenantId")),
                    EC_Name = reader["EC_Name"] as string,
                    EC_Code = reader["EC_Code"] as string,
                    EC_Description = reader["EC_Description"] as string,
                    EC_CreatedAt = reader.GetDateTime(reader.GetOrdinal("EC_CreatedAt")),
                    EC_UpdatedAt = reader.GetDateTime(reader.GetOrdinal("EC_UpdatedAt"))
                });
            }
            return items;
        }
    }
}
