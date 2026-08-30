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
    public class AdmissionApplicationDAL : IAdmissionApplicationDAL
    {
        private readonly DBHelper _dbHelper;
        public AdmissionApplicationDAL(DBHelper dbHelper) { _dbHelper = dbHelper; }

        public async Task<AdmissionApplication> GetByIdAsync(Guid id, Guid tenantId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_AdmissionApplications_AA", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "GetById";
            cmd.Parameters.Add("@AA_Id", SqlDbType.UniqueIdentifier).Value = id;
            cmd.Parameters.Add("@AA_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? Map(reader) : null;
        }

        public async Task<(List<AdmissionApplication> Items, int TotalCount)> GetPagedAsync(Guid tenantId, string searchTerm,
            Guid? branchId, Guid? courseId, Guid? academicYearId, string status, int page, int pageSize)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_AdmissionApplications_AA", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "GetPaged";
            cmd.Parameters.Add("@AA_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;
            cmd.Parameters.Add("@SearchTerm", SqlDbType.NVarChar, 255).Value = (object)searchTerm ?? DBNull.Value;
            cmd.Parameters.Add("@BranchId", SqlDbType.UniqueIdentifier).Value = (object)branchId ?? DBNull.Value;
            cmd.Parameters.Add("@CourseId", SqlDbType.UniqueIdentifier).Value = (object)courseId ?? DBNull.Value;
            cmd.Parameters.Add("@AcademicYearId", SqlDbType.UniqueIdentifier).Value = (object)academicYearId ?? DBNull.Value;
            cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = (object)status ?? DBNull.Value;
            cmd.Parameters.Add("@PageNumber", SqlDbType.Int).Value = page;
            cmd.Parameters.Add("@PageSize", SqlDbType.Int).Value = pageSize;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            int totalCount = 0;
            if (await reader.ReadAsync()) totalCount = reader.GetInt32(reader.GetOrdinal("TotalCount"));
            var items = new List<AdmissionApplication>();
            await reader.NextResultAsync();
            while (await reader.ReadAsync()) items.Add(Map(reader));
            return (items, totalCount);
        }

        public async Task<bool> IsApplicationNumberTakenAsync(Guid tenantId, string number, Guid? excludeId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_AdmissionApplications_AA", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "ExistsByNumber";
            cmd.Parameters.Add("@AA_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;
            cmd.Parameters.Add("@AA_ApplicationNumber", SqlDbType.NVarChar, 50).Value = number;
            cmd.Parameters.Add("@ExcludeId", SqlDbType.UniqueIdentifier).Value = (object)excludeId ?? DBNull.Value;
            await conn.OpenAsync();
            return (int)await cmd.ExecuteScalarAsync() == 1;
        }

        public async Task<Guid> CreateAsync(AdmissionApplication a)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_AdmissionApplications_AA", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "Insert";
            AddParams(cmd, a);
            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return Guid.TryParse(result?.ToString(), out var id) ? id : a.AA_Id;
        }

        public async Task<bool> UpdateAsync(AdmissionApplication a)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_AdmissionApplications_AA", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "Update";
            AddParams(cmd, a);
            await conn.OpenAsync();
            return (int)await cmd.ExecuteScalarAsync() > 0;
        }

        public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_AdmissionApplications_AA", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "Delete";
            cmd.Parameters.Add("@AA_Id", SqlDbType.UniqueIdentifier).Value = id;
            cmd.Parameters.Add("@AA_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;
            await conn.OpenAsync();
            return (int)await cmd.ExecuteScalarAsync() > 0;
        }

        public async Task<bool> ReviewAsync(Guid id, string status, string notes, Guid tenantId, Guid reviewedBy)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_AdmissionApplications_AA", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "Review";
            cmd.Parameters.Add("@AA_Id", SqlDbType.UniqueIdentifier).Value = id;
            cmd.Parameters.Add("@AA_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;
            cmd.Parameters.Add("@AA_Status", SqlDbType.NVarChar, 20).Value = status;
            cmd.Parameters.Add("@AA_Notes", SqlDbType.NVarChar, -1).Value = (object)notes ?? DBNull.Value;
            cmd.Parameters.Add("@AA_ReviewedBy", SqlDbType.UniqueIdentifier).Value = reviewedBy;
            await conn.OpenAsync();
            return (int)await cmd.ExecuteScalarAsync() > 0;
        }

        private void AddParams(SqlCommand cmd, AdmissionApplication a)
        {
            cmd.Parameters.Add("@AA_Id", SqlDbType.UniqueIdentifier).Value = a.AA_Id;
            cmd.Parameters.Add("@AA_TenantId", SqlDbType.UniqueIdentifier).Value = a.AA_TenantId;
            cmd.Parameters.Add("@AA_BranchId", SqlDbType.UniqueIdentifier).Value = a.AA_BranchId;
            cmd.Parameters.Add("@AA_ApplicationNumber", SqlDbType.NVarChar, 50).Value = (object)a.AA_ApplicationNumber ?? DBNull.Value;
            cmd.Parameters.Add("@AA_FirstName", SqlDbType.NVarChar, 100).Value = a.AA_FirstName;
            cmd.Parameters.Add("@AA_LastName", SqlDbType.NVarChar, 100).Value = a.AA_LastName;
            cmd.Parameters.Add("@AA_DateOfBirth", SqlDbType.Date).Value = (object)a.AA_DateOfBirth ?? DBNull.Value;
            cmd.Parameters.Add("@AA_Gender", SqlDbType.NVarChar, 20).Value = (object)a.AA_Gender ?? DBNull.Value;
            cmd.Parameters.Add("@AA_Email", SqlDbType.NVarChar, 255).Value = (object)a.AA_Email ?? DBNull.Value;
            cmd.Parameters.Add("@AA_Phone", SqlDbType.NVarChar, 30).Value = a.AA_Phone;
            cmd.Parameters.Add("@AA_CourseId", SqlDbType.UniqueIdentifier).Value = (object)a.AA_CourseId ?? DBNull.Value;
            cmd.Parameters.Add("@AA_AcademicYearId", SqlDbType.UniqueIdentifier).Value = a.AA_AcademicYearId;
            cmd.Parameters.Add("@AA_Status", SqlDbType.NVarChar, 20).Value = a.AA_Status;
            cmd.Parameters.Add("@AA_SubmittedAt", SqlDbType.DateTime2).Value = (object)a.AA_SubmittedAt ?? DBNull.Value;
            cmd.Parameters.Add("@AA_Notes", SqlDbType.NVarChar, -1).Value = (object)a.AA_Notes ?? DBNull.Value;
        }

        private static AdmissionApplication Map(SqlDataReader r) => new()
        {
            AA_Id = r.GetGuid(r.GetOrdinal("AA_Id")),
            AA_TenantId = r.GetGuid(r.GetOrdinal("AA_TenantId")),
            AA_BranchId = r.GetGuid(r.GetOrdinal("AA_BranchId")),
            AA_ApplicationNumber = r["AA_ApplicationNumber"] as string,
            AA_FirstName = r["AA_FirstName"] as string,
            AA_LastName = r["AA_LastName"] as string,
            AA_DateOfBirth = r["AA_DateOfBirth"] as DateTime?,
            AA_Gender = r["AA_Gender"] as string,
            AA_Email = r["AA_Email"] as string,
            AA_Phone = r["AA_Phone"] as string,
            AA_CourseId = r["AA_CourseId"] as Guid?,
            AA_AcademicYearId = r.GetGuid(r.GetOrdinal("AA_AcademicYearId")),
            AA_Status = r["AA_Status"] as string,
            AA_SubmittedAt = r["AA_SubmittedAt"] as DateTime?,
            AA_ReviewedAt = r["AA_ReviewedAt"] as DateTime?,
            AA_ReviewedBy = r["AA_ReviewedBy"] as Guid?,
            AA_Notes = r["AA_Notes"] as string,
            AA_CreatedAt = r.GetDateTime(r.GetOrdinal("AA_CreatedAt")),
            AA_UpdatedAt = r.GetDateTime(r.GetOrdinal("AA_UpdatedAt")),
            CourseName = r["CourseName"] as string,
            AcademicYearName = r["AcademicYearName"] as string
        };
    }
}
