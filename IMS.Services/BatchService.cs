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
    public class BatchService : IBatchService
    {
        private readonly IBatchDAL _repo;
        private readonly IMasterService _masterService;

        public BatchService(IBatchDAL repo, IMasterService masterService)
        {
            _repo = repo;
            _masterService = masterService;
        }

        public async Task<BatchIndexViewModel> GetBatchListAsync(
            Guid tenantId, string searchTerm, Guid? branchId, Guid? courseId,
            Guid? academicYearId, string status, int pageNumber, int pageSize)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize is < 1 or > 100 ? 10 : pageSize;

            var (items, totalCount) = await _repo.GetPagedAsync(
                tenantId, searchTerm, branchId, courseId, academicYearId, status, pageNumber, pageSize);

            var vm = new BatchIndexViewModel
            {
                SearchTerm = searchTerm,
                BranchFilter = branchId,
                CourseFilter = courseId,
                AcademicYearFilter = academicYearId,
                StatusFilter = status,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                BranchOptions = HardcodedMasterData.GetBranchSelectList(branchId),
                CourseOptions = new(),
                AcademicYearOptions = new(),
                StatusOptions = GetBatchStatusSelectList(status)
            };

            foreach (var b in items)
            {
                vm.Batches.Add(new BatchListItemViewModel
                {
                    BT_Id = b.BT_Id,
                    BT_Name = b.BT_Name,
                    BT_Code = b.BT_Code,
                    BranchName = HardcodedMasterData.GetBranchName(b.BT_BranchId),
                    CourseName = b.CourseName ?? "-",
                    AcademicYearName = b.AcademicYearName ?? "-",
                    BT_StartDate = b.BT_StartDate,
                    BT_EndDate = b.BT_EndDate,
                    BT_Capacity = b.BT_Capacity,
                    EnrolledCount = b.EnrolledCount,
                    BT_Status = b.BT_Status
                });
            }

            return vm;
        }

        public async Task<BatchDetailsViewModel> GetBatchDetailsAsync(Guid id, Guid tenantId)
        {
            var b = await _repo.GetByIdAsync(id, tenantId);
            if (b == null) return null;

            return new BatchDetailsViewModel
            {
                BT_Id = b.BT_Id,
                BT_Name = b.BT_Name,
                BT_Code = b.BT_Code,
                BranchName = HardcodedMasterData.GetBranchName(b.BT_BranchId),
                CourseName = b.CourseName ?? "-",
                AcademicYearName = b.AcademicYearName ?? "-",
                BT_StartDate = b.BT_StartDate,
                BT_EndDate = b.BT_EndDate,
                BT_Capacity = b.BT_Capacity,
                EnrolledCount = b.EnrolledCount,
                BT_Status = b.BT_Status,
                BT_CreatedAt = b.BT_CreatedAt,
                BT_UpdatedAt = b.BT_UpdatedAt
            };
        }

        public async Task<BatchFormViewModel> GetBatchForEditAsync(Guid id, Guid tenantId)
        {
            var b = await _repo.GetByIdAsync(id, tenantId);
            if (b == null) return null;

            var vm = new BatchFormViewModel
            {
                BT_Id = b.BT_Id,
                BT_BranchId = b.BT_BranchId,
                BT_CourseId = b.BT_CourseId,
                BT_AcademicYearId = b.BT_AcademicYearId,
                BT_Name = b.BT_Name,
                BT_Code = b.BT_Code,
                BT_StartDate = b.BT_StartDate,
                BT_EndDate = b.BT_EndDate,
                BT_Capacity = b.BT_Capacity,
                BT_Status = b.BT_Status
            };

            PopulateDropdowns(vm);
            return vm;
        }

        public async Task<ServiceResult> CreateBatchAsync(BatchFormViewModel model, Guid tenantId)
        {
            if (!string.IsNullOrWhiteSpace(model.BT_Code) &&
                await _repo.IsCodeTakenAsync(tenantId, model.BT_Code, null))
                return ServiceResult.Fail("This batch code is already in use.");

            var entity = MapToEntity(model, tenantId, Guid.NewGuid());
            var id = await _repo.CreateAsync(entity);
            return ServiceResult.Ok("Batch created successfully.", id);
        }

        public async Task<ServiceResult> UpdateBatchAsync(BatchFormViewModel model, Guid tenantId)
        {
            if (!model.BT_Id.HasValue)
                return ServiceResult.Fail("Batch Id is required for update.");

            if (await _repo.IsCodeTakenAsync(tenantId, model.BT_Code, model.BT_Id))
                return ServiceResult.Fail("This batch code is already in use.");

            var entity = MapToEntity(model, tenantId, model.BT_Id.Value);
            var success = await _repo.UpdateAsync(entity);
            return success
                ? ServiceResult.Ok("Batch updated successfully.", model.BT_Id)
                : ServiceResult.Fail("Batch not found.");
        }

        public async Task<ServiceResult> DeleteBatchAsync(Guid id, Guid tenantId)
        {
            var success = await _repo.DeleteAsync(id, tenantId);
            return success
                ? ServiceResult.Ok("Batch deleted successfully.")
                : ServiceResult.Fail("Unable to delete batch.");
        }

        public void PopulateDropdowns(BatchFormViewModel vm)
        {
            vm.BranchOptions = HardcodedMasterData.GetBranchSelectList(vm.BT_BranchId);
            vm.CourseOptions = GetMasterSelectList("Course", vm.BT_CourseId.ToString());
            vm.AcademicYearOptions = GetMasterSelectList("AcademicYear", vm.BT_AcademicYearId.ToString());
            vm.StatusOptions = GetBatchStatusSelectList(vm.BT_Status);
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

        private static Batch MapToEntity(BatchFormViewModel m, Guid tenantId, Guid id) => new()
        {
            BT_Id = id,
            BT_TenantId = tenantId,
            BT_BranchId = m.BT_BranchId,
            BT_CourseId = m.BT_CourseId,
            BT_AcademicYearId = m.BT_AcademicYearId,
            BT_Name = m.BT_Name,
            BT_Code = m.BT_Code,
            BT_StartDate = m.BT_StartDate,
            BT_EndDate = m.BT_EndDate,
            BT_Capacity = m.BT_Capacity,
            BT_Status = m.BT_Status
        };

        private static List<SelectListItem> GetBatchStatusSelectList(string selected = null)
        {
            var statuses = new[] { "Active", "Completed", "Cancelled" };
            return new List<SelectListItem>(
                Array.ConvertAll(statuses, s => new SelectListItem { Value = s, Text = s, Selected = s == selected }));
        }
    }
}
