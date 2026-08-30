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
    public class ExamService : IExamService
    {
        private readonly IExamDAL _examDAL;
        private readonly IExamSubjectDAL _examSubjectDAL;
        private readonly IMarkDAL _markDAL;
        private readonly IResultDAL _resultDAL;
        private readonly IMasterService _masterService;

        public ExamService(IExamDAL examDAL, IExamSubjectDAL examSubjectDAL, IMarkDAL markDAL,
            IResultDAL resultDAL, IMasterService masterService)
        {
            _examDAL = examDAL;
            _examSubjectDAL = examSubjectDAL;
            _markDAL = markDAL;
            _resultDAL = resultDAL;
            _masterService = masterService;
        }

        public async Task<ExamIndexViewModel> GetExamListAsync(
            Guid tenantId, string searchTerm, Guid? courseId, Guid? batchId,
            string status, int pageNumber, int pageSize)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize is < 1 or > 100 ? 10 : pageSize;

            var (items, totalCount) = await _examDAL.GetPagedAsync(
                tenantId, searchTerm, courseId, batchId, status, pageNumber, pageSize);

            var vm = new ExamIndexViewModel
            {
                SearchTerm = searchTerm,
                CourseFilter = courseId,
                BatchFilter = batchId,
                StatusFilter = status,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                CourseOptions = GetMasterSelectList("Course", courseId?.ToString()),
                BatchOptions = GetMasterSelectList("Batch", batchId?.ToString()),
                StatusOptions = GetExamStatusSelectList(status)
            };

            foreach (var e in items)
            {
                vm.Exams.Add(new ExamListItemViewModel
                {
                    EX_Id = e.EX_Id,
                    EX_Code = e.EX_Code,
                    EX_Name = e.EX_Name,
                    ExamTypeName = e.ExamTypeName ?? "-",
                    CourseName = e.CourseName ?? "-",
                    BatchName = e.BatchName ?? "-",
                    AcademicYearName = e.AcademicYearName ?? "-",
                    EX_StartDate = e.EX_StartDate,
                    EX_EndDate = e.EX_EndDate,
                    EX_Status = e.EX_Status
                });
            }

            return vm;
        }

        public async Task<ExamDetailsViewModel> GetExamDetailsAsync(Guid id, Guid tenantId)
        {
            var e = await _examDAL.GetByIdAsync(id, tenantId);
            if (e == null) return null;

            var subjects = await _examSubjectDAL.GetByExamIdAsync(id);

            return new ExamDetailsViewModel
            {
                EX_Id = e.EX_Id,
                EX_Code = e.EX_Code,
                EX_Name = e.EX_Name,
                ExamTypeName = e.ExamTypeName ?? "-",
                CourseName = e.CourseName ?? "-",
                BatchName = e.BatchName ?? "-",
                AcademicYearName = e.AcademicYearName ?? "-",
                EX_StartDate = e.EX_StartDate,
                EX_EndDate = e.EX_EndDate,
                EX_Status = e.EX_Status,
                EX_CreatedAt = e.EX_CreatedAt,
                EX_UpdatedAt = e.EX_UpdatedAt,
                Subjects = subjects.Select(s => new ExamSubjectViewModel
                {
                    ES_Id = s.ES_Id,
                    ES_SubjectId = s.ES_SubjectId,
                    ES_MaxMarks = s.ES_MaxMarks,
                    ES_PassMarks = s.ES_PassMarks,
                    ES_Weightage = s.ES_Weightage
                }).ToList()
            };
        }

        public async Task<ExamFormViewModel> GetExamForEditAsync(Guid id, Guid tenantId)
        {
            var e = await _examDAL.GetByIdAsync(id, tenantId);
            if (e == null) return null;

            var vm = new ExamFormViewModel
            {
                EX_Id = e.EX_Id,
                EX_AcademicYearId = e.EX_AcademicYearId,
                EX_CourseId = e.EX_CourseId,
                EX_BatchId = e.EX_BatchId,
                EX_ExamTypeId = e.EX_ExamTypeId,
                EX_Name = e.EX_Name,
                EX_Code = e.EX_Code,
                EX_StartDate = e.EX_StartDate,
                EX_EndDate = e.EX_EndDate,
                EX_Status = e.EX_Status
            };

            PopulateDropdowns(vm);
            return vm;
        }

        public async Task<ServiceResult> CreateExamAsync(ExamFormViewModel model, Guid tenantId)
        {
            if (!string.IsNullOrWhiteSpace(model.EX_Code) &&
                await _examDAL.IsCodeTakenAsync(tenantId, model.EX_Code, null))
                return ServiceResult.Fail("This exam code is already in use.");

            var entity = MapToEntity(model, tenantId, Guid.NewGuid());
            var id = await _examDAL.CreateAsync(entity);
            return ServiceResult.Ok("Exam created successfully.", id);
        }

        public async Task<ServiceResult> UpdateExamAsync(ExamFormViewModel model, Guid tenantId)
        {
            if (!model.EX_Id.HasValue)
                return ServiceResult.Fail("Exam Id is required for update.");

            if (await _examDAL.IsCodeTakenAsync(tenantId, model.EX_Code, model.EX_Id))
                return ServiceResult.Fail("This exam code is already in use.");

            var entity = MapToEntity(model, tenantId, model.EX_Id.Value);
            var success = await _examDAL.UpdateAsync(entity);
            return success
                ? ServiceResult.Ok("Exam updated successfully.", model.EX_Id)
                : ServiceResult.Fail("Exam not found.");
        }

        public async Task<ServiceResult> DeleteExamAsync(Guid id, Guid tenantId)
        {
            var success = await _examDAL.DeleteAsync(id, tenantId);
            return success
                ? ServiceResult.Ok("Exam deleted successfully.")
                : ServiceResult.Fail("Unable to delete exam.");
        }

        public async Task<MarksEntryViewModel> GetMarksEntryAsync(Guid examId, Guid tenantId)
        {
            var exam = await _examDAL.GetByIdAsync(examId, tenantId);
            if (exam == null) return null;

            var subjects = await _examSubjectDAL.GetByExamIdAsync(examId);
            var existingMarks = await _markDAL.GetByExamIdAsync(examId);
            var marksBySubject = existingMarks.GroupBy(m => m.M_ExamSubjectId)
                .ToDictionary(g => g.Key, g => g.ToDictionary(m => m.M_StudentId, m => m));

            var students = _masterService.GetAll("Student") ?? new List<Dictionary<string, object>>();

            var vm = new MarksEntryViewModel
            {
                ExamId = examId,
                ExamName = exam.EX_Name,
                ExamCode = exam.EX_Code
            };

            foreach (var subject in subjects)
            {
                var subjectEntry = new ExamSubjectEntryViewModel
                {
                    ExamSubjectId = subject.ES_Id,
                    SubjectName = subject.SubjectName ?? "-",
                    MaxMarks = subject.ES_MaxMarks,
                    PassMarks = subject.ES_PassMarks
                };

                marksBySubject.TryGetValue(subject.ES_Id, out var subjectMarks);

                foreach (var student in students)
                {
                    var idEntry = student.FirstOrDefault(kvp => kvp.Key.EndsWith("_Id"));
                    if (idEntry.Value == null) continue;
                    var studentId = Guid.Parse(idEntry.Value.ToString());
                    var firstName = student.FirstOrDefault(kvp => kvp.Key.EndsWith("_FirstName")).Value?.ToString() ?? "";
                    var lastName = student.FirstOrDefault(kvp => kvp.Key.EndsWith("_LastName")).Value?.ToString() ?? "";
                    var studentCode = student.FirstOrDefault(kvp => kvp.Key.EndsWith("_StudentCode")).Value?.ToString() ?? "";

                    Mark existingMark = null;
                    subjectMarks?.TryGetValue(studentId, out existingMark);

                    subjectEntry.Students.Add(new StudentMarkEntryViewModel
                    {
                        StudentId = studentId,
                        StudentCode = studentCode,
                        StudentName = $"{firstName} {lastName}".Trim(),
                        MarksObtained = existingMark?.M_MarksObtained,
                        Remarks = existingMark?.M_Remarks
                    });
                }

                vm.Subjects.Add(subjectEntry);
            }

            return vm;
        }

        public async Task<ServiceResult> SaveMarksAsync(MarksEntryViewModel model, Guid tenantId)
        {
            var allMarks = new List<Mark>();
            foreach (var subject in model.Subjects)
            {
                foreach (var student in subject.Students)
                {
                    if (student.MarksObtained.HasValue)
                    {
                        allMarks.Add(new Mark
                        {
                            M_Id = Guid.NewGuid(),
                            M_ExamSubjectId = subject.ExamSubjectId,
                            M_StudentId = student.StudentId,
                            M_MarksObtained = student.MarksObtained.Value,
                            M_Remarks = student.Remarks
                        });
                    }
                }
            }

            var success = await _markDAL.SaveMarksAsync(model.ExamId, allMarks);
            return success
                ? ServiceResult.Ok("Marks saved successfully.")
                : ServiceResult.Fail("Failed to save marks.");
        }

        public void PopulateDropdowns(ExamFormViewModel vm)
        {
            vm.AcademicYearOptions = GetMasterSelectList("AcademicYear", vm.EX_AcademicYearId.ToString());
            vm.CourseOptions = GetMasterSelectList("Course", vm.EX_CourseId.ToString());
            vm.BatchOptions = GetMasterSelectList("Batch", vm.EX_BatchId.ToString());
            vm.ExamTypeOptions = GetMasterSelectList("ExamType", vm.EX_ExamTypeId.ToString());
            vm.StatusOptions = GetExamStatusSelectList(vm.EX_Status);
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
                var nameEntry = item.FirstOrDefault(kvp => kvp.Key.EndsWith("_Name"));
                var displayName = nameEntry.Value?.ToString() ?? id;
                list.Add(new SelectListItem { Value = id, Text = displayName, Selected = id == selectedValue });
            }
            return list;
        }

        private static Exam MapToEntity(ExamFormViewModel m, Guid tenantId, Guid id) => new()
        {
            EX_Id = id,
            EX_TenantId = tenantId,
            EX_AcademicYearId = m.EX_AcademicYearId,
            EX_CourseId = m.EX_CourseId,
            EX_BatchId = m.EX_BatchId,
            EX_ExamTypeId = m.EX_ExamTypeId,
            EX_Name = m.EX_Name,
            EX_Code = m.EX_Code,
            EX_StartDate = m.EX_StartDate,
            EX_EndDate = m.EX_EndDate,
            EX_Status = m.EX_Status
        };

        private static List<SelectListItem> GetExamStatusSelectList(string selected = null)
        {
            var statuses = new[] { "Draft", "Scheduled", "In Progress", "Completed", "Cancelled" };
            return new List<SelectListItem>(
                Array.ConvertAll(statuses, s => new SelectListItem { Value = s, Text = s, Selected = s == selected }));
        }
    }
}
