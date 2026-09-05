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
    public class TeacherAttendanceDAL : ITeacherAttendanceDAL
    {
        private readonly DBHelper _dbHelper;

        public TeacherAttendanceDAL(DBHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task<AttendanceUpsertResult> UpsertAsync(TeacherAttendance a)
        {
            a.TA_Id = a.TA_Id == Guid.Empty ? Guid.NewGuid() : a.TA_Id;

            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_TeacherAttendance_AddEdit", conn);

            cmd.Parameters.Add("@TA_Id", SqlDbType.UniqueIdentifier).Value = a.TA_Id;
            cmd.Parameters.Add("@TA_TenantId", SqlDbType.UniqueIdentifier).Value = a.TA_TenantId;
            cmd.Parameters.Add("@TA_TeacherId", SqlDbType.UniqueIdentifier).Value = a.TA_TeacherId;
            cmd.Parameters.Add("@TA_Date", SqlDbType.Date).Value = a.TA_Date;
            cmd.Parameters.Add("@TA_Status", SqlDbType.NVarChar, 20).Value = a.TA_Status;
            cmd.Parameters.Add("@TA_Remarks", SqlDbType.NVarChar, 500).Value = (object)a.TA_Remarks ?? DBNull.Value;
            cmd.Parameters.Add("@TA_MarkedBy", SqlDbType.UniqueIdentifier).Value = a.TA_MarkedBy;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            await reader.ReadAsync();

            return new AttendanceUpsertResult
            {
                FinalStatus = reader["FinalStatus"] as string,
                WasOverriddenToOnLeave = reader.GetBoolean(reader.GetOrdinal("WasOverriddenToOnLeave"))
            };
        }

        public async Task<List<TeacherAttendance>> GetByDateAsync(Guid tenantId, DateTime date)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_TeacherAttendance_GetByDate", conn);
            cmd.Parameters.Add("@TA_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;
            cmd.Parameters.Add("@TA_Date", SqlDbType.Date).Value = date.Date;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            var items = new List<TeacherAttendance>();
            while (await reader.ReadAsync()) items.Add(Map(reader));
            return items;
        }

        public async Task<List<TeacherAttendance>> GetByTeacherAndRangeAsync(Guid tenantId, Guid teacherId, DateTime fromDate, DateTime toDate)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_TeacherAttendance_GetByTeacherAndRange", conn);
            cmd.Parameters.Add("@TA_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;
            cmd.Parameters.Add("@TA_TeacherId", SqlDbType.UniqueIdentifier).Value = teacherId;
            cmd.Parameters.Add("@FromDate", SqlDbType.Date).Value = fromDate.Date;
            cmd.Parameters.Add("@ToDate", SqlDbType.Date).Value = toDate.Date;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            var items = new List<TeacherAttendance>();
            while (await reader.ReadAsync()) items.Add(Map(reader));
            return items;
        }

        private static TeacherAttendance Map(SqlDataReader r) => new()
        {
            TA_Id = r.GetGuid(r.GetOrdinal("TA_Id")),
            TA_TenantId = r.GetGuid(r.GetOrdinal("TA_TenantId")),
            TA_TeacherId = r.GetGuid(r.GetOrdinal("TA_TeacherId")),
            TA_Date = r.GetDateTime(r.GetOrdinal("TA_Date")),
            TA_Status = r["TA_Status"] as string,
            TA_Remarks = r["TA_Remarks"] as string,
            TA_MarkedBy = r.GetGuid(r.GetOrdinal("TA_MarkedBy")),
            TA_CreatedAt = r.GetDateTime(r.GetOrdinal("TA_CreatedAt")),
            TA_UpdatedAt = r.GetDateTime(r.GetOrdinal("TA_UpdatedAt"))
        };
    }
}
