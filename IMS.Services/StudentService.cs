using System;
using System.Linq;
using System.Threading.Tasks;
using IMS.DAL.Interfaces;
using IMS.Helpers.Constants;
using IMS.Models.Entities;
using IMS.Models.ViewModels;
using IMS.Services.Interfaces;

namespace IMS.Services
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _repo;

        public StudentService(IStudentRepository repo)
        {
            _repo = repo;
        }

        public async Task<StudentIndexViewModel> GetStudentListAsync(
            Guid tenantId, string searchTerm, string status, Guid? branchId, int pageNumber, int pageSize)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize is < 1 or > 100 ? 10 : pageSize;

            var (items, totalCount) = await _repo.GetPagedAsync(tenantId, searchTerm, status, branchId, pageNumber, pageSize);

            var vm = new StudentIndexViewModel
            {
                SearchTerm = searchTerm,
                StatusFilter = status,
                BranchFilter = branchId,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                BranchOptions = HardcodedMasterData.GetBranchSelectList(branchId),
                StatusOptions = HardcodedMasterData.GetStatusSelectList(status)
            };

            foreach (var s in items)
            {
                vm.Students.Add(new StudentListItemViewModel
                {
                    S_Id = s.S_Id,
                    S_StudentCode = s.S_StudentCode,
                    S_AdmissionNumber = s.S_AdmissionNumber,
                    FullName = JoinName(s.S_FirstName, s.S_MiddleName, s.S_LastName),
                    S_Gender = s.S_Gender,
                    BranchName = HardcodedMasterData.GetBranchName(s.S_BranchId),
                    S_AdmissionDate = s.S_AdmissionDate,
                    S_Status = s.S_Status
                });
            }

            return vm;
        }

        public async Task<StudentDetailsViewModel> GetStudentDetailsAsync(Guid id, Guid tenantId)
        {
            var s = await _repo.GetByIdAsync(id, tenantId);
            if (s == null) return null;

            return new StudentDetailsViewModel
            {
                S_Id = s.S_Id,
                S_StudentCode = s.S_StudentCode,
                S_AdmissionNumber = s.S_AdmissionNumber,
                FullName = JoinName(s.S_FirstName, s.S_MiddleName, s.S_LastName),
                S_DateOfBirth = s.S_DateOfBirth,
                S_Gender = s.S_Gender,
                S_Email = s.S_Email,
                S_Phone = s.S_Phone,
                BranchName = HardcodedMasterData.GetBranchName(s.S_BranchId),
                S_AdmissionDate = s.S_AdmissionDate,
                S_Status = s.S_Status,
                S_CreatedAt = s.S_CreatedAt,
                S_UpdatedAt = s.S_UpdatedAt
            };
        }

        public async Task<StudentFormViewModel> GetStudentForEditAsync(Guid id, Guid tenantId)
        {
            var s = await _repo.GetByIdAsync(id, tenantId);
            if (s == null) return null;

            return new StudentFormViewModel
            {
                S_Id = s.S_Id,
                S_BranchId = s.S_BranchId,
                S_StudentCode = s.S_StudentCode,
                S_AdmissionNumber = s.S_AdmissionNumber,
                S_FirstName = s.S_FirstName,
                S_MiddleName = s.S_MiddleName,
                S_LastName = s.S_LastName,
                S_DateOfBirth = s.S_DateOfBirth,
                S_Gender = s.S_Gender,
                S_Email = s.S_Email,
                S_Phone = s.S_Phone,
                S_AdmissionDate = s.S_AdmissionDate,
                S_Status = s.S_Status,
                BranchOptions = HardcodedMasterData.GetBranchSelectList(s.S_BranchId),
                GenderOptions = HardcodedMasterData.GetGenderSelectList(s.S_Gender),
                StatusOptions = HardcodedMasterData.GetStatusSelectList(s.S_Status)
            };
        }

        public async Task<(Guid? Id, string Error)> CreateStudentAsync(StudentFormViewModel model, Guid tenantId)
        {
            if (await _repo.IsAdmissionNumberTakenAsync(tenantId, model.S_AdmissionNumber, null))
                return (null, "This admission number is already in use.");

            var studentCode = string.IsNullOrWhiteSpace(model.S_StudentCode)
                ? GenerateStudentCode(model.S_BranchId)
                : model.S_StudentCode;

            if (await _repo.IsStudentCodeTakenAsync(tenantId, studentCode, null))
                return (null, "This student code is already in use.");

            var entity = new Student
            {
                S_Id = Guid.NewGuid(),
                S_TenantId = tenantId,
                S_BranchId = model.S_BranchId,
                S_StudentCode = studentCode,
                S_AdmissionNumber = model.S_AdmissionNumber,
                S_FirstName = model.S_FirstName,
                S_MiddleName = model.S_MiddleName,
                S_LastName = model.S_LastName,
                S_DateOfBirth = model.S_DateOfBirth,
                S_Gender = model.S_Gender,
                S_Email = model.S_Email,
                S_Phone = model.S_Phone,
                S_AdmissionDate = model.S_AdmissionDate,
                S_Status = model.S_Status
            };

            var id = await _repo.CreateAsync(entity);
            return (id, null);
        }

        public async Task<(bool Success, string Error)> UpdateStudentAsync(StudentFormViewModel model, Guid tenantId)
        {
            if (!model.S_Id.HasValue)
                return (false, "Student Id is required for update.");

            if (await _repo.IsAdmissionNumberTakenAsync(tenantId, model.S_AdmissionNumber, model.S_Id))
                return (false, "This admission number is already in use.");

            if (await _repo.IsStudentCodeTakenAsync(tenantId, model.S_StudentCode, model.S_Id))
                return (false, "This student code is already in use.");

            var entity = new Student
            {
                S_Id = model.S_Id.Value,
                S_TenantId = tenantId,
                S_BranchId = model.S_BranchId,
                S_StudentCode = model.S_StudentCode,
                S_AdmissionNumber = model.S_AdmissionNumber,
                S_FirstName = model.S_FirstName,
                S_MiddleName = model.S_MiddleName,
                S_LastName = model.S_LastName,
                S_DateOfBirth = model.S_DateOfBirth,
                S_Gender = model.S_Gender,
                S_Email = model.S_Email,
                S_Phone = model.S_Phone,
                S_AdmissionDate = model.S_AdmissionDate,
                S_Status = model.S_Status
            };

            var success = await _repo.UpdateAsync(entity);
            return (success, success ? null : "Student not found or already removed.");
        }

        public Task<bool> DeleteStudentAsync(Guid id, Guid tenantId) => _repo.SoftDeleteAsync(id, tenantId);

        // ---------- helpers ----------

        private static string JoinName(string first, string middle, string last) =>
            string.Join(" ", new[] { first, middle, last }.Where(p => !string.IsNullOrWhiteSpace(p)));

        private static string GenerateStudentCode(Guid branchId)
        {
            // TEMP scheme: BR-yyMMdd-xxxx (random suffix). Replace with a proper
            // sequence-per-branch once Master Data / numbering rules are finalized.
            var suffix = Guid.NewGuid().ToString("N")[..4].ToUpperInvariant();
            return $"STU-{DateTime.UtcNow:yyMMdd}-{suffix}";
        }
    }
}
