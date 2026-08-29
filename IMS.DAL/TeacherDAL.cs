using IMS.DAL.Common;
using IMS.DAL.Interfaces;
using IMS.Models.Teacher;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.DAL
{
    public class TeacherDAL : ITeacherDAL
    {
        private readonly DBHelper _dbHelper;

        public TeacherDAL(DBHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task<Teacher> GetByIdAsync(Guid id, Guid tenantId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_Teachers_GetById", conn);
            cmd.Parameters.Add("@T_Id", SqlDbType.UniqueIdentifier).Value = id;
            cmd.Parameters.Add("@T_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapTeacher(reader) : null;
        }

        public async Task<List<Teacher>> GetProfilesByIdsAsync(List<Guid> ids, Guid tenantId)
        {
            var list = new List<Teacher>();
            if (ids == null || ids.Count == 0) return list;

            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_Teachers_GetByIds", conn);
            cmd.Parameters.Add("@IdsCsv", SqlDbType.NVarChar, -1).Value = string.Join(",", ids);
            cmd.Parameters.Add("@T_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                list.Add(MapTeacher(reader));

            return list;
        }

        public async Task<bool> IsEmployeeCodeTakenAsync(Guid tenantId, string employeeCode, Guid? excludeId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_Teachers_CheckEmployeeCodeExists", conn);
            cmd.Parameters.Add("@T_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;
            cmd.Parameters.Add("@EmployeeCode", SqlDbType.NVarChar, 50).Value = employeeCode;
            cmd.Parameters.Add("@ExcludeId", SqlDbType.UniqueIdentifier).Value = (object)excludeId ?? DBNull.Value;

            await conn.OpenAsync();
            return (int)await cmd.ExecuteScalarAsync() == 1;
        }

        public async Task CreateTeacherProfileAsync(Teacher t)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_Teachers_Create", conn);
            AddTeacherParameters(cmd, t);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<bool> UpdateTeacherProfileAsync(Teacher t)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_Teachers_Update", conn);
            AddTeacherParameters(cmd, t);

            await conn.OpenAsync();
            var rowsAffected = (int)await cmd.ExecuteScalarAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> SoftDeleteAsync(Guid id, Guid tenantId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_Teachers_SoftDelete", conn);
            cmd.Parameters.Add("@T_Id", SqlDbType.UniqueIdentifier).Value = id;
            cmd.Parameters.Add("@T_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;

            await conn.OpenAsync();
            var rowsAffected = (int)await cmd.ExecuteScalarAsync();
            return rowsAffected > 0;
        }

        private static void AddTeacherParameters(SqlCommand cmd, Teacher t)
        {
            cmd.Parameters.Add("@T_Id", SqlDbType.UniqueIdentifier).Value = t.T_Id;
            cmd.Parameters.Add("@T_TenantId", SqlDbType.UniqueIdentifier).Value = t.T_TenantId;
            cmd.Parameters.Add("@T_BranchId", SqlDbType.UniqueIdentifier).Value = t.T_BranchId;
            cmd.Parameters.Add("@T_EmployeeCode", SqlDbType.NVarChar, 50).Value = t.T_EmployeeCode;
            cmd.Parameters.Add("@T_Designation", SqlDbType.NVarChar, 100).Value = (object)t.T_Designation ?? DBNull.Value;
            cmd.Parameters.Add("@T_Department", SqlDbType.NVarChar, 100).Value = (object)t.T_Department ?? DBNull.Value;
            cmd.Parameters.Add("@T_JoiningDate", SqlDbType.Date).Value = (object)t.T_JoiningDate ?? DBNull.Value;
            cmd.Parameters.Add("@T_Qualification", SqlDbType.NVarChar, 255).Value = (object)t.T_Qualification ?? DBNull.Value;
            cmd.Parameters.Add("@T_ExperienceYears", SqlDbType.Int).Value = (object)t.T_ExperienceYears ?? DBNull.Value;
            cmd.Parameters.Add("@T_BloodGroup", SqlDbType.NVarChar, 10).Value = (object)t.T_BloodGroup ?? DBNull.Value;
            cmd.Parameters.Add("@T_Status", SqlDbType.NVarChar, 20).Value = t.T_Status;
        }

        private static Teacher MapTeacher(SqlDataReader r) => new()
        {
            T_Id = r.GetGuid(r.GetOrdinal("T_Id")),
            T_TenantId = r.GetGuid(r.GetOrdinal("T_TenantId")),
            T_BranchId = r.GetGuid(r.GetOrdinal("T_BranchId")),
            T_EmployeeCode = r["T_EmployeeCode"] as string,
            T_Designation = r["T_Designation"] as string,
            T_Department = r["T_Department"] as string,
            T_JoiningDate = r["T_JoiningDate"] as DateTime?,
            T_Qualification = r["T_Qualification"] as string,
            T_ExperienceYears = r["T_ExperienceYears"] as int?,
            T_BloodGroup = r["T_BloodGroup"] as string,
            T_Status = r["T_Status"] as string,
            T_CreatedAt = r.GetDateTime(r.GetOrdinal("T_CreatedAt")),
            T_UpdatedAt = r.GetDateTime(r.GetOrdinal("T_UpdatedAt")),
            T_DeletedAt = r["T_DeletedAt"] as DateTime?
        };
    }
}
