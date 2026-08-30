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
    public class ExamDAL : IExamDAL
    {
        private readonly DBHelper _dbHelper;

        public ExamDAL(DBHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task<Exam> GetByIdAsync(Guid id, Guid tenantId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_Exams_EX", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "GetById";
            cmd.Parameters.Add("@EX_Id", SqlDbType.UniqueIdentifier).Value = id;
            cmd.Parameters.Add("@EX_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapExam(reader) : null;
        }

        public async Task<(List<Exam> Items, int TotalCount)> GetPagedAsync(
            Guid tenantId, string searchTerm, Guid? courseId, Guid? batchId,
            string status, int pageNumber, int pageSize)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_Exams_EX", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "GetPaged";
            cmd.Parameters.Add("@EX_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;
            cmd.Parameters.Add("@SearchTerm", SqlDbType.NVarChar, 255).Value = (object)searchTerm ?? DBNull.Value;
            cmd.Parameters.Add("@CourseId", SqlDbType.UniqueIdentifier).Value = (object)courseId ?? DBNull.Value;
            cmd.Parameters.Add("@BatchId", SqlDbType.UniqueIdentifier).Value = (object)batchId ?? DBNull.Value;
            cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = (object)status ?? DBNull.Value;
            cmd.Parameters.Add("@PageNumber", SqlDbType.Int).Value = pageNumber;
            cmd.Parameters.Add("@PageSize", SqlDbType.Int).Value = pageSize;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            int totalCount = 0;
            if (await reader.ReadAsync())
                totalCount = reader.GetInt32(reader.GetOrdinal("TotalCount"));

            var items = new List<Exam>();
            await reader.NextResultAsync();
            while (await reader.ReadAsync())
                items.Add(MapExam(reader));

            return (items, totalCount);
        }

        public async Task<bool> IsCodeTakenAsync(Guid tenantId, string code, Guid? excludeId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_Exams_EX", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "ExistsByCode";
            cmd.Parameters.Add("@EX_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;
            cmd.Parameters.Add("@EX_Code", SqlDbType.NVarChar, 50).Value = code;
            cmd.Parameters.Add("@ExcludeId", SqlDbType.UniqueIdentifier).Value = (object)excludeId ?? DBNull.Value;

            await conn.OpenAsync();
            return (int)await cmd.ExecuteScalarAsync() == 1;
        }

        public async Task<Guid> CreateAsync(Exam exam)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_Exams_EX", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "Insert";
            AddParameters(cmd, exam);

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return Guid.TryParse(result?.ToString(), out var id) ? id : exam.EX_Id;
        }

        public async Task<bool> UpdateAsync(Exam exam)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_Exams_EX", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "Update";
            AddParameters(cmd, exam);

            await conn.OpenAsync();
            return (int)await cmd.ExecuteScalarAsync() > 0;
        }

        public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_Exams_EX", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "Delete";
            cmd.Parameters.Add("@EX_Id", SqlDbType.UniqueIdentifier).Value = id;
            cmd.Parameters.Add("@EX_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;

            await conn.OpenAsync();
            return (int)await cmd.ExecuteScalarAsync() > 0;
        }

        private void AddParameters(SqlCommand cmd, Exam e)
        {
            cmd.Parameters.Add("@EX_Id", SqlDbType.UniqueIdentifier).Value = e.EX_Id;
            cmd.Parameters.Add("@EX_TenantId", SqlDbType.UniqueIdentifier).Value = e.EX_TenantId;
            cmd.Parameters.Add("@EX_AcademicYearId", SqlDbType.UniqueIdentifier).Value = e.EX_AcademicYearId;
            cmd.Parameters.Add("@EX_CourseId", SqlDbType.UniqueIdentifier).Value = e.EX_CourseId;
            cmd.Parameters.Add("@EX_BatchId", SqlDbType.UniqueIdentifier).Value = e.EX_BatchId;
            cmd.Parameters.Add("@EX_ExamTypeId", SqlDbType.UniqueIdentifier).Value = e.EX_ExamTypeId;
            cmd.Parameters.Add("@EX_Name", SqlDbType.NVarChar, 150).Value = e.EX_Name;
            cmd.Parameters.Add("@EX_Code", SqlDbType.NVarChar, 50).Value = e.EX_Code;
            cmd.Parameters.Add("@EX_StartDate", SqlDbType.Date).Value = e.EX_StartDate;
            cmd.Parameters.Add("@EX_EndDate", SqlDbType.Date).Value = e.EX_EndDate;
            cmd.Parameters.Add("@EX_Status", SqlDbType.NVarChar, 20).Value = e.EX_Status;
        }

        private static Exam MapExam(SqlDataReader r) => new()
        {
            EX_Id = r.GetGuid(r.GetOrdinal("EX_Id")),
            EX_TenantId = r.GetGuid(r.GetOrdinal("EX_TenantId")),
            EX_AcademicYearId = r.GetGuid(r.GetOrdinal("EX_AcademicYearId")),
            EX_CourseId = r.GetGuid(r.GetOrdinal("EX_CourseId")),
            EX_BatchId = r.GetGuid(r.GetOrdinal("EX_BatchId")),
            EX_ExamTypeId = r.GetGuid(r.GetOrdinal("EX_ExamTypeId")),
            EX_Name = r["EX_Name"] as string,
            EX_Code = r["EX_Code"] as string,
            EX_StartDate = r.GetDateTime(r.GetOrdinal("EX_StartDate")),
            EX_EndDate = r.GetDateTime(r.GetOrdinal("EX_EndDate")),
            EX_Status = r["EX_Status"] as string,
            EX_CreatedAt = r.GetDateTime(r.GetOrdinal("EX_CreatedAt")),
            EX_UpdatedAt = r.GetDateTime(r.GetOrdinal("EX_UpdatedAt")),
            AcademicYearName = r["AcademicYearName"] as string,
            CourseName = r["CourseName"] as string,
            BatchName = r["BatchName"] as string,
            ExamTypeName = r["ExamTypeName"] as string
        };
    }

    public class ExamSubjectDAL : IExamSubjectDAL
    {
        private readonly DBHelper _dbHelper;

        public ExamSubjectDAL(DBHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task<List<ExamSubject>> GetByExamIdAsync(Guid examId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_ExamSubjects_ES", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "GetByExam";
            cmd.Parameters.Add("@ES_ExamId", SqlDbType.UniqueIdentifier).Value = examId;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            var items = new List<ExamSubject>();
            while (await reader.ReadAsync())
            {
                items.Add(new ExamSubject
                {
                    ES_Id = reader.GetGuid(reader.GetOrdinal("ES_Id")),
                    ES_ExamId = reader.GetGuid(reader.GetOrdinal("ES_ExamId")),
                    ES_SubjectId = reader.GetGuid(reader.GetOrdinal("ES_SubjectId")),
                    ES_MaxMarks = reader.GetDecimal(reader.GetOrdinal("ES_MaxMarks")),
                    ES_PassMarks = reader.GetDecimal(reader.GetOrdinal("ES_PassMarks")),
                    ES_Weightage = reader["ES_Weightage"] as decimal?,
                    SubjectName = reader["SubjectName"] as string
                });
            }
            return items;
        }

        public async Task<bool> SaveSubjectsAsync(Guid examId, List<ExamSubject> subjects)
        {
            using var conn = _dbHelper.GetConnection();
            await conn.OpenAsync();

            using var deleteCmd = _dbHelper.CreateCommand("USP_ExamSubjects_ES", conn);
            deleteCmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "DeleteByExam";
            deleteCmd.Parameters.Add("@ES_ExamId", SqlDbType.UniqueIdentifier).Value = examId;
            await deleteCmd.ExecuteNonQueryAsync();

            foreach (var subject in subjects)
            {
                using var cmd = _dbHelper.CreateCommand("USP_ExamSubjects_ES", conn);
                cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "Insert";
                cmd.Parameters.Add("@ES_Id", SqlDbType.UniqueIdentifier).Value = subject.ES_Id;
                cmd.Parameters.Add("@ES_ExamId", SqlDbType.UniqueIdentifier).Value = examId;
                cmd.Parameters.Add("@ES_SubjectId", SqlDbType.UniqueIdentifier).Value = subject.ES_SubjectId;
                cmd.Parameters.Add("@ES_MaxMarks", SqlDbType.Decimal).Value = subject.ES_MaxMarks;
                cmd.Parameters.Add("@ES_PassMarks", SqlDbType.Decimal).Value = subject.ES_PassMarks;
                cmd.Parameters.Add("@ES_Weightage", SqlDbType.Decimal).Value = (object)subject.ES_Weightage ?? DBNull.Value;
                await cmd.ExecuteNonQueryAsync();
            }

            return true;
        }
    }

    public class MarkDAL : IMarkDAL
    {
        private readonly DBHelper _dbHelper;

        public MarkDAL(DBHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task<List<Mark>> GetByExamIdAsync(Guid examId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_Marks_M", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "GetByExam";
            cmd.Parameters.Add("@ExamId", SqlDbType.UniqueIdentifier).Value = examId;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            var items = new List<Mark>();
            while (await reader.ReadAsync())
            {
                items.Add(new Mark
                {
                    M_Id = reader.GetGuid(reader.GetOrdinal("M_Id")),
                    M_ExamSubjectId = reader.GetGuid(reader.GetOrdinal("M_ExamSubjectId")),
                    M_StudentId = reader.GetGuid(reader.GetOrdinal("M_StudentId")),
                    M_MarksObtained = reader.GetDecimal(reader.GetOrdinal("M_MarksObtained")),
                    M_Percentage = reader["M_Percentage"] as decimal?,
                    M_Remarks = reader["M_Remarks"] as string,
                    M_CreatedAt = reader.GetDateTime(reader.GetOrdinal("M_CreatedAt")),
                    M_UpdatedAt = reader.GetDateTime(reader.GetOrdinal("M_UpdatedAt")),
                    StudentName = reader["StudentName"] as string,
                    StudentCode = reader["StudentCode"] as string,
                    Grade = reader["Grade"] as string
                });
            }
            return items;
        }

        public async Task<bool> SaveMarksAsync(Guid examId, List<Mark> marks)
        {
            using var conn = _dbHelper.GetConnection();
            await conn.OpenAsync();

            foreach (var mark in marks)
            {
                using var cmd = _dbHelper.CreateCommand("USP_Marks_M", conn);
                cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "Upsert";
                cmd.Parameters.Add("@M_Id", SqlDbType.UniqueIdentifier).Value = mark.M_Id;
                cmd.Parameters.Add("@M_ExamSubjectId", SqlDbType.UniqueIdentifier).Value = mark.M_ExamSubjectId;
                cmd.Parameters.Add("@M_StudentId", SqlDbType.UniqueIdentifier).Value = mark.M_StudentId;
                cmd.Parameters.Add("@M_MarksObtained", SqlDbType.Decimal).Value = mark.M_MarksObtained;
                cmd.Parameters.Add("@M_Remarks", SqlDbType.NVarChar, -1).Value = (object)mark.M_Remarks ?? DBNull.Value;
                await cmd.ExecuteNonQueryAsync();
            }

            return true;
        }
    }

    public class ResultDAL : IResultDAL
    {
        private readonly DBHelper _dbHelper;

        public ResultDAL(DBHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task<List<Result>> GetByExamIdAsync(Guid examId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("USP_Results_R", conn);
            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "GetByExam";
            cmd.Parameters.Add("@R_ExamId", SqlDbType.UniqueIdentifier).Value = examId;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            var items = new List<Result>();
            while (await reader.ReadAsync())
            {
                items.Add(new Result
                {
                    R_Id = reader.GetGuid(reader.GetOrdinal("R_Id")),
                    R_ExamId = reader.GetGuid(reader.GetOrdinal("R_ExamId")),
                    R_StudentId = reader.GetGuid(reader.GetOrdinal("R_StudentId")),
                    R_TotalMarks = reader.GetDecimal(reader.GetOrdinal("R_TotalMarks")),
                    R_MarksObtained = reader.GetDecimal(reader.GetOrdinal("R_MarksObtained")),
                    R_Percentage = reader.GetDecimal(reader.GetOrdinal("R_Percentage")),
                    R_Grade = reader["R_Grade"] as string,
                    R_ResultStatus = reader["R_ResultStatus"] as string,
                    R_Remarks = reader["R_Remarks"] as string,
                    R_PublishedAt = reader["R_PublishedAt"] as DateTime?,
                    R_CreatedAt = reader.GetDateTime(reader.GetOrdinal("R_CreatedAt")),
                    R_UpdatedAt = reader.GetDateTime(reader.GetOrdinal("R_UpdatedAt")),
                    StudentName = reader["StudentName"] as string,
                    StudentCode = reader["StudentCode"] as string
                });
            }
            return items;
        }

        public async Task<bool> PublishResultsAsync(Guid examId, List<Result> results)
        {
            using var conn = _dbHelper.GetConnection();
            await conn.OpenAsync();

            using var deleteCmd = _dbHelper.CreateCommand("USP_Results_R", conn);
            deleteCmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "DeleteByExam";
            deleteCmd.Parameters.Add("@R_ExamId", SqlDbType.UniqueIdentifier).Value = examId;
            await deleteCmd.ExecuteNonQueryAsync();

            foreach (var result in results)
            {
                using var cmd = _dbHelper.CreateCommand("USP_Results_R", conn);
                cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "Insert";
                cmd.Parameters.Add("@R_Id", SqlDbType.UniqueIdentifier).Value = result.R_Id;
                cmd.Parameters.Add("@R_ExamId", SqlDbType.UniqueIdentifier).Value = examId;
                cmd.Parameters.Add("@R_StudentId", SqlDbType.UniqueIdentifier).Value = result.R_StudentId;
                cmd.Parameters.Add("@R_TotalMarks", SqlDbType.Decimal).Value = result.R_TotalMarks;
                cmd.Parameters.Add("@R_MarksObtained", SqlDbType.Decimal).Value = result.R_MarksObtained;
                cmd.Parameters.Add("@R_Percentage", SqlDbType.Decimal).Value = result.R_Percentage;
                cmd.Parameters.Add("@R_Grade", SqlDbType.NVarChar, 20).Value = (object)result.R_Grade ?? DBNull.Value;
                cmd.Parameters.Add("@R_ResultStatus", SqlDbType.NVarChar, 30).Value = result.R_ResultStatus;
                cmd.Parameters.Add("@R_Remarks", SqlDbType.NVarChar, -1).Value = (object)result.R_Remarks ?? DBNull.Value;
                await cmd.ExecuteNonQueryAsync();
            }

            return true;
        }
    }
}
