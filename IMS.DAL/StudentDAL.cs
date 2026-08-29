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
    public class StudentDAL : IStudentDAL
    {
        private readonly DBHelper _dbHelper;

        public StudentDAL(DBHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }
        public async Task<Student> GetByIdAsync(Guid id, Guid tenantId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_Students_GetById", conn);
            cmd.Parameters.Add("@S_Id", SqlDbType.UniqueIdentifier).Value = id;
            cmd.Parameters.Add("@S_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapStudent(reader) : null;
        }

        public async Task<(List<Student> Items, int TotalCount)> GetPagedAsync(
            Guid tenantId, string searchTerm, string status, Guid? branchId, Guid? classId, int pageNumber, int pageSize)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_Students_GetPaged", conn);

            cmd.Parameters.Add("@S_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;
            cmd.Parameters.Add("@SearchTerm", SqlDbType.NVarChar, 255).Value = (object)searchTerm ?? DBNull.Value;
            cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = (object)status ?? DBNull.Value;
            cmd.Parameters.Add("@BranchId", SqlDbType.UniqueIdentifier).Value = (object)branchId ?? DBNull.Value;
            cmd.Parameters.Add("@PageNumber", SqlDbType.Int).Value = pageNumber;
            cmd.Parameters.Add("@PageSize", SqlDbType.Int).Value = pageSize;
            // NOTE: SP_Students_GetPaged does not yet filter by class - if you
            // need server-side class filtering, add @ClassId to the SP (see
            // Database/SP_Students.sql) and pass classId through here.

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            int totalCount = 0;
            if (await reader.ReadAsync())
                totalCount = reader.GetInt32(reader.GetOrdinal("TotalCount"));

            var items = new List<Student>();
            await reader.NextResultAsync();
            while (await reader.ReadAsync())
                items.Add(MapStudent(reader));

            return (items, totalCount);
        }

        public async Task<bool> IsAdmissionNumberTakenAsync(Guid tenantId, string admissionNumber, Guid? excludeId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_Students_CheckAdmissionNumberExists", conn);
            cmd.Parameters.Add("@S_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;
            cmd.Parameters.Add("@AdmissionNumber", SqlDbType.NVarChar, 50).Value = admissionNumber;
            cmd.Parameters.Add("@ExcludeId", SqlDbType.UniqueIdentifier).Value = (object)excludeId ?? DBNull.Value;

            await conn.OpenAsync();
            return (int)await cmd.ExecuteScalarAsync() == 1;
        }

        public async Task<bool> IsStudentCodeTakenAsync(Guid tenantId, string studentCode, Guid? excludeId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_Students_CheckStudentCodeExists", conn);
            cmd.Parameters.Add("@S_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;
            cmd.Parameters.Add("@StudentCode", SqlDbType.NVarChar, 50).Value = studentCode;
            cmd.Parameters.Add("@ExcludeId", SqlDbType.UniqueIdentifier).Value = (object)excludeId ?? DBNull.Value;

            await conn.OpenAsync();
            return (int)await cmd.ExecuteScalarAsync() == 1;
        }

        public async Task<List<StudentGuardianRecord>> GetGuardiansByStudentIdAsync(Guid studentId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_StudentGuardians_GetByStudentId", conn);
            cmd.Parameters.Add("@SG_StudentId", SqlDbType.UniqueIdentifier).Value = studentId;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            var list = new List<StudentGuardianRecord>();
            while (await reader.ReadAsync())
            {
                list.Add(new StudentGuardianRecord
                {
                    SG_Id = reader.GetGuid(reader.GetOrdinal("SG_Id")),
                    SG_StudentId = reader.GetGuid(reader.GetOrdinal("SG_StudentId")),
                    SG_GuardianId = reader.GetGuid(reader.GetOrdinal("SG_GuardianId")),
                    SG_Relation = reader["SG_Relation"] as string,
                    SG_IsPrimary = reader.GetBoolean(reader.GetOrdinal("SG_IsPrimary")),
                    G_FirstName = reader["G_FirstName"] as string,
                    G_LastName = reader["G_LastName"] as string,
                    G_Phone = reader["G_Phone"] as string,
                    G_Email = reader["G_Email"] as string,
                    G_Occupation = reader["G_Occupation"] as string
                });
            }
            return list;
        }

        // ---------------------------------------------------------------
        // Composite writes (student + guardians in one transaction)
        // ---------------------------------------------------------------

        public async Task<Guid> CreateStudentWithGuardiansAsync(Student student, List<GuardianLinkInput> guardians)
        {
            student.S_Id = student.S_Id == Guid.Empty ? Guid.NewGuid() : student.S_Id;

            using var conn = _dbHelper.GetConnection();
            await conn.OpenAsync();
            using var tran = conn.BeginTransaction();

            try
            {
                await ExecStudentCreateAsync(conn, tran, student);
                await SyncGuardiansAsync(conn, tran, student.S_Id, student.S_TenantId, guardians);

                tran.Commit();
                return student.S_Id;
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        public async Task<bool> UpdateStudentWithGuardiansAsync(Student student, List<GuardianLinkInput> guardians)
        {
            using var conn = _dbHelper.GetConnection();
            await conn.OpenAsync();
            using var tran = conn.BeginTransaction();

            try
            {
                var rowsAffected = await ExecStudentUpdateAsync(conn, tran, student);
                if (rowsAffected == 0)
                {
                    tran.Rollback();
                    return false;
                }

                // Simplest correct approach for a small per-student list:
                // wipe existing links, then re-insert the current set.
                await ExecRemoveGuardianLinksAsync(conn, tran, student.S_Id);
                await SyncGuardiansAsync(conn, tran, student.S_Id, student.S_TenantId, guardians);

                tran.Commit();
                return true;
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        public async Task<bool> SoftDeleteAsync(Guid id, Guid tenantId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_Students_SoftDelete", conn);
            cmd.Parameters.Add("@S_Id", SqlDbType.UniqueIdentifier).Value = id;
            cmd.Parameters.Add("@S_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;

            await conn.OpenAsync();
            var rowsAffected = (int)await cmd.ExecuteScalarAsync();
            return rowsAffected > 0;
        }

        // ---------------------------------------------------------------
        // Transaction-scoped helpers (used only by the composite methods above)
        // ---------------------------------------------------------------

        private async Task ExecStudentCreateAsync(SqlConnection conn, SqlTransaction tran, Student s)
        {
            using var cmd = _dbHelper.CreateCommand("SP_Students_Create", conn);
            cmd.Transaction = tran;
            AddStudentParameters(cmd, s);

            // SP_Students_Create returns the (possibly auto-generated) admission
            // number as a single-row result set - capture it back onto the entity.
            var generatedNumber = (string)await cmd.ExecuteScalarAsync();
            if (!string.IsNullOrWhiteSpace(generatedNumber))
                s.S_AdmissionNumber = generatedNumber;
        }

        private async Task<int> ExecStudentUpdateAsync(SqlConnection conn, SqlTransaction tran, Student s)
        {
            using var cmd = _dbHelper.CreateCommand("SP_Students_Update", conn);
            cmd.Transaction = tran;
            AddStudentParameters(cmd, s);
            return (int)await cmd.ExecuteScalarAsync();
        }

        private async Task ExecRemoveGuardianLinksAsync(SqlConnection conn, SqlTransaction tran, Guid studentId)
        {
            using var cmd = _dbHelper.CreateCommand("SP_StudentGuardians_RemoveByStudent", conn);
            cmd.Transaction = tran;
            cmd.Parameters.Add("@SG_StudentId", SqlDbType.UniqueIdentifier).Value = studentId;
            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// For each guardian row: use the existing GuardianId if one was
        /// picked from search, otherwise create a new Guardians_G row - then
        /// link it to the student via Students_Guardians.
        /// </summary>
        private async Task SyncGuardiansAsync(
            SqlConnection conn, SqlTransaction tran, Guid studentId, Guid tenantId, List<GuardianLinkInput> guardians)
        {
            if (guardians == null) return;

            foreach (var g in guardians)
            {
                var guardianId = g.ExistingGuardianId;

                if (!guardianId.HasValue)
                {
                    guardianId = Guid.NewGuid();
                    using var createCmd = _dbHelper.CreateCommand("SP_Guardians_Create", conn);
                    createCmd.Transaction = tran;
                    createCmd.Parameters.Add("@G_Id", SqlDbType.UniqueIdentifier).Value = guardianId.Value;
                    createCmd.Parameters.Add("@G_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;
                    createCmd.Parameters.Add("@G_FirstName", SqlDbType.NVarChar, 100).Value = g.FirstName;
                    createCmd.Parameters.Add("@G_LastName", SqlDbType.NVarChar, 100).Value = (object)g.LastName ?? DBNull.Value;
                    createCmd.Parameters.Add("@G_Phone", SqlDbType.NVarChar, 30).Value = (object)g.Phone ?? DBNull.Value;
                    createCmd.Parameters.Add("@G_Email", SqlDbType.NVarChar, 255).Value = (object)g.Email ?? DBNull.Value;
                    createCmd.Parameters.Add("@G_Occupation", SqlDbType.NVarChar, 100).Value = (object)g.Occupation ?? DBNull.Value;
                    await createCmd.ExecuteNonQueryAsync();
                }

                using var linkCmd = _dbHelper.CreateCommand("SP_StudentGuardians_Add", conn);
                linkCmd.Transaction = tran;
                linkCmd.Parameters.Add("@SG_Id", SqlDbType.UniqueIdentifier).Value = Guid.NewGuid();
                linkCmd.Parameters.Add("@SG_StudentId", SqlDbType.UniqueIdentifier).Value = studentId;
                linkCmd.Parameters.Add("@SG_GuardianId", SqlDbType.UniqueIdentifier).Value = guardianId.Value;
                linkCmd.Parameters.Add("@SG_Relation", SqlDbType.NVarChar, 50).Value = g.Relation;
                linkCmd.Parameters.Add("@SG_IsPrimary", SqlDbType.Bit).Value = g.IsPrimary;
                await linkCmd.ExecuteNonQueryAsync();
            }
        }

        private static void AddStudentParameters(SqlCommand cmd, Student s)
        {
            cmd.Parameters.Add("@S_Id", SqlDbType.UniqueIdentifier).Value = s.S_Id;
            cmd.Parameters.Add("@S_TenantId", SqlDbType.UniqueIdentifier).Value = s.S_TenantId;
            cmd.Parameters.Add("@S_BranchId", SqlDbType.UniqueIdentifier).Value = s.S_BranchId;
            cmd.Parameters.Add("@S_UserId", SqlDbType.UniqueIdentifier).Value = (object)s.S_UserId ?? DBNull.Value;
            cmd.Parameters.Add("@S_StudentCode", SqlDbType.NVarChar, 50).Value = s.S_StudentCode;
            // Blank/null => SP_Students_Create auto-generates it (see 05_AdmissionNumber_AutoGenerate.sql)
            cmd.Parameters.Add("@S_AdmissionNumber", SqlDbType.NVarChar, 50).Value =
                string.IsNullOrWhiteSpace(s.S_AdmissionNumber) ? DBNull.Value : s.S_AdmissionNumber;
            cmd.Parameters.Add("@S_FirstName", SqlDbType.NVarChar, 100).Value = s.S_FirstName;
            cmd.Parameters.Add("@S_MiddleName", SqlDbType.NVarChar, 100).Value = (object)s.S_MiddleName ?? DBNull.Value;
            cmd.Parameters.Add("@S_LastName", SqlDbType.NVarChar, 100).Value = s.S_LastName;
            cmd.Parameters.Add("@S_DateOfBirth", SqlDbType.Date).Value = (object)s.S_DateOfBirth ?? DBNull.Value;
            cmd.Parameters.Add("@S_Gender", SqlDbType.NVarChar, 20).Value = (object)s.S_Gender ?? DBNull.Value;
            cmd.Parameters.Add("@S_Email", SqlDbType.NVarChar, 255).Value = (object)s.S_Email ?? DBNull.Value;
            cmd.Parameters.Add("@S_Phone", SqlDbType.NVarChar, 30).Value = (object)s.S_Phone ?? DBNull.Value;
            cmd.Parameters.Add("@S_AdmissionDate", SqlDbType.Date).Value = (object)s.S_AdmissionDate ?? DBNull.Value;
            cmd.Parameters.Add("@S_Status", SqlDbType.NVarChar, 20).Value = s.S_Status;
            cmd.Parameters.Add("@S_ClassId", SqlDbType.UniqueIdentifier).Value = (object)s.S_ClassId ?? DBNull.Value;
            cmd.Parameters.Add("@S_SectionId", SqlDbType.UniqueIdentifier).Value = (object)s.S_SectionId ?? DBNull.Value;
            cmd.Parameters.Add("@S_BloodGroup", SqlDbType.NVarChar, 10).Value = (object)s.S_BloodGroup ?? DBNull.Value;
            cmd.Parameters.Add("@S_AddressLine1", SqlDbType.NVarChar, 255).Value = (object)s.S_AddressLine1 ?? DBNull.Value;
            cmd.Parameters.Add("@S_AddressLine2", SqlDbType.NVarChar, 255).Value = (object)s.S_AddressLine2 ?? DBNull.Value;
            cmd.Parameters.Add("@S_City", SqlDbType.NVarChar, 100).Value = (object)s.S_City ?? DBNull.Value;
            cmd.Parameters.Add("@S_State", SqlDbType.NVarChar, 100).Value = (object)s.S_State ?? DBNull.Value;
            cmd.Parameters.Add("@S_PostalCode", SqlDbType.NVarChar, 20).Value = (object)s.S_PostalCode ?? DBNull.Value;
            cmd.Parameters.Add("@S_Country", SqlDbType.NVarChar, 100).Value = (object)s.S_Country ?? DBNull.Value;
        }

        private static Student MapStudent(SqlDataReader r) => new()
        {
            S_Id = r.GetGuid(r.GetOrdinal("S_Id")),
            S_TenantId = r.GetGuid(r.GetOrdinal("S_TenantId")),
            S_BranchId = r.GetGuid(r.GetOrdinal("S_BranchId")),
            S_UserId = r["S_UserId"] as Guid?,
            S_StudentCode = r["S_StudentCode"] as string,
            S_AdmissionNumber = r["S_AdmissionNumber"] as string,
            S_FirstName = r["S_FirstName"] as string,
            S_MiddleName = r["S_MiddleName"] as string,
            S_LastName = r["S_LastName"] as string,
            S_DateOfBirth = r["S_DateOfBirth"] as DateTime?,
            S_Gender = r["S_Gender"] as string,
            S_Email = r["S_Email"] as string,
            S_Phone = r["S_Phone"] as string,
            S_AdmissionDate = r["S_AdmissionDate"] as DateTime?,
            S_Status = r["S_Status"] as string,
            S_ClassId = r["S_ClassId"] as Guid?,
            S_SectionId = r["S_SectionId"] as Guid?,
            S_BloodGroup = r["S_BloodGroup"] as string,
            S_AddressLine1 = r["S_AddressLine1"] as string,
            S_AddressLine2 = r["S_AddressLine2"] as string,
            S_City = r["S_City"] as string,
            S_State = r["S_State"] as string,
            S_PostalCode = r["S_PostalCode"] as string,
            S_Country = r["S_Country"] as string,
            S_CreatedAt = r.GetDateTime(r.GetOrdinal("S_CreatedAt")),
            S_UpdatedAt = r.GetDateTime(r.GetOrdinal("S_UpdatedAt")),
            S_DeletedAt = r["S_DeletedAt"] as DateTime?
        };
    }
}
