using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IMS.Models.Entities;

namespace IMS.DAL.Interfaces
{
    public interface IExamDAL
    {
        Task<Exam> GetByIdAsync(Guid id, Guid tenantId);
        Task<(List<Exam> Items, int TotalCount)> GetPagedAsync(
            Guid tenantId, string searchTerm, Guid? courseId, Guid? batchId,
            string status, int pageNumber, int pageSize);
        Task<bool> IsCodeTakenAsync(Guid tenantId, string code, Guid? excludeId);
        Task<Guid> CreateAsync(Exam exam);
        Task<bool> UpdateAsync(Exam exam);
        Task<bool> DeleteAsync(Guid id, Guid tenantId);
    }

    public interface IExamSubjectDAL
    {
        Task<List<ExamSubject>> GetByExamIdAsync(Guid examId);
        Task<bool> SaveSubjectsAsync(Guid examId, List<ExamSubject> subjects);
    }

    public interface IMarkDAL
    {
        Task<List<Mark>> GetByExamIdAsync(Guid examId);
        Task<bool> SaveMarksAsync(Guid examId, List<Mark> marks);
    }

    public interface IResultDAL
    {
        Task<List<Result>> GetByExamIdAsync(Guid examId);
        Task<bool> PublishResultsAsync(Guid examId, List<Result> results);
    }
}
