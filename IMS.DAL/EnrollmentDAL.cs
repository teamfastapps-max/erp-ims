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
    public class EnrollmentDAL : IEnrollmentDAL
    {
        private readonly DBHelper _dbHelper;
        public EnrollmentDAL(DBHelper dbHelper) { _dbHelper = dbHelper; }

        public async Task<Enrollment> GetByIdAsync(Guid id, Guid tenantId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_Enrollments_E", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "GetById";
            cmd.Parameters.Add("@E_Id", SqlDbType.UniqueIdentifier).Value = id;
            cmd.Parameters.Add("@E_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? Map(reader) : null;
        }

        public async Task<(List<Enrollment> Items, int TotalCount)> GetPagedAsync(Guid tenantId, string searchTerm,
            Guid? academicYearId, Guid? courseId, Guid? batchId, string status, int page, int pageSize)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_Enrollments_E", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "GetPaged";
            cmd.Parameters.Add("@E_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;
            cmd.Parameters.Add("@SearchTerm", SqlDbType.NVarChar, 255).Value = (object)searchTerm ?? DBNull.Value;
            cmd.Parameters.Add("@AcademicYearId", SqlDbType.UniqueIdentifier).Value = (object)academicYearId ?? DBNull.Value;
            cmd.Parameters.Add("@CourseId", SqlDbType.UniqueIdentifier).Value = (object)courseId ?? DBNull.Value;
            cmd.Parameters.Add("@BatchId", SqlDbType.UniqueIdentifier).Value = (object)batchId ?? DBNull.Value;
            cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = (object)status ?? DBNull.Value;
            cmd.Parameters.Add("@PageNumber", SqlDbType.Int).Value = page;
            cmd.Parameters.Add("@PageSize", SqlDbType.Int).Value = pageSize;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            int totalCount = 0;
            if (await reader.ReadAsync()) totalCount = reader.GetInt32(reader.GetOrdinal("TotalCount"));
            var items = new List<Enrollment>();
            await reader.NextResultAsync();
            while (await reader.ReadAsync()) items.Add(Map(reader));
            return (items, totalCount);
        }

        public async Task<bool> IsDuplicateAsync(Guid tenantId, Guid studentId, Guid batchId, Guid? excludeId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_Enrollments_E", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "IsDuplicate";
            cmd.Parameters.Add("@E_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;
            cmd.Parameters.Add("@E_StudentId", SqlDbType.UniqueIdentifier).Value = studentId;
            cmd.Parameters.Add("@E_BatchId", SqlDbType.UniqueIdentifier).Value = batchId;
            cmd.Parameters.Add("@ExcludeId", SqlDbType.UniqueIdentifier).Value = (object)excludeId ?? DBNull.Value;
            await conn.OpenAsync();
            return (int)await cmd.ExecuteScalarAsync() == 1;
        }

        public async Task<Guid> CreateAsync(Enrollment e)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_Enrollments_E", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "Insert";
            AddParams(cmd, e);
            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return Guid.TryParse(result?.ToString(), out var id) ? id : e.E_Id;
        }

        public async Task<bool> UpdateAsync(Enrollment e)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_Enrollments_E", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "Update";
            AddParams(cmd, e);
            await conn.OpenAsync();
            return (int)await cmd.ExecuteScalarAsync() > 0;
        }

        public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_Enrollments_E", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "Delete";
            cmd.Parameters.Add("@E_Id", SqlDbType.UniqueIdentifier).Value = id;
            cmd.Parameters.Add("@E_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;
            await conn.OpenAsync();
            return (int)await cmd.ExecuteScalarAsync() > 0;
        }

        private void AddParams(SqlCommand cmd, Enrollment e)
        {
            cmd.Parameters.Add("@E_Id", SqlDbType.UniqueIdentifier).Value = e.E_Id;
            cmd.Parameters.Add("@E_TenantId", SqlDbType.UniqueIdentifier).Value = e.E_TenantId;
            cmd.Parameters.Add("@E_StudentId", SqlDbType.UniqueIdentifier).Value = e.E_StudentId;
            cmd.Parameters.Add("@E_AcademicYearId", SqlDbType.UniqueIdentifier).Value = e.E_AcademicYearId;
            cmd.Parameters.Add("@E_CourseId", SqlDbType.UniqueIdentifier).Value = e.E_CourseId;
            cmd.Parameters.Add("@E_BatchId", SqlDbType.UniqueIdentifier).Value = e.E_BatchId;
            cmd.Parameters.Add("@E_EnrollmentNumber", SqlDbType.NVarChar, 50).Value = (object)e.E_EnrollmentNumber ?? DBNull.Value;
            cmd.Parameters.Add("@E_EnrollmentDate", SqlDbType.Date).Value = e.E_EnrollmentDate;
            cmd.Parameters.Add("@E_Status", SqlDbType.NVarChar, 20).Value = e.E_Status;
            cmd.Parameters.Add("@E_CompletionDate", SqlDbType.Date).Value = (object)e.E_CompletionDate ?? DBNull.Value;
        }

        private static Enrollment Map(SqlDataReader r) => new()
        {
            E_Id = r.GetGuid(r.GetOrdinal("E_Id")),
            E_TenantId = r.GetGuid(r.GetOrdinal("E_TenantId")),
            E_StudentId = r.GetGuid(r.GetOrdinal("E_StudentId")),
            E_AcademicYearId = r.GetGuid(r.GetOrdinal("E_AcademicYearId")),
            E_CourseId = r.GetGuid(r.GetOrdinal("E_CourseId")),
            E_BatchId = r.GetGuid(r.GetOrdinal("E_BatchId")),
            E_EnrollmentNumber = r["E_EnrollmentNumber"] as string,
            E_EnrollmentDate = r.GetDateTime(r.GetOrdinal("E_EnrollmentDate")),
            E_Status = r["E_Status"] as string,
            E_CompletionDate = r["E_CompletionDate"] as DateTime?,
            E_CreatedAt = r.GetDateTime(r.GetOrdinal("E_CreatedAt")),
            E_UpdatedAt = r.GetDateTime(r.GetOrdinal("E_UpdatedAt")),
            StudentName = r["StudentName"] as string,
            CourseName = r["CourseName"] as string,
            BatchName = r["BatchName"] as string,
            AcademicYearName = r["AcademicYearName"] as string
        };
    }
}
