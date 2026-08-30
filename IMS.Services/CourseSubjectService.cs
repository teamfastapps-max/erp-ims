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
    public class CourseSubjectService : ICourseSubjectService
    {
        private readonly ICourseSubjectDAL _repo;
        private readonly IMasterService _masterService;

        public CourseSubjectService(ICourseSubjectDAL repo, IMasterService masterService) { _repo = repo; _masterService = masterService; }

        public async Task<CourseSubjectIndexViewModel> GetListAsync(Guid tenantId, Guid? courseId)
        {
            var items = courseId.HasValue
                ? await _repo.GetByCourseIdAsync(courseId.Value, tenantId)
                : await _repo.GetAllAsync(tenantId);

            return new CourseSubjectIndexViewModel
            {
                CourseFilter = courseId,
                Items = items.ConvertAll(s => new CourseSubjectListItemViewModel
                {
                    CS_CourseId = s.CS_CourseId,
                    CS_SubjectId = s.CS_SubjectId,
                    CourseName = s.CourseName ?? "-",
                    SubjectName = s.SubjectName ?? "-",
                    CS_SequenceNo = s.CS_SequenceNo,
                    CS_IsMandatory = s.CS_IsMandatory,
                    CS_MaxMarks = s.CS_MaxMarks,
                    CS_PassMarks = s.CS_PassMarks
                })
            };
        }

        public async Task<CourseSubjectFormViewModel> GetForEditAsync(Guid courseId, Guid subjectId, Guid tenantId)
        {
            var item = await _repo.GetByIdAsync(courseId, subjectId, tenantId);
            if (item == null) return null;

            var vm = new CourseSubjectFormViewModel
            {
                CS_CourseId = item.CS_CourseId,
                CS_SubjectId = item.CS_SubjectId,
                CS_SequenceNo = item.CS_SequenceNo,
                CS_IsMandatory = item.CS_IsMandatory,
                CS_MaxMarks = item.CS_MaxMarks,
                CS_PassMarks = item.CS_PassMarks
            };
            PopulateDropdowns(vm, tenantId);
            return vm;
        }

        public async Task<ServiceResult> CreateAsync(CourseSubjectFormViewModel model, Guid tenantId)
        {
            if (await _repo.ExistsAsync(model.CS_CourseId, model.CS_SubjectId, tenantId))
                return ServiceResult.Fail("This subject is already assigned to this course.");

            var entity = MapToEntity(model, tenantId);
            var success = await _repo.CreateAsync(entity);
            return success
                ? ServiceResult.Ok("Subject added to course successfully.")
                : ServiceResult.Fail("Failed to add subject.");
        }

        public async Task<ServiceResult> UpdateAsync(CourseSubjectFormViewModel model, Guid tenantId)
        {
            var entity = MapToEntity(model, tenantId);
            var success = await _repo.UpdateAsync(entity, tenantId);
            return success
                ? ServiceResult.Ok("Course subject updated successfully.")
                : ServiceResult.Fail("Record not found.");
        }

        public async Task<ServiceResult> DeleteAsync(Guid courseId, Guid subjectId, Guid tenantId)
        {
            var success = await _repo.DeleteAsync(courseId, subjectId, tenantId);
            return success
                ? ServiceResult.Ok("Subject removed from course.")
                : ServiceResult.Fail("Unable to remove subject.");
        }

        public void PopulateDropdowns(CourseSubjectFormViewModel vm, Guid tenantId)
        {
            vm.CourseOptions = GetMasterSelectList("Course", vm.CS_CourseId.ToString());
            vm.SubjectOptions = GetMasterSelectList("Subject", vm.CS_SubjectId.ToString());
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

        private static CourseSubject MapToEntity(CourseSubjectFormViewModel m, Guid tenantId) => new()
        {
            CS_CourseId = m.CS_CourseId,
            CS_SubjectId = m.CS_SubjectId,
            CS_SequenceNo = m.CS_SequenceNo,
            CS_IsMandatory = m.CS_IsMandatory,
            CS_MaxMarks = m.CS_MaxMarks,
            CS_PassMarks = m.CS_PassMarks
        };
    }
}
