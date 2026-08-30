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
    public class AttendanceSessionDAL : IAttendanceSessionDAL
    {
        private readonly DBHelper _dbHelper;

        public AttendanceSessionDAL(DBHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task<AttendanceSession> GetByIdAsync(Guid id, Guid tenantId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_AttendanceSessions_AS", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "GetById";
            cmd.Parameters.Add("@AS_Id", SqlDbType.UniqueIdentifier).Value = id;
            cmd.Parameters.Add("@AS_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapSession(reader) : null;
        }

        public async Task<(List<AttendanceSession> Items, int TotalCount)> GetPagedAsync(
            Guid tenantId, string searchTerm, Guid? branchId, Guid? batchId,
            DateTime? date, int pageNumber, int pageSize)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_AttendanceSessions_AS", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "GetPaged";
            cmd.Parameters.Add("@AS_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;
            cmd.Parameters.Add("@SearchTerm", SqlDbType.NVarChar, 255).Value = (object)searchTerm ?? DBNull.Value;
            cmd.Parameters.Add("@BranchId", SqlDbType.UniqueIdentifier).Value = (object)branchId ?? DBNull.Value;
            cmd.Parameters.Add("@BatchId", SqlDbType.UniqueIdentifier).Value = (object)batchId ?? DBNull.Value;
            cmd.Parameters.Add("@Date", SqlDbType.Date).Value = (object)date ?? DBNull.Value;
            cmd.Parameters.Add("@PageNumber", SqlDbType.Int).Value = pageNumber;
            cmd.Parameters.Add("@PageSize", SqlDbType.Int).Value = pageSize;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            int totalCount = 0;
            if (await reader.ReadAsync())
                totalCount = reader.GetInt32(reader.GetOrdinal("TotalCount"));

            var items = new List<AttendanceSession>();
            await reader.NextResultAsync();
            while (await reader.ReadAsync())
                items.Add(MapSession(reader));

            return (items, totalCount);
        }

        public async Task<Guid> CreateAsync(AttendanceSession session)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_AttendanceSessions_AS", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "Insert";
            AddParameters(cmd, session);

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return Guid.TryParse(result?.ToString(), out var id) ? id : session.AS_Id;
        }

        public async Task<bool> UpdateAsync(AttendanceSession session)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_AttendanceSessions_AS", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "Update";
            AddParameters(cmd, session);

            await conn.OpenAsync();
            return (int)await cmd.ExecuteScalarAsync() > 0;
        }

        public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_AttendanceSessions_AS", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "Delete";
            cmd.Parameters.Add("@AS_Id", SqlDbType.UniqueIdentifier).Value = id;
            cmd.Parameters.Add("@AS_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;

            await conn.OpenAsync();
            return (int)await cmd.ExecuteScalarAsync() > 0;
        }

        private void AddParameters(SqlCommand cmd, AttendanceSession s)
        {
            cmd.Parameters.Add("@AS_Id", SqlDbType.UniqueIdentifier).Value = s.AS_Id;
            cmd.Parameters.Add("@AS_TenantId", SqlDbType.UniqueIdentifier).Value = s.AS_TenantId;
            cmd.Parameters.Add("@AS_BranchId", SqlDbType.UniqueIdentifier).Value = s.AS_BranchId;
            cmd.Parameters.Add("@AS_BatchId", SqlDbType.UniqueIdentifier).Value = s.AS_BatchId;
            cmd.Parameters.Add("@AS_SubjectId", SqlDbType.UniqueIdentifier).Value = (object)s.AS_SubjectId ?? DBNull.Value;
            cmd.Parameters.Add("@AS_StaffId", SqlDbType.UniqueIdentifier).Value = (object)s.AS_StaffId ?? DBNull.Value;
            cmd.Parameters.Add("@AS_AttendanceDate", SqlDbType.Date).Value = s.AS_AttendanceDate;
            cmd.Parameters.Add("@AS_StartTime", SqlDbType.Time).Value = (object)s.AS_StartTime ?? DBNull.Value;
            cmd.Parameters.Add("@AS_EndTime", SqlDbType.Time).Value = (object)s.AS_EndTime ?? DBNull.Value;
            cmd.Parameters.Add("@AS_Remarks", SqlDbType.NVarChar, -1).Value = (object)s.AS_Remarks ?? DBNull.Value;
        }

        private static AttendanceSession MapSession(SqlDataReader r) => new()
        {
            AS_Id = r.GetGuid(r.GetOrdinal("AS_Id")),
            AS_TenantId = r.GetGuid(r.GetOrdinal("AS_TenantId")),
            AS_BranchId = r.GetGuid(r.GetOrdinal("AS_BranchId")),
            AS_BatchId = r.GetGuid(r.GetOrdinal("AS_BatchId")),
            AS_SubjectId = r["AS_SubjectId"] as Guid?,
            AS_StaffId = r["AS_StaffId"] as Guid?,
            AS_AttendanceDate = r.GetDateTime(r.GetOrdinal("AS_AttendanceDate")),
            AS_StartTime = r["AS_StartTime"] as TimeSpan?,
            AS_EndTime = r["AS_EndTime"] as TimeSpan?,
            AS_Remarks = r["AS_Remarks"] as string,
            AS_CreatedAt = r.GetDateTime(r.GetOrdinal("AS_CreatedAt")),
            AS_UpdatedAt = r.GetDateTime(r.GetOrdinal("AS_UpdatedAt")),
            BranchName = r["BranchName"] as string,
            BatchName = r["BatchName"] as string,
            SubjectName = r["SubjectName"] as string,
            StaffName = r["StaffName"] as string
        };
    }

    public class AttendanceRecordDAL : IAttendanceRecordDAL
    {
        private readonly DBHelper _dbHelper;

        public AttendanceRecordDAL(DBHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task<List<AttendanceRecord>> GetBySessionIdAsync(Guid sessionId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_AttendanceRecords_AR", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "GetBySession";
            cmd.Parameters.Add("@AR_AttendanceSessionId", SqlDbType.UniqueIdentifier).Value = sessionId;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            var items = new List<AttendanceRecord>();
            while (await reader.ReadAsync())
            {
                items.Add(new AttendanceRecord
                {
                    AR_Id = reader.GetGuid(reader.GetOrdinal("AR_Id")),
                    AR_AttendanceSessionId = reader.GetGuid(reader.GetOrdinal("AR_AttendanceSessionId")),
                    AR_StudentId = reader.GetGuid(reader.GetOrdinal("AR_StudentId")),
                    AR_Status = reader["AR_Status"] as string,
                    AR_Remarks = reader["AR_Remarks"] as string,
                    AR_CreatedAt = reader.GetDateTime(reader.GetOrdinal("AR_CreatedAt")),
                    AR_UpdatedAt = reader.GetDateTime(reader.GetOrdinal("AR_UpdatedAt")),
                    StudentName = reader["StudentName"] as string,
                    StudentCode = reader["StudentCode"] as string
                });
            }
            return items;
        }

        public async Task<bool> SaveRecordsAsync(Guid sessionId, List<AttendanceRecord> records)
        {
            using var conn = _dbHelper.GetConnection();
            await conn.OpenAsync();

            using var deleteCmd = _dbHelper.CreateCommand("USP_AttendanceRecords_AR", conn);
            deleteCmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "DeleteBySession";
            deleteCmd.Parameters.Add("@AR_AttendanceSessionId", SqlDbType.UniqueIdentifier).Value = sessionId;
            await deleteCmd.ExecuteNonQueryAsync();

            foreach (var record in records)
            {
                using var cmd = _dbHelper.CreateCommand("USP_AttendanceRecords_AR", conn);
                cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "Insert";
                cmd.Parameters.Add("@AR_Id", SqlDbType.UniqueIdentifier).Value = record.AR_Id;
                cmd.Parameters.Add("@AR_AttendanceSessionId", SqlDbType.UniqueIdentifier).Value = sessionId;
                cmd.Parameters.Add("@AR_StudentId", SqlDbType.UniqueIdentifier).Value = record.AR_StudentId;
                cmd.Parameters.Add("@AR_Status", SqlDbType.NVarChar, 20).Value = record.AR_Status;
                cmd.Parameters.Add("@AR_Remarks", SqlDbType.NVarChar, -1).Value = (object)record.AR_Remarks ?? DBNull.Value;
                await cmd.ExecuteNonQueryAsync();
            }

            return true;
        }
    }
}
