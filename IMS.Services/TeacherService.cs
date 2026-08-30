using IMS.DAL.Interfaces;
using IMS.Helpers.Constants;
using IMS.Models.Teacher;
using IMS.Models.TenantUser;
using IMS.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Services
{
    public class TeacherService : ITeacherService
    {
        private readonly ITeacherDAL _repo;
        private readonly IUserApiService _userApi;
        private readonly IRoleApiService _roleApi;
        private const string PendingSetupLabel = "Pending Setup";
        private const string TeacherRoleName = "Teacher";
        public TeacherService(ITeacherDAL repo, IUserApiService userApi, IRoleApiService roleApi)
        {
            _repo = repo;
            _userApi = userApi;
            _roleApi = roleApi;
        }

        public async Task<TeacherIndexViewModel> GetTeacherListAsync(Guid tenantId, string accessToken, string searchTerm, string status, Guid? branchId, int pageNumber, int pageSize)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize is < 1 or > 100 ? 10 : pageSize;

            var apiPage = await _userApi.GetTenantUsersAsync(pageNumber, pageSize, accessToken);
            var teacherDocs = apiPage.Docs.Where(u => string.Equals(u.CustomRoleName, TeacherRoleName, StringComparison.OrdinalIgnoreCase)).ToList();
            var userIds = teacherDocs
                .Where(u => Guid.TryParse(u.Id, out _))
                .Select(u => Guid.Parse(u.Id))
                .ToList();

            var profiles = await _repo.GetProfilesByIdsAsync(userIds, tenantId);
            var profileById = profiles.ToDictionary(p => p.T_Id);
            var vm = new TeacherIndexViewModel
            {
                SearchTerm = searchTerm,
                StatusFilter = status,
                BranchFilter = branchId,
                PageNumber = pageNumber,
                PageSize = pageSize,
                //TotalCount = apiPage.Meta?.TotalDocs ?? apiPage.Docs.Count,
                TotalCount = teacherDocs.Count,
                BranchOptions = HardcodedMasterData.GetBranchSelectList(branchId),
                StatusOptions = BuildTeacherStatusOptions(status)
            };

            foreach (var u in teacherDocs)
            {
                if (!Guid.TryParse(u.Id, out var id)) continue;
                profileById.TryGetValue(id, out var profile);
                var isPending = profile == null;

                if (branchId.HasValue && (isPending || profile.T_BranchId != branchId.Value)) continue;
                if (!string.IsNullOrWhiteSpace(status) && (isPending || !string.Equals(profile.T_Status, status, StringComparison.OrdinalIgnoreCase))) continue;
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var haystack = $"{u.FullName} {profile?.T_EmployeeCode} {u.Email} {u.Phone}";
                    if (haystack.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) < 0) continue;
                }

                vm.Teachers.Add(new TeacherListItemViewModel
                {
                    T_Id = id,
                    T_EmployeeCode = profile?.T_EmployeeCode,
                    FullName = u.FullName,
                    Email = u.Email,
                    Phone = u.Phone,
                    BranchName = isPending ? "-" : HardcodedMasterData.GetBranchName(profile.T_BranchId),
                    Designation = profile?.T_Designation,
                    Department = profile?.T_Department,
                    T_Status = isPending ? PendingSetupLabel : profile.T_Status,
                    IsPendingSetup = isPending
                });
            }
            return vm;
        }

        public async Task<TeacherDetailsViewModel> GetTeacherDetailsAsync(Guid id, Guid tenantId, string accessToken)
        {
            var identityTask = _userApi.GetTenantUserByIdAsync(id.ToString(), accessToken);
            var profileTask = _repo.GetByIdAsync(id, tenantId);
            await Task.WhenAll(identityTask, profileTask);

            var identity = identityTask.Result;
            var profile = profileTask.Result;
            if (identity == null || profile == null) return null;

            return new TeacherDetailsViewModel
            {
                T_Id = id,
                T_EmployeeCode = profile.T_EmployeeCode,
                FullName = identity.FullName,
                Email = identity.Email,
                Phone = identity.Phone,
                FullAddress = JoinAddress(identity.Location),
                BranchName = HardcodedMasterData.GetBranchName(profile.T_BranchId),
                RoleName = identity.CustomRoleName,
                Designation = profile.T_Designation,
                Department = profile.T_Department,
                T_JoiningDate = profile.T_JoiningDate,
                T_Qualification = profile.T_Qualification,
                T_ExperienceYears = profile.T_ExperienceYears,
                T_BloodGroup = profile.T_BloodGroup,
                T_Status = profile.T_Status,
                CreatedAt = profile.T_CreatedAt
            };
        }

        public async Task<TeacherFormViewModel> GetNewTeacherFormAsync(Guid tenantId, string accessToken)
        {
            var vm = new TeacherFormViewModel { T_Status = "Active" };
            await PopulateDropdownsAsync(vm, accessToken);
            return vm;
        }

        public async Task<TeacherFormViewModel> GetTeacherForEditAsync(Guid id, Guid tenantId, string accessToken)
        {
            var identityTask = _userApi.GetTenantUserByIdAsync(id.ToString(), accessToken);
            var profileTask = _repo.GetByIdAsync(id, tenantId);
            await Task.WhenAll(identityTask, profileTask);

            var identity = identityTask.Result;
            if (identity == null) return null;

            var profile = profileTask.Result;
            var hasProfile = profile != null && profile.T_IsActive;
            var vm = new TeacherFormViewModel
            {
                T_Id = id,
                Email = identity.Email,
                FirstName = identity.FirstName,
                LastName = identity.LastName,
                Phone = identity.Phone,
                CustomRoleId = identity.CustomRoleId,
                AddressLine1 = identity.Location?.AddressLine1,
                AddressLine2 = identity.Location?.AddressLine2,
                City = identity.Location?.City,
                State = identity.Location?.State,
                PostalCode = identity.Location?.PostalCode,
                Country = identity.Location?.Country,
                T_BranchId = hasProfile ? profile.T_BranchId : Guid.Empty,
                T_EmployeeCode = hasProfile ? profile.T_EmployeeCode : null,
                T_Designation = hasProfile ? profile.T_Designation : null,
                T_Department = hasProfile ? profile.T_Department : null,
                T_JoiningDate = hasProfile ? profile.T_JoiningDate : null,
                T_Qualification = hasProfile ? profile.T_Qualification : null,
                T_ExperienceYears = hasProfile ? profile.T_ExperienceYears : null,
                T_BloodGroup = hasProfile ? profile.T_BloodGroup : null,
                T_Status = hasProfile ? profile.T_Status : "Active",
            };

            await PopulateDropdownsAsync(vm, accessToken);
            return vm;
        }

        public async Task<ServiceResult> CreateTeacherAsync(TeacherFormViewModel model, Guid tenantId, string accessToken)
        {
            if (!IsStrongPassword(model.Password))
                return ServiceResult.Fail("Password must be at least 8 characters and include an uppercase letter, a lowercase letter, a number, and a special character.");

            var employeeCode = string.IsNullOrWhiteSpace(model.T_EmployeeCode)? GenerateEmployeeCode() : model.T_EmployeeCode;

            if (await _repo.IsEmployeeCodeTakenAsync(tenantId, employeeCode, null))
                return ServiceResult.Fail("This employee code is already in use.");

            var createRequest = new CreateTenantUserRequest
            {
                Email = model.Email,
                Password = model.Password,
                FirstName = model.FirstName,
                LastName = model.LastName,
                UserType = "TENANT_USER",
                CustomRoleId = model.CustomRoleId,
                Location = BuildLocation(model)
            };

            var created = await _userApi.CreateTenantUserAsync(createRequest, accessToken);
            if (!created.Success || created.Data == null || !Guid.TryParse(created.Data.Id, out var newId))
                return ServiceResult.Fail(created.ErrorMessage ?? "Could not create the teacher's login account. Please try again.");

            var entity = new Teacher
            {
                T_Id = newId,
                T_TenantId = tenantId,
                T_BranchId = model.T_BranchId,
                T_EmployeeCode = employeeCode,
                T_Designation = model.T_Designation,
                T_Department = model.T_Department,
                T_JoiningDate = model.T_JoiningDate,
                T_Qualification = model.T_Qualification,
                T_ExperienceYears = model.T_ExperienceYears,
                T_BloodGroup = model.T_BloodGroup,
                T_Status = string.IsNullOrWhiteSpace(model.T_Status) ? "Active" : model.T_Status
            };

            try
            {
                await _repo.AddEditTeacherProfileAsync(entity);
            }
            catch
            {
                await _userApi.DeleteTenantUserAsync(created.Data.Id, accessToken);
                throw;
            }

            return ServiceResult.Ok("Teacher created successfully.", newId);
        }

        public async Task<ServiceResult> UpdateTeacherAsync(TeacherFormViewModel model, Guid tenantId, string accessToken)
        {
            if (!model.T_Id.HasValue)
                return ServiceResult.Fail("Teacher Id is required for update.");

            var employeeCode = string.IsNullOrWhiteSpace(model.T_EmployeeCode)? GenerateEmployeeCode() : model.T_EmployeeCode;

            if (await _repo.IsEmployeeCodeTakenAsync(tenantId, employeeCode, model.T_Id))
                return ServiceResult.Fail("This employee code is already in use.");

            var updateRequest = new UpdateTenantUserRequest
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Phone = model.Phone,
                Location = BuildLocation(model),
                UserType = "TENANT_USER",
                CustomRoleId = model.CustomRoleId
            };

            var updated = await _userApi.UpdateTenantUserAsync(model.T_Id.Value.ToString(), updateRequest, accessToken);
            if (!updated.Success || updated.Data == null)
                return ServiceResult.Fail(updated.ErrorMessage ?? "Could not update the teacher's account. Please try again.");

            var entity = new Teacher
            {
                T_Id = model.T_Id.Value,
                T_TenantId = tenantId,
                T_BranchId = model.T_BranchId,
                T_EmployeeCode = employeeCode,
                T_Designation = model.T_Designation,
                T_Department = model.T_Department,
                T_JoiningDate = model.T_JoiningDate,
                T_Qualification = model.T_Qualification,
                T_ExperienceYears = model.T_ExperienceYears,
                T_BloodGroup = model.T_BloodGroup,
                T_Status = model.T_Status
            };

            var success = await _repo.AddEditTeacherProfileAsync(entity);
            return success
                ? ServiceResult.Ok("Teacher updated successfully.", model.T_Id)
                : ServiceResult.Fail("Could not save the teacher's profile. Please try again.");
        }

        public async Task<ServiceResult> DeleteTeacherAsync(Guid id, Guid tenantId, string accessToken)
        {
            var apiDeleted = await _userApi.DeleteTenantUserAsync(id.ToString(), accessToken);
            if (!apiDeleted)
                return ServiceResult.Fail("Could not remove the teacher's login account. Please try again.");

            var localDeleted = await _repo.SoftDeleteAsync(id, tenantId);
            return localDeleted? ServiceResult.Ok("Teacher removed successfully.")
                : ServiceResult.Fail("Login account removed, but the teacher profile was already gone.");
        }

        // ---------- helpers ----------

        private async Task PopulateDropdownsAsync(TeacherFormViewModel vm, string accessToken)
        {
            vm.BranchOptions = HardcodedMasterData.GetBranchSelectList(vm.T_BranchId);
            vm.StatusOptions = BuildTeacherStatusOptions(vm.T_Status);
            vm.DesignationOptions = BuildDesignationOptions(vm.T_Designation);
            vm.BloodGroupOptions = HardcodedMasterData.GetBloodGroupSelectList(vm.T_BloodGroup);

            var roles = await _roleApi.GetTenantRolesAsync(accessToken);
            vm.RoleOptions = roles.Select(r => new SelectListItem
            {
                Value = r.Id,
                Text = r.Name,
                Selected = r.Id == vm.CustomRoleId
            });
        }

        private static LocationModel BuildLocation(TeacherFormViewModel m) => new()
        {
            Label = "Primary",
            AddressLine1 = m.AddressLine1,
            AddressLine2 = m.AddressLine2,
            City = m.City,
            State = m.State,
            PostalCode = m.PostalCode,
            Country = m.Country
        };

        private static string JoinAddress(LocationModel loc)
        {
            if (loc == null) return "-";
            var parts = new[] { loc.AddressLine1, loc.AddressLine2, loc.City, loc.State, loc.PostalCode, loc.Country }
                .Where(p => !string.IsNullOrWhiteSpace(p));
            var joined = string.Join(", ", parts);
            return string.IsNullOrWhiteSpace(joined) ? "-" : joined;
        }

        private static IEnumerable<SelectListItem> BuildTeacherStatusOptions(string selected) =>
            new[] { "Active", "OnLeave", "Resigned", "Terminated" }
                .Select(s => new SelectListItem { Value = s, Text = s, Selected = s == selected });

        private static IEnumerable<SelectListItem> BuildDesignationOptions(string selected) =>
            new[] { "Teacher", "Senior Teacher", "Head of Department", "Vice Principal", "Principal" }
                .Select(d => new SelectListItem { Value = d, Text = d, Selected = d == selected });

        private static string GenerateEmployeeCode()
        {
            var suffix = Guid.NewGuid().ToString("N")[..4].ToUpperInvariant();
            return $"EMP-{DateTime.UtcNow:yyMMdd}-{suffix}";
        }

        /// <summary>
        /// Server-side backstop for a security-sensitive field: min 8 chars, at least
        /// one uppercase, one lowercase, one digit, one special character. The primary
        /// UX for this lives in teacher.js (matches this rule exactly, same as the
        /// student module's client-side-only validation) — this is a safety net in
        /// case someone bypasses the client (e.g. a raw API call), not a first line of defense.
        /// </summary>
        private static bool IsStrongPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8) return false;
            return password.Any(char.IsUpper)
                && password.Any(char.IsLower)
                && password.Any(char.IsDigit)
                && password.Any(c => !char.IsLetterOrDigit(c));
        }
    }
}
