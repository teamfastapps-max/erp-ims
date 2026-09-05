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
    public class TeacherLeaveDAL : ITeacherLeaveDAL
    {
        private readonly DBHelper _dbHelper;

        public TeacherLeaveDAL(DBHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task<LeaveApplyResult> ApplyAsync(TeacherLeave leave)
        {
            leave.TL_Id = leave.TL_Id == Guid.Empty ? Guid.NewGuid() : leave.TL_Id;

            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_TeacherLeaves_Apply", conn);

            cmd.Parameters.Add("@TL_Id", SqlDbType.UniqueIdentifier).Value = leave.TL_Id;
            cmd.Parameters.Add("@TL_TenantId", SqlDbType.UniqueIdentifier).Value = leave.TL_TenantId;
            cmd.Parameters.Add("@TL_TeacherId", SqlDbType.UniqueIdentifier).Value = leave.TL_TeacherId;
            cmd.Parameters.Add("@TL_TeacherName", SqlDbType.NVarChar, 200).Value = leave.TL_TeacherName;
            cmd.Parameters.Add("@TL_LeaveType", SqlDbType.NVarChar, 30).Value = leave.TL_LeaveType;
            cmd.Parameters.Add("@TL_FromDate", SqlDbType.Date).Value = leave.TL_FromDate;
            cmd.Parameters.Add("@TL_ToDate", SqlDbType.Date).Value = leave.TL_ToDate;
            cmd.Parameters.Add("@TL_Reason", SqlDbType.NVarChar, 500).Value = (object)leave.TL_Reason ?? DBNull.Value;
            cmd.Parameters.Add("@TL_AppliedBy", SqlDbType.UniqueIdentifier).Value = leave.TL_AppliedBy;

            await conn.OpenAsync();

            // SqlException from THROW inside the SP (overlap / bad date range)
            // propagates naturally here - the service layer catches it and
            // maps it to a clean ServiceResult.Fail(message) for the UI.
            using var reader = await cmd.ExecuteReaderAsync();
            await reader.ReadAsync();
            return new LeaveApplyResult
            {
                TL_Id = reader.GetGuid(reader.GetOrdinal("TL_Id")),
                TotalDays = reader.GetInt32(reader.GetOrdinal("TotalDays"))
            };
        }

        public async Task<bool> ApproveAsync(Guid id, Guid tenantId, Guid approvedBy)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_TeacherLeaves_Approve", conn);
            cmd.Parameters.Add("@TL_Id", SqlDbType.UniqueIdentifier).Value = id;
            cmd.Parameters.Add("@TL_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;
            cmd.Parameters.Add("@ApprovedBy", SqlDbType.UniqueIdentifier).Value = approvedBy;

            await conn.OpenAsync();
            return (int)await cmd.ExecuteScalarAsync() > 0;
        }

        public async Task<bool> RejectAsync(Guid id, Guid tenantId, Guid approvedBy, string rejectionReason)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_TeacherLeaves_Reject", conn);
            cmd.Parameters.Add("@TL_Id", SqlDbType.UniqueIdentifier).Value = id;
            cmd.Parameters.Add("@TL_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;
            cmd.Parameters.Add("@ApprovedBy", SqlDbType.UniqueIdentifier).Value = approvedBy;
            cmd.Parameters.Add("@RejectionReason", SqlDbType.NVarChar, 500).Value = rejectionReason;

            await conn.OpenAsync();
            return (int)await cmd.ExecuteScalarAsync() > 0;
        }

        public async Task<bool> CancelAsync(Guid id, Guid tenantId, Guid requestingTeacherId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_TeacherLeaves_Cancel", conn);
            cmd.Parameters.Add("@TL_Id", SqlDbType.UniqueIdentifier).Value = id;
            cmd.Parameters.Add("@TL_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;
            cmd.Parameters.Add("@RequestingTeacherId", SqlDbType.UniqueIdentifier).Value = requestingTeacherId;

            await conn.OpenAsync();
            return (int)await cmd.ExecuteScalarAsync() > 0;
        }

        public async Task<(List<TeacherLeave> Items, int TotalCount)> GetAllPagedAsync(Guid tenantId, string status, int pageNumber, int pageSize)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_TeacherLeaves_GetAllPaged", conn);
            cmd.Parameters.Add("@TL_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;
            cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = (object)status ?? DBNull.Value;
            cmd.Parameters.Add("@PageNumber", SqlDbType.Int).Value = pageNumber;
            cmd.Parameters.Add("@PageSize", SqlDbType.Int).Value = pageSize;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            int total = 0;
            if (await reader.ReadAsync()) total = reader.GetInt32(reader.GetOrdinal("TotalCount"));

            var items = new List<TeacherLeave>();
            await reader.NextResultAsync();
            while (await reader.ReadAsync()) items.Add(Map(reader));

            return (items, total);
        }

        public async Task<(List<TeacherLeave> Items, int TotalCount)> GetByTeacherIdAsync(Guid tenantId, Guid teacherId, int pageNumber, int pageSize)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_TeacherLeaves_GetByTeacherId", conn);
            cmd.Parameters.Add("@TL_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;
            cmd.Parameters.Add("@TL_TeacherId", SqlDbType.UniqueIdentifier).Value = teacherId;
            cmd.Parameters.Add("@PageNumber", SqlDbType.Int).Value = pageNumber;
            cmd.Parameters.Add("@PageSize", SqlDbType.Int).Value = pageSize;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            int total = 0;
            if (await reader.ReadAsync()) total = reader.GetInt32(reader.GetOrdinal("TotalCount"));

            var items = new List<TeacherLeave>();
            await reader.NextResultAsync();
            while (await reader.ReadAsync()) items.Add(Map(reader));

            return (items, total);
        }

        private static TeacherLeave Map(SqlDataReader r) => new()
        {
            TL_Id = r.GetGuid(r.GetOrdinal("TL_Id")),
            TL_TenantId = r.GetGuid(r.GetOrdinal("TL_TenantId")),
            TL_TeacherId = r.GetGuid(r.GetOrdinal("TL_TeacherId")),
            TL_TeacherName = r["TL_TeacherName"] as string,
            TL_LeaveType = r["TL_LeaveType"] as string,
            TL_FromDate = r.GetDateTime(r.GetOrdinal("TL_FromDate")),
            TL_ToDate = r.GetDateTime(r.GetOrdinal("TL_ToDate")),
            TL_TotalDays = r.GetInt32(r.GetOrdinal("TL_TotalDays")),
            TL_Reason = r["TL_Reason"] as string,
            TL_Status = r["TL_Status"] as string,
            TL_AppliedAt = r.GetDateTime(r.GetOrdinal("TL_AppliedAt")),
            TL_AppliedBy = r.GetGuid(r.GetOrdinal("TL_AppliedBy")),
            TL_ApprovedBy = r["TL_ApprovedBy"] as Guid?,
            TL_ApprovedAt = r["TL_ApprovedAt"] as DateTime?,
            TL_RejectionReason = r["TL_RejectionReason"] as string,
            TL_CreatedAt = r.GetDateTime(r.GetOrdinal("TL_CreatedAt")),
            TL_UpdatedAt = r.GetDateTime(r.GetOrdinal("TL_UpdatedAt"))
        };
        public async Task<LeaveApplyResult> UpdateAsync(TeacherLeave leave, Guid requestingTeacherId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_TeacherLeaves_Update", conn);

            cmd.Parameters.Add("@TL_Id", SqlDbType.UniqueIdentifier).Value = leave.TL_Id;
            cmd.Parameters.Add("@TL_TenantId", SqlDbType.UniqueIdentifier).Value = leave.TL_TenantId;
            cmd.Parameters.Add("@RequestingTeacherId", SqlDbType.UniqueIdentifier).Value = requestingTeacherId;
            cmd.Parameters.Add("@TL_LeaveType", SqlDbType.NVarChar, 30).Value = leave.TL_LeaveType;
            cmd.Parameters.Add("@TL_FromDate", SqlDbType.Date).Value = leave.TL_FromDate;
            cmd.Parameters.Add("@TL_ToDate", SqlDbType.Date).Value = leave.TL_ToDate;
            cmd.Parameters.Add("@TL_Reason", SqlDbType.NVarChar, 500).Value = (object)leave.TL_Reason ?? DBNull.Value;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            await reader.ReadAsync();

            var rowsAffected = reader.GetInt32(reader.GetOrdinal("RowsAffected"));
            return new LeaveApplyResult
            {
                TL_Id = rowsAffected > 0 ? leave.TL_Id : Guid.Empty, // Guid.Empty signals "not updated" to the service layer
                TotalDays = reader.GetInt32(reader.GetOrdinal("TotalDays"))
            };
        }
    }
}
