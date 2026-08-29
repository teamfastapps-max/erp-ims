using System;
using System.Collections.Generic;
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
        private readonly IStudentDAL _repo;

        public StudentService(IStudentDAL repo)
        {
            _repo = repo;
        }

        public async Task<StudentIndexViewModel> GetStudentListAsync(
            Guid tenantId, string searchTerm, string status, Guid? branchId, Guid? classId, int pageNumber, int pageSize)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize is < 1 or > 100 ? 10 : pageSize;

            var (items, totalCount) = await _repo.GetPagedAsync(tenantId, searchTerm, status, branchId, classId, pageNumber, pageSize);

            var vm = new StudentIndexViewModel
            {
                SearchTerm = searchTerm,
                StatusFilter = status,
                BranchFilter = branchId,
                ClassFilter = classId,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                BranchOptions = HardcodedMasterData.GetBranchSelectList(branchId),
                StatusOptions = HardcodedMasterData.GetStatusSelectList(status),
                ClassOptions = HardcodedMasterData.GetClassSelectList(classId)
            };

            foreach (var s in items)
            {
                if (classId.HasValue && s.S_ClassId != classId) continue;

                vm.Students.Add(new StudentListItemViewModel
                {
                    S_Id = s.S_Id,
                    S_StudentCode = s.S_StudentCode,
                    S_AdmissionNumber = s.S_AdmissionNumber,
                    FullName = JoinName(s.S_FirstName, s.S_MiddleName, s.S_LastName),
                    S_Gender = s.S_Gender,
                    BranchName = HardcodedMasterData.GetBranchName(s.S_BranchId),
                    ClassName = HardcodedMasterData.GetClassName(s.S_ClassId),
                    SectionName = HardcodedMasterData.GetSectionName(s.S_SectionId),
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

            var guardianRecords = await _repo.GetGuardiansByStudentIdAsync(id);

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
                ClassName = HardcodedMasterData.GetClassName(s.S_ClassId),
                SectionName = HardcodedMasterData.GetSectionName(s.S_SectionId),
                S_BloodGroup = s.S_BloodGroup,
                FullAddress = JoinAddress(s),
                S_AdmissionDate = s.S_AdmissionDate,
                S_Status = s.S_Status,
                S_CreatedAt = s.S_CreatedAt,
                S_UpdatedAt = s.S_UpdatedAt,
                Guardians = guardianRecords.Select(MapGuardianRecordToRow).ToList()
            };
        }

        public async Task<StudentFormViewModel> GetStudentForEditAsync(Guid id, Guid tenantId)
        {
            var s = await _repo.GetByIdAsync(id, tenantId);
            if (s == null) return null;

            var guardianRecords = await _repo.GetGuardiansByStudentIdAsync(id);

            var vm = new StudentFormViewModel
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
                S_ClassId = s.S_ClassId,
                S_SectionId = s.S_SectionId,
                S_BloodGroup = s.S_BloodGroup,
                S_AddressLine1 = s.S_AddressLine1,
                S_AddressLine2 = s.S_AddressLine2,
                S_City = s.S_City,
                S_State = s.S_State,
                S_PostalCode = s.S_PostalCode,
                S_Country = s.S_Country,
                Guardians = guardianRecords.Select(MapGuardianRecordToRow).ToList()
            };

            PopulateDropdowns(vm);
            return vm;
        }

        public async Task<ServiceResult> CreateStudentAsync(StudentFormViewModel model, Guid tenantId)
        {
            // Blank admission number => DB auto-generates a guaranteed-unique one
            // (see SP_Students_Create); only check uniqueness if admin typed one in.
            if (!string.IsNullOrWhiteSpace(model.S_AdmissionNumber) &&
                await _repo.IsAdmissionNumberTakenAsync(tenantId, model.S_AdmissionNumber, null))
                return ServiceResult.Fail("This admission number is already in use.");

            var studentCode = string.IsNullOrWhiteSpace(model.S_StudentCode)
                ? GenerateStudentCode()
                : model.S_StudentCode;

            if (await _repo.IsStudentCodeTakenAsync(tenantId, studentCode, null))
                return ServiceResult.Fail("This student code is already in use.");

            var entity = MapToEntity(model, tenantId, Guid.NewGuid());
            entity.S_StudentCode = studentCode;

            var guardianInputs = MapGuardians(model.Guardians);

            var id = await _repo.CreateStudentWithGuardiansAsync(entity, guardianInputs);
            return ServiceResult.Ok("Student created successfully.", id);
        }

        public async Task<ServiceResult> UpdateStudentAsync(StudentFormViewModel model, Guid tenantId)
        {
            if (!model.S_Id.HasValue)
                return ServiceResult.Fail("Student Id is required for update.");

            if (await _repo.IsAdmissionNumberTakenAsync(tenantId, model.S_AdmissionNumber, model.S_Id))
                return ServiceResult.Fail("This admission number is already in use.");

            if (await _repo.IsStudentCodeTakenAsync(tenantId, model.S_StudentCode, model.S_Id))
                return ServiceResult.Fail("This student code is already in use.");

            var entity = MapToEntity(model, tenantId, model.S_Id.Value);
            var guardianInputs = MapGuardians(model.Guardians);

            var success = await _repo.UpdateStudentWithGuardiansAsync(entity, guardianInputs);
            return success
                ? ServiceResult.Ok("Student updated successfully.", model.S_Id)
                : ServiceResult.Fail("Student not found or already removed.");
        }

        public async Task<ServiceResult> DeleteStudentAsync(Guid id, Guid tenantId)
        {
            var success = await _repo.SoftDeleteAsync(id, tenantId);
            return success
                ? ServiceResult.Ok("Student removed successfully.")
                : ServiceResult.Fail("Unable to remove student.");
        }

        // ---------- helpers ----------

        public static void PopulateDropdowns(StudentFormViewModel vm)
        {
            vm.BranchOptions = HardcodedMasterData.GetBranchSelectList(vm.S_BranchId);
            vm.GenderOptions = HardcodedMasterData.GetGenderSelectList(vm.S_Gender);
            vm.StatusOptions = HardcodedMasterData.GetStatusSelectList(vm.S_Status);
            vm.ClassOptions = HardcodedMasterData.GetClassSelectList(vm.S_ClassId);
            vm.SectionOptions = HardcodedMasterData.GetSectionSelectList(vm.S_SectionId);
            vm.BloodGroupOptions = HardcodedMasterData.GetBloodGroupSelectList(vm.S_BloodGroup);
            vm.RelationOptions = HardcodedMasterData.GetRelationSelectList();
        }

        private static Student MapToEntity(StudentFormViewModel m, Guid tenantId, Guid id) => new()
        {
            S_Id = id,
            S_TenantId = tenantId,
            S_BranchId = m.S_BranchId,
            S_StudentCode = m.S_StudentCode,
            S_AdmissionNumber = m.S_AdmissionNumber,
            S_FirstName = m.S_FirstName,
            S_MiddleName = m.S_MiddleName,
            S_LastName = m.S_LastName,
            S_DateOfBirth = m.S_DateOfBirth,
            S_Gender = m.S_Gender,
            S_Email = m.S_Email,
            S_Phone = m.S_Phone,
            S_AdmissionDate = m.S_AdmissionDate,
            S_Status = m.S_Status,
            S_ClassId = m.S_ClassId,
            S_SectionId = m.S_SectionId,
            S_BloodGroup = m.S_BloodGroup,
            S_AddressLine1 = m.S_AddressLine1,
            S_AddressLine2 = m.S_AddressLine2,
            S_City = m.S_City,
            S_State = m.S_State,
            S_PostalCode = m.S_PostalCode,
            S_Country = m.S_Country
        };

        private static List<GuardianLinkInput> MapGuardians(List<GuardianRowViewModel> rows) =>
            (rows ?? new List<GuardianRowViewModel>())
                .Where(r => r.ExistingGuardianId.HasValue || !string.IsNullOrWhiteSpace(r.FirstName))
                .Select(r => new GuardianLinkInput
                {
                    ExistingGuardianId = r.ExistingGuardianId,
                    FirstName = r.FirstName,
                    LastName = r.LastName,
                    Phone = r.Phone,
                    Email = r.Email,
                    Occupation = r.Occupation,
                    Relation = r.Relation,
                    IsPrimary = r.IsPrimary
                })
                .ToList();

        private static GuardianRowViewModel MapGuardianRecordToRow(StudentGuardianRecord r) => new()
        {
            ExistingGuardianId = r.SG_GuardianId,
            FirstName = r.G_FirstName,
            LastName = r.G_LastName,
            Phone = r.G_Phone,
            Email = r.G_Email,
            Occupation = r.G_Occupation,
            Relation = r.SG_Relation,
            IsPrimary = r.SG_IsPrimary
        };

        private static string JoinName(string first, string middle, string last) =>
            string.Join(" ", new[] { first, middle, last }.Where(p => !string.IsNullOrWhiteSpace(p)));

        private static string JoinAddress(Student s)
        {
            var parts = new[] { s.S_AddressLine1, s.S_AddressLine2, s.S_City, s.S_State, s.S_PostalCode, s.S_Country }
                .Where(p => !string.IsNullOrWhiteSpace(p));
            var joined = string.Join(", ", parts);
            return string.IsNullOrWhiteSpace(joined) ? "-" : joined;
        }

        private static string GenerateStudentCode()
        {
            var suffix = Guid.NewGuid().ToString("N")[..4].ToUpperInvariant();
            return $"STU-{DateTime.UtcNow:yyMMdd}-{suffix}";
        }
    }
}
