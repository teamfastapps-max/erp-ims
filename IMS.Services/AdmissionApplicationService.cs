using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IMS.DAL.Interfaces;
using IMS.Helpers.Constants;
using IMS.Models.Entities;
using IMS.Models.ViewModels;
using IMS.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IMS.Services
{
    public class AdmissionApplicationService : IAdmissionApplicationService
    {
        private readonly IAdmissionApplicationDAL _repo;
        private readonly IMasterService _masterService;
        public AdmissionApplicationService(IAdmissionApplicationDAL repo, IMasterService masterService) { _repo = repo; _masterService = masterService; }

        public async Task<AdmissionApplicationIndexViewModel> GetListAsync(Guid tenantId, string searchTerm, Guid? branchId,
            Guid? courseId, Guid? academicYearId, string status, int page, int pageSize)
        {
            page = page < 1 ? 1 : page;
            var (items, totalCount) = await _repo.GetPagedAsync(tenantId, searchTerm, branchId, courseId, academicYearId, status, page, pageSize);

            return new AdmissionApplicationIndexViewModel
            {
                SearchTerm = searchTerm,
                BranchFilter = branchId,
                CourseFilter = courseId,
                AcademicYearFilter = academicYearId,
                StatusFilter = status,
                PageNumber = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                BranchOptions = new(),
                CourseOptions = new(),
                AcademicYearOptions = new(),
                StatusOptions = GetStatusSelectList(status),
                Applications = items.ConvertAll(a => new AdmissionApplicationListItemViewModel
                {
                    AA_Id = a.AA_Id,
                    AA_ApplicationNumber = a.AA_ApplicationNumber,
                    AA_FirstName = a.AA_FirstName,
                    AA_LastName = a.AA_LastName,
                    CourseName = a.CourseName ?? "-",
                    AcademicYearName = a.AcademicYearName ?? "-",
                    AA_SubmittedAt = a.AA_SubmittedAt,
                    AA_Status = a.AA_Status
                })
            };
        }

        public async Task<AdmissionApplicationDetailsViewModel> GetDetailsAsync(Guid id, Guid tenantId)
        {
            var a = await _repo.GetByIdAsync(id, tenantId);
            if (a == null) return null;
            return new AdmissionApplicationDetailsViewModel
            {
                AA_Id = a.AA_Id,
                AA_ApplicationNumber = a.AA_ApplicationNumber,
                AA_FirstName = a.AA_FirstName,
                AA_LastName = a.AA_LastName,
                AA_DateOfBirth = a.AA_DateOfBirth,
                AA_Gender = a.AA_Gender,
                AA_Email = a.AA_Email,
                AA_Phone = a.AA_Phone,
                CourseName = a.CourseName ?? "-",
                AcademicYearName = a.AcademicYearName ?? "-",
                AA_Status = a.AA_Status,
                AA_SubmittedAt = a.AA_SubmittedAt,
                AA_ReviewedAt = a.AA_ReviewedAt,
                AA_Notes = a.AA_Notes,
                AA_CreatedAt = a.AA_CreatedAt,
                AA_UpdatedAt = a.AA_UpdatedAt
            };
        }

        public async Task<AdmissionApplicationFormViewModel> GetForEditAsync(Guid id, Guid tenantId)
        {
            var a = await _repo.GetByIdAsync(id, tenantId);
            if (a == null) return null;
            var vm = new AdmissionApplicationFormViewModel
            {
                AA_Id = a.AA_Id,
                AA_BranchId = a.AA_BranchId,
                AA_ApplicationNumber = a.AA_ApplicationNumber,
                AA_FirstName = a.AA_FirstName,
                AA_LastName = a.AA_LastName,
                AA_DateOfBirth = a.AA_DateOfBirth,
                AA_Gender = a.AA_Gender,
                AA_Email = a.AA_Email,
                AA_Phone = a.AA_Phone,
                AA_CourseId = a.AA_CourseId,
                AA_AcademicYearId = a.AA_AcademicYearId,
                AA_Status = a.AA_Status,
                AA_Notes = a.AA_Notes
            };
            PopulateDropdowns(vm);
            return vm;
        }

        public async Task<ServiceResult> CreateAsync(AdmissionApplicationFormViewModel model, Guid tenantId)
        {
            if (!string.IsNullOrWhiteSpace(model.AA_ApplicationNumber) &&
                await _repo.IsApplicationNumberTakenAsync(tenantId, model.AA_ApplicationNumber, null))
                return ServiceResult.Fail("This application number is already in use.");

            var entity = MapToEntity(model, tenantId, Guid.NewGuid());
            entity.AA_SubmittedAt = DateTime.UtcNow;
            var id = await _repo.CreateAsync(entity);
            return ServiceResult.Ok("Application submitted successfully.", id);
        }

        public async Task<ServiceResult> UpdateAsync(AdmissionApplicationFormViewModel model, Guid tenantId)
        {
            if (!model.AA_Id.HasValue) return ServiceResult.Fail("Id required.");
            if (await _repo.IsApplicationNumberTakenAsync(tenantId, model.AA_ApplicationNumber, model.AA_Id))
                return ServiceResult.Fail("This application number is already in use.");

            var entity = MapToEntity(model, tenantId, model.AA_Id.Value);
            var success = await _repo.UpdateAsync(entity);
            return success ? ServiceResult.Ok("Updated.", model.AA_Id) : ServiceResult.Fail("Not found.");
        }

        public async Task<ServiceResult> DeleteAsync(Guid id, Guid tenantId)
        {
            var success = await _repo.DeleteAsync(id, tenantId);
            return success ? ServiceResult.Ok("Deleted.") : ServiceResult.Fail("Unable to delete.");
        }

        public async Task<ServiceResult> ReviewAsync(AdmissionReviewViewModel model, Guid tenantId, Guid reviewedBy)
        {
            var success = await _reviewAsync(model.AA_Id, model.AA_Status, model.AA_Notes, tenantId, reviewedBy);
            return success ? ServiceResult.Ok($"Application {model.AA_Status.ToLower()}.") : ServiceResult.Fail("Application not found.");
        }

        private async Task<bool> _reviewAsync(Guid id, string status, string notes, Guid tenantId, Guid reviewedBy)
        {
            return await _repo.ReviewAsync(id, status, notes, tenantId, reviewedBy);
        }

        public void PopulateDropdowns(AdmissionApplicationFormViewModel vm)
        {
            vm.BranchOptions = HardcodedMasterData.GetBranchSelectList(vm.AA_BranchId);
            vm.CourseOptions = GetMasterSelectList("Course", vm.AA_CourseId?.ToString());
            vm.AcademicYearOptions = GetMasterSelectList("AcademicYear", vm.AA_AcademicYearId.ToString());
            vm.GenderOptions = new()
            {
                new("Male", "Male"), new("Female", "Female"), new("Other", "Other")
            };
            vm.StatusOptions = GetStatusSelectList(vm.AA_Status);
        }

        private List<SelectListItem> GetMasterSelectList(string entityType, string selectedValue = null)
        {
            var items = _masterService.GetAll(entityType);
            var list = new List<SelectListItem>();
            if (items == null) return list;
            foreach (var item in items)
            {
                var keyEntry = item.FirstOrDefault(kvp => kvp.Key.EndsWith("_Id"));
                var id = keyEntry.Value?.ToString() ?? "";

                string displayName = null;
                var nameEntry = item.FirstOrDefault(kvp => kvp.Key.EndsWith("_Name"));
                if (nameEntry.Value != null) displayName = nameEntry.Value.ToString();

                if (string.IsNullOrEmpty(displayName))
                {
                    var firstName = item.FirstOrDefault(kvp => kvp.Key.EndsWith("_FirstName")).Value?.ToString() ?? "";
                    var lastName = item.FirstOrDefault(kvp => kvp.Key.EndsWith("_LastName")).Value?.ToString() ?? "";
                    displayName = $"{firstName} {lastName}".Trim();
                }

                if (string.IsNullOrEmpty(displayName))
                    displayName = item.Values.ElementAtOrDefault(1)?.ToString() ?? id;

                list.Add(new SelectListItem { Value = id, Text = displayName, Selected = id == selectedValue });
            }
            return list;
        }

        private static AdmissionApplication MapToEntity(AdmissionApplicationFormViewModel m, Guid tenantId, Guid id) => new()
        {
            AA_Id = id,
            AA_TenantId = tenantId,
            AA_BranchId = m.AA_BranchId,
            AA_ApplicationNumber = m.AA_ApplicationNumber,
            AA_FirstName = m.AA_FirstName,
            AA_LastName = m.AA_LastName,
            AA_DateOfBirth = m.AA_DateOfBirth,
            AA_Gender = m.AA_Gender,
            AA_Email = m.AA_Email,
            AA_Phone = m.AA_Phone,
            AA_CourseId = m.AA_CourseId,
            AA_AcademicYearId = m.AA_AcademicYearId,
            AA_Status = m.AA_Status,
            AA_Notes = m.AA_Notes
        };

        private static System.Collections.Generic.List<SelectListItem> GetStatusSelectList(string selected = null)
        {
            var statuses = new[] { "Submitted", "UnderReview", "Approved", "Rejected", "Waitlisted" };
            return new System.Collections.Generic.List<SelectListItem>(
                Array.ConvertAll(statuses, s => new SelectListItem { Value = s, Text = s, Selected = s == selected }));
        }
    }
}
