using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using IMS.DAL.Common;
using IMS.DAL.Interfaces;
using IMS.Models.Portal;

namespace IMS.DAL
{
    public class StudentLeaveDAL : IStudentLeaveDAL
    {
        private readonly DBHelper _dbHelper;

        public StudentLeaveDAL(DBHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task<(List<StudentLeaveDto> Items, int TotalCount)> GetPagedAsync(Guid tenantId, string? status, string? search, int pageNumber, int pageSize)
        {
            var list = new List<StudentLeaveDto>();
            int totalCount = 0;

            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_StudentLeaves_GetPaged", conn);

            cmd.Parameters.Add("@TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;
            cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = (object?)status ?? DBNull.Value;
            cmd.Parameters.Add("@Search", SqlDbType.NVarChar, 100).Value = (object?)search ?? DBNull.Value;
            cmd.Parameters.Add("@PageNumber", SqlDbType.Int).Value = pageNumber;
            cmd.Parameters.Add("@PageSize", SqlDbType.Int).Value = pageSize;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new StudentLeaveDto
                {
                    LeaveId = (Guid)reader["LeaveId"],
                    StudentId = (Guid)reader["StudentId"],
                    StudentName = reader["StudentName"]?.ToString() ?? string.Empty,
                    StudentCode = reader["StudentCode"]?.ToString(),
                    FromDate = Convert.ToDateTime(reader["FromDate"]),
                    ToDate = Convert.ToDateTime(reader["ToDate"]),
                    TotalDays = Convert.ToInt32(reader["TotalDays"]),
                    LeaveType = reader["LeaveType"]?.ToString() ?? "Leave",
                    Reason = reader["Reason"]?.ToString() ?? string.Empty,
                    Status = reader["Status"]?.ToString() ?? "Pending",
                    AppliedBy = reader["AppliedBy"]?.ToString() ?? "Student",
                    AppliedAt = Convert.ToDateTime(reader["AppliedAt"]),
                    ApprovedAt = reader["ApprovedAt"] != DBNull.Value ? (DateTime?)reader["ApprovedAt"] : null,
                    RejectionReason = reader["RejectionReason"]?.ToString()
                });
                totalCount = Convert.ToInt32(reader["TotalCount"]);
            }

            return (list, totalCount);
        }

        public async Task<bool> ReviewAsync(Guid leaveId, Guid tenantId, Guid approvedBy, string status, string? rejectionReason)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_StudentLeaves_Review", conn);
            cmd.Parameters.Add("@LeaveId", SqlDbType.UniqueIdentifier).Value = leaveId;
            cmd.Parameters.Add("@TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;
            cmd.Parameters.Add("@ApprovedBy", SqlDbType.UniqueIdentifier).Value = approvedBy;
            cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = status;
            cmd.Parameters.Add("@RejectionReason", SqlDbType.NVarChar, 500).Value = (object?)rejectionReason ?? DBNull.Value;

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return result != null && Convert.ToInt32(result) > 0;
        }

        public async Task<bool> DeleteAsync(Guid leaveId, Guid tenantId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_StudentLeaves_Delete", conn);
            cmd.Parameters.Add("@LeaveId", SqlDbType.UniqueIdentifier).Value = leaveId;
            cmd.Parameters.Add("@TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return result != null && Convert.ToInt32(result) > 0;
        }
    }
}
