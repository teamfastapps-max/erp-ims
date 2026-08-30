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
    public class CourseSubjectDAL : ICourseSubjectDAL
    {
        private readonly DBHelper _dbHelper;

        public CourseSubjectDAL(DBHelper dbHelper) { _dbHelper = dbHelper; }

        public async Task<List<CourseSubject>> GetAllAsync(Guid tenantId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_CourseSubjects_CS", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "GetAll";
            cmd.Parameters.Add("@TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            var list = new List<CourseSubject>();
            while (await reader.ReadAsync()) list.Add(Map(reader));
            return list;
        }

        public async Task<List<CourseSubject>> GetByCourseIdAsync(Guid courseId, Guid tenantId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_CourseSubjects_CS", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "GetByCourseId";
            cmd.Parameters.Add("@CS_CourseId", SqlDbType.UniqueIdentifier).Value = courseId;
            cmd.Parameters.Add("@TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            var list = new List<CourseSubject>();
            while (await reader.ReadAsync()) list.Add(Map(reader));
            return list;
        }

        public async Task<CourseSubject> GetByIdAsync(Guid courseId, Guid subjectId, Guid tenantId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_CourseSubjects_CS", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "GetById";
            cmd.Parameters.Add("@CS_CourseId", SqlDbType.UniqueIdentifier).Value = courseId;
            cmd.Parameters.Add("@CS_SubjectId", SqlDbType.UniqueIdentifier).Value = subjectId;
            cmd.Parameters.Add("@TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? Map(reader) : null;
        }

        public async Task<bool> ExistsAsync(Guid courseId, Guid subjectId, Guid tenantId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_CourseSubjects_CS", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "Exists";
            cmd.Parameters.Add("@CS_CourseId", SqlDbType.UniqueIdentifier).Value = courseId;
            cmd.Parameters.Add("@CS_SubjectId", SqlDbType.UniqueIdentifier).Value = subjectId;
            cmd.Parameters.Add("@TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;

            await conn.OpenAsync();
            return (int)await cmd.ExecuteScalarAsync() == 1;
        }

        public async Task<bool> CreateAsync(CourseSubject cs)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_CourseSubjects_CS", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "Insert";
            AddParams(cmd, cs);

            await conn.OpenAsync();
            return (int)await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> UpdateAsync(CourseSubject cs, Guid tenantId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_CourseSubjects_CS", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "Update";
            cmd.Parameters.Add("@TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;
            AddParams(cmd, cs);

            await conn.OpenAsync();
            return (int)await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> DeleteAsync(Guid courseId, Guid subjectId, Guid tenantId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_CourseSubjects_CS", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "Delete";
            cmd.Parameters.Add("@CS_CourseId", SqlDbType.UniqueIdentifier).Value = courseId;
            cmd.Parameters.Add("@CS_SubjectId", SqlDbType.UniqueIdentifier).Value = subjectId;
            cmd.Parameters.Add("@TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;

            await conn.OpenAsync();
            return (int)await cmd.ExecuteNonQueryAsync() > 0;
        }

        private void AddParams(SqlCommand cmd, CourseSubject cs)
        {
            cmd.Parameters.Add("@CS_CourseId", SqlDbType.UniqueIdentifier).Value = cs.CS_CourseId;
            cmd.Parameters.Add("@CS_SubjectId", SqlDbType.UniqueIdentifier).Value = cs.CS_SubjectId;
            cmd.Parameters.Add("@CS_SequenceNo", SqlDbType.Int).Value = cs.CS_SequenceNo;
            cmd.Parameters.Add("@CS_IsMandatory", SqlDbType.Bit).Value = cs.CS_IsMandatory;
            cmd.Parameters.Add("@CS_MaxMarks", SqlDbType.Decimal).Value = (object)cs.CS_MaxMarks ?? DBNull.Value;
            cmd.Parameters.Add("@CS_PassMarks", SqlDbType.Decimal).Value = (object)cs.CS_PassMarks ?? DBNull.Value;
        }

        private static CourseSubject Map(SqlDataReader r) => new()
        {
            CS_CourseId = r.GetGuid(r.GetOrdinal("CS_CourseId")),
            CS_SubjectId = r.GetGuid(r.GetOrdinal("CS_SubjectId")),
            CS_SequenceNo = r.GetInt32(r.GetOrdinal("CS_SequenceNo")),
            CS_IsMandatory = r.GetBoolean(r.GetOrdinal("CS_IsMandatory")),
            CS_MaxMarks = r["CS_MaxMarks"] as decimal?,
            CS_PassMarks = r["CS_PassMarks"] as decimal?,
            CourseName = r["CourseName"] as string,
            SubjectName = r["SubjectName"] as string
        };
    }
}
