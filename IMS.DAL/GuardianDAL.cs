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
    public class GuardianDAL : IGuardianDAL
    {
        private readonly DBHelper _dbHelper;

        public GuardianDAL(DBHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task<List<GuardianSearchResult>> SearchAsync(Guid tenantId, string searchTerm)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_Guardians_Search", conn);
            cmd.Parameters.Add("@G_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;
            cmd.Parameters.Add("@SearchTerm", SqlDbType.NVarChar, 255).Value = searchTerm ?? string.Empty;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            var results = new List<GuardianSearchResult>();
            while (await reader.ReadAsync())
            {
                results.Add(new GuardianSearchResult
                {
                    G_Id = reader.GetGuid(reader.GetOrdinal("G_Id")),
                    G_FirstName = reader["G_FirstName"] as string,
                    G_LastName = reader["G_LastName"] as string,
                    G_Phone = reader["G_Phone"] as string,
                    G_Email = reader["G_Email"] as string,
                    G_Occupation = reader["G_Occupation"] as string
                });
            }
            return results;
        }

        public async Task<Guardian> GetByIdAsync(Guid id, Guid tenantId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_Guardians_GetById", conn);
            cmd.Parameters.Add("@G_Id", SqlDbType.UniqueIdentifier).Value = id;
            cmd.Parameters.Add("@G_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) return null;

            return new Guardian
            {
                G_Id = reader.GetGuid(reader.GetOrdinal("G_Id")),
                G_TenantId = reader.GetGuid(reader.GetOrdinal("G_TenantId")),
                G_FirstName = reader["G_FirstName"] as string,
                G_LastName = reader["G_LastName"] as string,
                G_Phone = reader["G_Phone"] as string,
                G_Email = reader["G_Email"] as string,
                G_Occupation = reader["G_Occupation"] as string,
                G_CreatedAt = reader.GetDateTime(reader.GetOrdinal("G_CreatedAt")),
                G_UpdatedAt = reader.GetDateTime(reader.GetOrdinal("G_UpdatedAt")),
                G_DeletedAt = reader["G_DeletedAt"] as DateTime?
            };
        }
    }
}
