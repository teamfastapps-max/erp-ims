using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using IMS.DAL.Common;
using IMS.DAL.Interfaces;
using IMS.Models.Portal;

namespace IMS.DAL
{
    public class TransferCertificateDAL : ITransferCertificateDAL
    {
        private readonly DBHelper _dbHelper;

        public TransferCertificateDAL(DBHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task<(List<TransferCertificateDto> Items, int TotalCount)> GetPagedAsync(Guid tenantId, string? status, string? search, int pageNumber, int pageSize)
        {
            var list = new List<TransferCertificateDto>();
            int totalCount = 0;

            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_TransferCertificates_GetPaged", conn);

            cmd.Parameters.Add("@TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;
            cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 30).Value = (object?)status ?? DBNull.Value;
            cmd.Parameters.Add("@Search", SqlDbType.NVarChar, 100).Value = (object?)search ?? DBNull.Value;
            cmd.Parameters.Add("@PageNumber", SqlDbType.Int).Value = pageNumber;
            cmd.Parameters.Add("@PageSize", SqlDbType.Int).Value = pageSize;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new TransferCertificateDto
                {
                    TCId = (Guid)reader["TCId"],
                    StudentId = (Guid)reader["StudentId"],
                    StudentName = reader["StudentName"]?.ToString() ?? string.Empty,
                    StudentCode = reader["StudentCode"]?.ToString(),
                    AdmissionNumber = reader["AdmissionNumber"]?.ToString(),
                    CourseName = reader["CourseName"]?.ToString(),
                    BatchName = reader["BatchName"]?.ToString(),
                    ApplicationNumber = reader["ApplicationNumber"]?.ToString() ?? string.Empty,
                    ApplicationDate = Convert.ToDateTime(reader["ApplicationDate"]),
                    ExpectedLeavingDate = Convert.ToDateTime(reader["ExpectedLeavingDate"]),
                    Reason = reader["Reason"]?.ToString() ?? string.Empty,
                    LibraryClearance = Convert.ToBoolean(reader["LibraryClearance"]),
                    FeeClearance = Convert.ToBoolean(reader["FeeClearance"]),
                    LabClearance = Convert.ToBoolean(reader["LabClearance"]),
                    Status = reader["Status"]?.ToString() ?? "Submitted",
                    CertificateNumber = reader["CertificateNumber"]?.ToString(),
                    IssuedDate = reader["IssuedDate"] != DBNull.Value ? (DateTime?)reader["IssuedDate"] : null,
                    Remarks = reader["Remarks"]?.ToString(),
                    CreatedAt = reader["CreatedAt"] != DBNull.Value ? (DateTime?)reader["CreatedAt"] : null
                });
                totalCount = Convert.ToInt32(reader["TotalCount"]);
            }

            return (list, totalCount);
        }

        public async Task<bool> ReviewAsync(Guid tcId, Guid tenantId, bool libraryClearance, bool feeClearance, bool labClearance, string status, string? remarks)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_TransferCertificates_Review", conn);
            cmd.Parameters.Add("@TCId", SqlDbType.UniqueIdentifier).Value = tcId;
            cmd.Parameters.Add("@TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;
            cmd.Parameters.Add("@LibraryClearance", SqlDbType.Bit).Value = libraryClearance;
            cmd.Parameters.Add("@FeeClearance", SqlDbType.Bit).Value = feeClearance;
            cmd.Parameters.Add("@LabClearance", SqlDbType.Bit).Value = labClearance;
            cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 30).Value = status;
            cmd.Parameters.Add("@Remarks", SqlDbType.NVarChar, -1).Value = (object?)remarks ?? DBNull.Value;

            await conn.OpenAsync();
            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(Guid tcId, Guid tenantId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_TransferCertificates_Delete", conn);
            cmd.Parameters.Add("@TCId", SqlDbType.UniqueIdentifier).Value = tcId;
            cmd.Parameters.Add("@TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;

            await conn.OpenAsync();
            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        public async Task<TransferCertificatePrintViewModel?> GetByIdAsync(Guid tcId, Guid tenantId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_TransferCertificates_GetById", conn);
            cmd.Parameters.Add("@TCId", SqlDbType.UniqueIdentifier).Value = tcId;
            cmd.Parameters.Add("@TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new TransferCertificatePrintViewModel
                {
                    TCId = (Guid)reader["TCId"],
                    StudentId = (Guid)reader["StudentId"],
                    StudentName = reader["StudentName"]?.ToString() ?? string.Empty,
                    StudentCode = reader["StudentCode"]?.ToString(),
                    AdmissionNumber = reader["AdmissionNumber"]?.ToString(),
                    Gender = reader["Gender"]?.ToString(),
                    DateOfBirth = reader["DateOfBirth"] != DBNull.Value ? (DateTime?)reader["DateOfBirth"] : null,
                    BloodGroup = reader["BloodGroup"]?.ToString(),
                    Address = reader["Address"]?.ToString(),
                    Phone = reader["Phone"]?.ToString(),
                    Email = reader["Email"]?.ToString(),
                    GuardianName = reader["GuardianName"]?.ToString(),
                    CourseName = reader["CourseName"]?.ToString(),
                    BatchName = reader["BatchName"]?.ToString(),
                    AcademicYearName = reader["AcademicYearName"]?.ToString(),
                    BranchName = reader["BranchName"]?.ToString(),
                    OrganizationName = reader["OrganizationName"]?.ToString(),
                    OrganizationAddress = reader["OrganizationAddress"]?.ToString(),
                    OrganizationPhone = reader["OrganizationPhone"]?.ToString(),
                    OrganizationEmail = reader["OrganizationEmail"]?.ToString(),
                    ApplicationNumber = reader["ApplicationNumber"]?.ToString() ?? string.Empty,
                    ApplicationDate = Convert.ToDateTime(reader["ApplicationDate"]),
                    ExpectedLeavingDate = Convert.ToDateTime(reader["ExpectedLeavingDate"]),
                    Reason = reader["Reason"]?.ToString() ?? string.Empty,
                    LibraryClearance = Convert.ToBoolean(reader["LibraryClearance"]),
                    FeeClearance = Convert.ToBoolean(reader["FeeClearance"]),
                    LabClearance = Convert.ToBoolean(reader["LabClearance"]),
                    Status = reader["Status"]?.ToString() ?? "Submitted",
                    CertificateNumber = reader["CertificateNumber"]?.ToString() ?? string.Empty,
                    IssuedDate = reader["IssuedDate"] != DBNull.Value ? Convert.ToDateTime(reader["IssuedDate"]) : DateTime.Today,
                    Remarks = reader["Remarks"]?.ToString(),
                    Conduct = reader["Conduct"]?.ToString() ?? "Good"
                };
            }

            return null;
        }
    }
}
