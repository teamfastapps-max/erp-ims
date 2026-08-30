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
    public class TimetableDAL : ITimetableDAL
    {
        private readonly DBHelper _dbHelper;
        public TimetableDAL(DBHelper dbHelper) { _dbHelper = dbHelper; }

        public async Task<Timetable> GetByIdAsync(Guid id, Guid tenantId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_Timetables_TT", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "GetById";
            cmd.Parameters.Add("@TT_Id", SqlDbType.UniqueIdentifier).Value = id;
            cmd.Parameters.Add("@TT_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? Map(reader) : null;
        }

        public async Task<List<Timetable>> GetAllAsync(Guid tenantId, Guid? batchId, Guid? branchId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_Timetables_TT", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "GetAll";
            cmd.Parameters.Add("@TT_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;
            cmd.Parameters.Add("@BatchId", SqlDbType.UniqueIdentifier).Value = (object)batchId ?? DBNull.Value;
            cmd.Parameters.Add("@BranchId", SqlDbType.UniqueIdentifier).Value = (object)branchId ?? DBNull.Value;
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            var list = new List<Timetable>();
            while (await reader.ReadAsync()) list.Add(Map(reader));
            return list;
        }

        public async Task<bool> HasConflictAsync(Guid tenantId, Guid batchId, int dayOfWeek, TimeSpan start, TimeSpan end, Guid? excludeId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_Timetables_TT", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "CheckConflict";
            cmd.Parameters.Add("@TT_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;
            cmd.Parameters.Add("@TT_BatchId", SqlDbType.UniqueIdentifier).Value = batchId;
            cmd.Parameters.Add("@TT_DayOfWeek", SqlDbType.SmallInt).Value = dayOfWeek;
            cmd.Parameters.Add("@TT_StartTime", SqlDbType.Time).Value = start;
            cmd.Parameters.Add("@TT_EndTime", SqlDbType.Time).Value = end;
            cmd.Parameters.Add("@ExcludeId", SqlDbType.UniqueIdentifier).Value = (object)excludeId ?? DBNull.Value;
            await conn.OpenAsync();
            return (int)await cmd.ExecuteScalarAsync() > 0;
        }

        public async Task<Guid> CreateAsync(Timetable t)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_Timetables_TT", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "Insert";
            AddParams(cmd, t);
            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return Guid.TryParse(result?.ToString(), out var id) ? id : t.TT_Id;
        }

        public async Task<bool> UpdateAsync(Timetable t)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_Timetables_TT", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "Update";
            AddParams(cmd, t);
            await conn.OpenAsync();
            return (int)await cmd.ExecuteScalarAsync() > 0;
        }

        public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_Timetables_TT", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "Delete";
            cmd.Parameters.Add("@TT_Id", SqlDbType.UniqueIdentifier).Value = id;
            cmd.Parameters.Add("@TT_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;
            await conn.OpenAsync();
            return (int)await cmd.ExecuteScalarAsync() > 0;
        }

        private void AddParams(SqlCommand cmd, Timetable t)
        {
            cmd.Parameters.Add("@TT_Id", SqlDbType.UniqueIdentifier).Value = t.TT_Id;
            cmd.Parameters.Add("@TT_TenantId", SqlDbType.UniqueIdentifier).Value = t.TT_TenantId;
            cmd.Parameters.Add("@TT_BranchId", SqlDbType.UniqueIdentifier).Value = t.TT_BranchId;
            cmd.Parameters.Add("@TT_BatchId", SqlDbType.UniqueIdentifier).Value = t.TT_BatchId;
            cmd.Parameters.Add("@TT_SubjectId", SqlDbType.UniqueIdentifier).Value = t.TT_SubjectId;
            cmd.Parameters.Add("@TT_StaffId", SqlDbType.UniqueIdentifier).Value = t.TT_StaffId;
            cmd.Parameters.Add("@TT_ClassroomId", SqlDbType.UniqueIdentifier).Value = (object)t.TT_ClassroomId ?? DBNull.Value;
            cmd.Parameters.Add("@TT_DayOfWeek", SqlDbType.SmallInt).Value = t.TT_DayOfWeek;
            cmd.Parameters.Add("@TT_StartTime", SqlDbType.Time).Value = t.TT_StartTime;
            cmd.Parameters.Add("@TT_EndTime", SqlDbType.Time).Value = t.TT_EndTime;
            cmd.Parameters.Add("@TT_EffectiveFrom", SqlDbType.Date).Value = (object)t.TT_EffectiveFrom ?? DBNull.Value;
            cmd.Parameters.Add("@TT_EffectiveTo", SqlDbType.Date).Value = (object)t.TT_EffectiveTo ?? DBNull.Value;
        }

        private static Timetable Map(SqlDataReader r) => new()
        {
            TT_Id = r.GetGuid(r.GetOrdinal("TT_Id")),
            TT_TenantId = r.GetGuid(r.GetOrdinal("TT_TenantId")),
            TT_BranchId = r.GetGuid(r.GetOrdinal("TT_BranchId")),
            TT_BatchId = r.GetGuid(r.GetOrdinal("TT_BatchId")),
            TT_SubjectId = r.GetGuid(r.GetOrdinal("TT_SubjectId")),
            TT_StaffId = r.GetGuid(r.GetOrdinal("TT_StaffId")),
            TT_ClassroomId = r["TT_ClassroomId"] as Guid?,
            TT_DayOfWeek = Convert.ToInt32(r["TT_DayOfWeek"]),
            TT_StartTime = (TimeSpan)r["TT_StartTime"],
            TT_EndTime = (TimeSpan)r["TT_EndTime"],
            TT_EffectiveFrom = r["TT_EffectiveFrom"] as DateTime?,
            TT_EffectiveTo = r["TT_EffectiveTo"] as DateTime?,
            TT_CreatedAt = r.GetDateTime(r.GetOrdinal("TT_CreatedAt")),
            TT_UpdatedAt = r.GetDateTime(r.GetOrdinal("TT_UpdatedAt")),
            SubjectName = r["SubjectName"] as string,
            StaffName = r["StaffName"] as string,
            ClassroomName = r["ClassroomName"] as string,
            BatchName = r["BatchName"] as string
        };
    }
}
