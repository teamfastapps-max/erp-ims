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
    public class BatchDAL : IBatchDAL
    {
        private readonly DBHelper _dbHelper;

        public BatchDAL(DBHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task<Batch> GetByIdAsync(Guid id, Guid tenantId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_Batches_BT", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "GetById";
            cmd.Parameters.Add("@BT_Id", SqlDbType.UniqueIdentifier).Value = id;
            cmd.Parameters.Add("@BT_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapBatch(reader) : null;
        }

        public async Task<(List<Batch> Items, int TotalCount)> GetPagedAsync(
            Guid tenantId, string searchTerm, Guid? branchId, Guid? courseId,
            Guid? academicYearId, string status, int pageNumber, int pageSize)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_Batches_BT", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "GetPaged";
            cmd.Parameters.Add("@BT_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;
            cmd.Parameters.Add("@SearchTerm", SqlDbType.NVarChar, 255).Value = (object)searchTerm ?? DBNull.Value;
            cmd.Parameters.Add("@BranchId", SqlDbType.UniqueIdentifier).Value = (object)branchId ?? DBNull.Value;
            cmd.Parameters.Add("@CourseId", SqlDbType.UniqueIdentifier).Value = (object)courseId ?? DBNull.Value;
            cmd.Parameters.Add("@AcademicYearId", SqlDbType.UniqueIdentifier).Value = (object)academicYearId ?? DBNull.Value;
            cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = (object)status ?? DBNull.Value;
            cmd.Parameters.Add("@PageNumber", SqlDbType.Int).Value = pageNumber;
            cmd.Parameters.Add("@PageSize", SqlDbType.Int).Value = pageSize;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            int totalCount = 0;
            if (await reader.ReadAsync())
                totalCount = reader.GetInt32(reader.GetOrdinal("TotalCount"));

            var items = new List<Batch>();
            await reader.NextResultAsync();
            while (await reader.ReadAsync())
                items.Add(MapBatch(reader));

            return (items, totalCount);
        }

        public async Task<bool> IsCodeTakenAsync(Guid tenantId, string code, Guid? excludeId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_Batches_BT", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "ExistsByCode";
            cmd.Parameters.Add("@BT_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;
            cmd.Parameters.Add("@BT_Code", SqlDbType.NVarChar, 50).Value = code;
            cmd.Parameters.Add("@ExcludeId", SqlDbType.UniqueIdentifier).Value = (object)excludeId ?? DBNull.Value;

            await conn.OpenAsync();
            return (int)await cmd.ExecuteScalarAsync() == 1;
        }

        public async Task<Guid> CreateAsync(Batch batch)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_Batches_BT", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "Insert";
            AddParameters(cmd, batch);

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return Guid.TryParse(result?.ToString(), out var id) ? id : batch.BT_Id;
        }

        public async Task<bool> UpdateAsync(Batch batch)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_Batches_BT", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "Update";
            AddParameters(cmd, batch);

            await conn.OpenAsync();
            return (int)await cmd.ExecuteScalarAsync() > 0;
        }

        public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_Batches_BT", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "Delete";
            cmd.Parameters.Add("@BT_Id", SqlDbType.UniqueIdentifier).Value = id;
            cmd.Parameters.Add("@BT_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;

            await conn.OpenAsync();
            return (int)await cmd.ExecuteScalarAsync() > 0;
        }

        private void AddParameters(SqlCommand cmd, Batch b)
        {
            cmd.Parameters.Add("@BT_Id", SqlDbType.UniqueIdentifier).Value = b.BT_Id;
            cmd.Parameters.Add("@BT_TenantId", SqlDbType.UniqueIdentifier).Value = b.BT_TenantId;
            cmd.Parameters.Add("@BT_BranchId", SqlDbType.UniqueIdentifier).Value = b.BT_BranchId;
            cmd.Parameters.Add("@BT_CourseId", SqlDbType.UniqueIdentifier).Value = b.BT_CourseId;
            cmd.Parameters.Add("@BT_AcademicYearId", SqlDbType.UniqueIdentifier).Value = b.BT_AcademicYearId;
            cmd.Parameters.Add("@BT_Name", SqlDbType.NVarChar, 150).Value = b.BT_Name;
            cmd.Parameters.Add("@BT_Code", SqlDbType.NVarChar, 50).Value = b.BT_Code;
            cmd.Parameters.Add("@BT_StartDate", SqlDbType.Date).Value = b.BT_StartDate;
            cmd.Parameters.Add("@BT_EndDate", SqlDbType.Date).Value = (object)b.BT_EndDate ?? DBNull.Value;
            cmd.Parameters.Add("@BT_Capacity", SqlDbType.Int).Value = (object)b.BT_Capacity ?? DBNull.Value;
            cmd.Parameters.Add("@BT_Status", SqlDbType.NVarChar, 20).Value = b.BT_Status;
        }

        private static Batch MapBatch(SqlDataReader r) => new()
        {
            BT_Id = r.GetGuid(r.GetOrdinal("BT_Id")),
            BT_TenantId = r.GetGuid(r.GetOrdinal("BT_TenantId")),
            BT_BranchId = r.GetGuid(r.GetOrdinal("BT_BranchId")),
            BT_CourseId = r.GetGuid(r.GetOrdinal("BT_CourseId")),
            BT_AcademicYearId = r.GetGuid(r.GetOrdinal("BT_AcademicYearId")),
            BT_Name = r["BT_Name"] as string,
            BT_Code = r["BT_Code"] as string,
            BT_StartDate = r.GetDateTime(r.GetOrdinal("BT_StartDate")),
            BT_EndDate = r["BT_EndDate"] as DateTime?,
            BT_Capacity = r["BT_Capacity"] as int?,
            BT_Status = r["BT_Status"] as string,
            BT_CreatedAt = r.GetDateTime(r.GetOrdinal("BT_CreatedAt")),
            BT_UpdatedAt = r.GetDateTime(r.GetOrdinal("BT_UpdatedAt")),
            CourseName = r["CourseName"] as string,
            AcademicYearName = r["AcademicYearName"] as string,
            EnrolledCount = r["EnrolledCount"] as int? ?? 0
        };
    }
}
