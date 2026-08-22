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
    public class StudentRepository : IStudentRepository
    {
        private readonly DBHelper _dbHelper;

        public StudentRepository(DBHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task<Guid> CreateAsync(Student s)
        {
            s.S_Id = s.S_Id == Guid.Empty ? Guid.NewGuid() : s.S_Id;

            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_Students_Create", conn);

            cmd.Parameters.Add("@S_Id", SqlDbType.UniqueIdentifier).Value = s.S_Id;
            cmd.Parameters.Add("@S_TenantId", SqlDbType.UniqueIdentifier).Value = s.S_TenantId;
            cmd.Parameters.Add("@S_BranchId", SqlDbType.UniqueIdentifier).Value = s.S_BranchId;
            cmd.Parameters.Add("@S_UserId", SqlDbType.UniqueIdentifier).Value = (object)s.S_UserId ?? DBNull.Value;
            cmd.Parameters.Add("@S_StudentCode", SqlDbType.NVarChar, 50).Value = s.S_StudentCode;
            cmd.Parameters.Add("@S_AdmissionNumber", SqlDbType.NVarChar, 50).Value = s.S_AdmissionNumber;
            cmd.Parameters.Add("@S_FirstName", SqlDbType.NVarChar, 100).Value = s.S_FirstName;
            cmd.Parameters.Add("@S_MiddleName", SqlDbType.NVarChar, 100).Value = (object)s.S_MiddleName ?? DBNull.Value;
            cmd.Parameters.Add("@S_LastName", SqlDbType.NVarChar, 100).Value = s.S_LastName;
            cmd.Parameters.Add("@S_DateOfBirth", SqlDbType.Date).Value = (object)s.S_DateOfBirth ?? DBNull.Value;
            cmd.Parameters.Add("@S_Gender", SqlDbType.NVarChar, 20).Value = (object)s.S_Gender ?? DBNull.Value;
            cmd.Parameters.Add("@S_Email", SqlDbType.NVarChar, 255).Value = (object)s.S_Email ?? DBNull.Value;
            cmd.Parameters.Add("@S_Phone", SqlDbType.NVarChar, 30).Value = (object)s.S_Phone ?? DBNull.Value;
            cmd.Parameters.Add("@S_AdmissionDate", SqlDbType.Date).Value = (object)s.S_AdmissionDate ?? DBNull.Value;
            cmd.Parameters.Add("@S_Status", SqlDbType.NVarChar, 20).Value = s.S_Status;

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            return s.S_Id;
        }

        public async Task<bool> UpdateAsync(Student s)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_Students_Update", conn);

            cmd.Parameters.Add("@S_Id", SqlDbType.UniqueIdentifier).Value = s.S_Id;
            cmd.Parameters.Add("@S_TenantId", SqlDbType.UniqueIdentifier).Value = s.S_TenantId;
            cmd.Parameters.Add("@S_BranchId", SqlDbType.UniqueIdentifier).Value = s.S_BranchId;
            cmd.Parameters.Add("@S_UserId", SqlDbType.UniqueIdentifier).Value = (object)s.S_UserId ?? DBNull.Value;
            cmd.Parameters.Add("@S_StudentCode", SqlDbType.NVarChar, 50).Value = s.S_StudentCode;
            cmd.Parameters.Add("@S_AdmissionNumber", SqlDbType.NVarChar, 50).Value = s.S_AdmissionNumber;
            cmd.Parameters.Add("@S_FirstName", SqlDbType.NVarChar, 100).Value = s.S_FirstName;
            cmd.Parameters.Add("@S_MiddleName", SqlDbType.NVarChar, 100).Value = (object)s.S_MiddleName ?? DBNull.Value;
            cmd.Parameters.Add("@S_LastName", SqlDbType.NVarChar, 100).Value = s.S_LastName;
            cmd.Parameters.Add("@S_DateOfBirth", SqlDbType.Date).Value = (object)s.S_DateOfBirth ?? DBNull.Value;
            cmd.Parameters.Add("@S_Gender", SqlDbType.NVarChar, 20).Value = (object)s.S_Gender ?? DBNull.Value;
            cmd.Parameters.Add("@S_Email", SqlDbType.NVarChar, 255).Value = (object)s.S_Email ?? DBNull.Value;
            cmd.Parameters.Add("@S_Phone", SqlDbType.NVarChar, 30).Value = (object)s.S_Phone ?? DBNull.Value;
            cmd.Parameters.Add("@S_AdmissionDate", SqlDbType.Date).Value = (object)s.S_AdmissionDate ?? DBNull.Value;
            cmd.Parameters.Add("@S_Status", SqlDbType.NVarChar, 20).Value = s.S_Status;

            await conn.OpenAsync();
            var rowsAffected = (int)await cmd.ExecuteScalarAsync(); // SP returns @@ROWCOUNT
            return rowsAffected > 0;
        }

        public async Task<bool> SoftDeleteAsync(Guid id, Guid tenantId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_Students_SoftDelete", conn);

            cmd.Parameters.Add("@S_Id", SqlDbType.UniqueIdentifier).Value = id;
            cmd.Parameters.Add("@S_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;

            await conn.OpenAsync();
            var rowsAffected = (int)await cmd.ExecuteScalarAsync(); // SP returns @@ROWCOUNT
            return rowsAffected > 0;
        }

        public async Task<Student> GetByIdAsync(Guid id, Guid tenantId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_Students_GetById", conn);

            cmd.Parameters.Add("@S_Id", SqlDbType.UniqueIdentifier).Value = id;
            cmd.Parameters.Add("@S_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? Map(reader) : null;
        }

        public async Task<(List<Student> Items, int TotalCount)> GetPagedAsync(
            Guid tenantId, string searchTerm, string status, Guid? branchId, int pageNumber, int pageSize)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_Students_GetPaged", conn);

            cmd.Parameters.Add("@S_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;
            cmd.Parameters.Add("@SearchTerm", SqlDbType.NVarChar, 255).Value = (object)searchTerm ?? DBNull.Value;
            cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = (object)status ?? DBNull.Value;
            cmd.Parameters.Add("@BranchId", SqlDbType.UniqueIdentifier).Value = (object)branchId ?? DBNull.Value;
            cmd.Parameters.Add("@PageNumber", SqlDbType.Int).Value = pageNumber;
            cmd.Parameters.Add("@PageSize", SqlDbType.Int).Value = pageSize;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            // Result set 1: total count
            int totalCount = 0;
            if (await reader.ReadAsync())
                totalCount = reader.GetInt32(reader.GetOrdinal("TotalCount"));

            // Result set 2: page of rows
            var items = new List<Student>();
            await reader.NextResultAsync();
            while (await reader.ReadAsync())
                items.Add(Map(reader));

            return (items, totalCount);
        }

        public async Task<bool> IsAdmissionNumberTakenAsync(Guid tenantId, string admissionNumber, Guid? excludeId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_Students_CheckAdmissionNumberExists", conn);

            cmd.Parameters.Add("@S_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;
            cmd.Parameters.Add("@AdmissionNumber", SqlDbType.NVarChar, 50).Value = admissionNumber;
            cmd.Parameters.Add("@ExcludeId", SqlDbType.UniqueIdentifier).Value = (object)excludeId ?? DBNull.Value;

            await conn.OpenAsync();
            return (int)await cmd.ExecuteScalarAsync() == 1;
        }

        public async Task<bool> IsStudentCodeTakenAsync(Guid tenantId, string studentCode, Guid? excludeId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_Students_CheckStudentCodeExists", conn);

            cmd.Parameters.Add("@S_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;
            cmd.Parameters.Add("@StudentCode", SqlDbType.NVarChar, 50).Value = studentCode;
            cmd.Parameters.Add("@ExcludeId", SqlDbType.UniqueIdentifier).Value = (object)excludeId ?? DBNull.Value;

            await conn.OpenAsync();
            return (int)await cmd.ExecuteScalarAsync() == 1;
        }

        // ---------- helpers ----------

        private static Student Map(SqlDataReader r) => new()
        {
            S_Id = r.GetGuid(r.GetOrdinal("S_Id")),
            S_TenantId = r.GetGuid(r.GetOrdinal("S_TenantId")),
            S_BranchId = r.GetGuid(r.GetOrdinal("S_BranchId")),
            S_UserId = r["S_UserId"] as Guid?,
            S_StudentCode = r["S_StudentCode"] as string,
            S_AdmissionNumber = r["S_AdmissionNumber"] as string,
            S_FirstName = r["S_FirstName"] as string,
            S_MiddleName = r["S_MiddleName"] as string,
            S_LastName = r["S_LastName"] as string,
            S_DateOfBirth = r["S_DateOfBirth"] as DateTime?,
            S_Gender = r["S_Gender"] as string,
            S_Email = r["S_Email"] as string,
            S_Phone = r["S_Phone"] as string,
            S_AdmissionDate = r["S_AdmissionDate"] as DateTime?,
            S_Status = r["S_Status"] as string,
            S_CreatedAt = r.GetDateTime(r.GetOrdinal("S_CreatedAt")),
            S_UpdatedAt = r.GetDateTime(r.GetOrdinal("S_UpdatedAt")),
            S_DeletedAt = r["S_DeletedAt"] as DateTime?
        };
    }
}
