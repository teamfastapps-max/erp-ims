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
    public class EnrollmentService : IEnrollmentService
    {
        private readonly IEnrollmentDAL _repo;
        private readonly IMasterService _masterService;

        public EnrollmentService(IEnrollmentDAL repo, IMasterService masterService) { _repo = repo; _masterService = masterService; }

        public async Task<EnrollmentIndexViewModel> GetListAsync(Guid tenantId, string searchTerm, Guid? academicYearId,
            Guid? courseId, Guid? batchId, string status, int page, int pageSize)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize is < 1 or > 100 ? 10 : pageSize;

            var (items, totalCount) = await _repo.GetPagedAsync(tenantId, searchTerm, academicYearId, courseId, batchId, status, page, pageSize);

            var vm = new EnrollmentIndexViewModel
            {
                SearchTerm = searchTerm,
                AcademicYearFilter = academicYearId,
                CourseFilter = courseId,
                BatchFilter = batchId,
                StatusFilter = status,
                PageNumber = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                AcademicYearOptions = new(),
                CourseOptions = new(),
                BatchOptions = new(),
                StatusOptions = GetStatusSelectList(status)
            };

            foreach (var e in items)
            {
                vm.Enrollments.Add(new EnrollmentListItemViewModel
                {
                    E_Id = e.E_Id,
                    E_EnrollmentNumber = e.E_EnrollmentNumber,
                    StudentName = e.StudentName ?? "-",
                    CourseName = e.CourseName ?? "-",
                    BatchName = e.BatchName ?? "-",
                    AcademicYearName = e.AcademicYearName ?? "-",
                    E_EnrollmentDate = e.E_EnrollmentDate,
                    E_Status = e.E_Status
                });
            }

            return vm;
        }

        public async Task<EnrollmentDetailsViewModel> GetDetailsAsync(Guid id, Guid tenantId)
        {
            var e = await _repo.GetByIdAsync(id, tenantId);
            if (e == null) return null;
            return new EnrollmentDetailsViewModel
            {
                E_Id = e.E_Id,
                E_EnrollmentNumber = e.E_EnrollmentNumber,
                StudentName = e.StudentName ?? "-",
                CourseName = e.CourseName ?? "-",
                BatchName = e.BatchName ?? "-",
                AcademicYearName = e.AcademicYearName ?? "-",
                E_EnrollmentDate = e.E_EnrollmentDate,
                E_Status = e.E_Status,
                E_CompletionDate = e.E_CompletionDate,
                E_CreatedAt = e.E_CreatedAt,
                E_UpdatedAt = e.E_UpdatedAt
            };
        }

        public async Task<EnrollmentFormViewModel> GetForEditAsync(Guid id, Guid tenantId)
        {
            var e = await _repo.GetByIdAsync(id, tenantId);
            if (e == null) return null;
            var vm = new EnrollmentFormViewModel
            {
                E_Id = e.E_Id,
                E_StudentId = e.E_StudentId,
                E_AcademicYearId = e.E_AcademicYearId,
                E_CourseId = e.E_CourseId,
                E_BatchId = e.E_BatchId,
                E_EnrollmentNumber = e.E_EnrollmentNumber,
                E_EnrollmentDate = e.E_EnrollmentDate,
                E_Status = e.E_Status,
                E_CompletionDate = e.E_CompletionDate
            };
            PopulateDropdowns(vm);
            return vm;
        }

        public async Task<ServiceResult> CreateAsync(EnrollmentFormViewModel model, Guid tenantId)
        {
            if (await _repo.IsDuplicateAsync(tenantId, model.E_StudentId, model.E_BatchId, null))
                return ServiceResult.Fail("This student is already enrolled in this batch.");

            var entity = MapToEntity(model, tenantId, Guid.NewGuid());
            var id = await _repo.CreateAsync(entity);
            return ServiceResult.Ok("Enrollment created successfully.", id);
        }

        public async Task<ServiceResult> UpdateAsync(EnrollmentFormViewModel model, Guid tenantId)
        {
            if (!model.E_Id.HasValue)
                return ServiceResult.Fail("Enrollment Id is required.");

            if (await _repo.IsDuplicateAsync(tenantId, model.E_StudentId, model.E_BatchId, model.E_Id))
                return ServiceResult.Fail("This student is already enrolled in this batch.");

            var entity = MapToEntity(model, tenantId, model.E_Id.Value);
            var success = await _repo.UpdateAsync(entity);
            return success
                ? ServiceResult.Ok("Enrollment updated.", model.E_Id)
                : ServiceResult.Fail("Enrollment not found.");
        }

        public async Task<ServiceResult> DeleteAsync(Guid id, Guid tenantId)
        {
            var success = await _repo.DeleteAsync(id, tenantId);
            return success
                ? ServiceResult.Ok("Enrollment deleted.")
                : ServiceResult.Fail("Unable to delete enrollment.");
        }

        public void PopulateDropdowns(EnrollmentFormViewModel vm)
        {
            vm.StudentOptions = GetMasterSelectList("Student", vm.E_StudentId.ToString());
            vm.AcademicYearOptions = GetMasterSelectList("AcademicYear", vm.E_AcademicYearId.ToString());
            vm.CourseOptions = GetMasterSelectList("Course", vm.E_CourseId.ToString());
            vm.BatchOptions = GetMasterSelectList("Batch", vm.E_BatchId.ToString());
            vm.StatusOptions = GetStatusSelectList(vm.E_Status);
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

        private static Enrollment MapToEntity(EnrollmentFormViewModel m, Guid tenantId, Guid id) => new()
        {
            E_Id = id,
            E_TenantId = tenantId,
            E_StudentId = m.E_StudentId,
            E_AcademicYearId = m.E_AcademicYearId,
            E_CourseId = m.E_CourseId,
            E_BatchId = m.E_BatchId,
            E_EnrollmentNumber = m.E_EnrollmentNumber,
            E_EnrollmentDate = m.E_EnrollmentDate,
            E_Status = m.E_Status,
            E_CompletionDate = m.E_CompletionDate
        };

        private static System.Collections.Generic.List<SelectListItem> GetStatusSelectList(string selected = null)
        {
            var statuses = new[] { "Active", "Completed", "Withdrawn", "Transferred" };
            return new System.Collections.Generic.List<SelectListItem>(
                Array.ConvertAll(statuses, s => new SelectListItem { Value = s, Text = s, Selected = s == selected }));
        }
    }
}
