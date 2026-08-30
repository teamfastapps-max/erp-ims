using System;
using System.Threading.Tasks;
using IMS.Models.ViewModels;

namespace IMS.Services.Interfaces
{
    public interface IExamService
    {
        Task<ExamIndexViewModel> GetExamListAsync(
            Guid tenantId, string searchTerm, Guid? courseId, Guid? batchId,
            string status, int pageNumber, int pageSize);

        Task<ExamDetailsViewModel> GetExamDetailsAsync(Guid id, Guid tenantId);

        Task<ExamFormViewModel> GetExamForEditAsync(Guid id, Guid tenantId);

        Task<ServiceResult> CreateExamAsync(ExamFormViewModel model, Guid tenantId);

        Task<ServiceResult> UpdateExamAsync(ExamFormViewModel model, Guid tenantId);

        Task<ServiceResult> DeleteExamAsync(Guid id, Guid tenantId);

        Task<MarksEntryViewModel> GetMarksEntryAsync(Guid examId, Guid tenantId);

        Task<ServiceResult> SaveMarksAsync(MarksEntryViewModel model, Guid tenantId);

        void PopulateDropdowns(ExamFormViewModel vm);
    }
}
