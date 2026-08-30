using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IMS.Models.Entities;

namespace IMS.DAL.Interfaces
{
    public interface ICourseSubjectDAL
    {
        Task<List<CourseSubject>> GetAllAsync(Guid tenantId);
        Task<List<CourseSubject>> GetByCourseIdAsync(Guid courseId, Guid tenantId);
        Task<CourseSubject> GetByIdAsync(Guid courseId, Guid subjectId, Guid tenantId);
        Task<bool> ExistsAsync(Guid courseId, Guid subjectId, Guid tenantId);
        Task<bool> CreateAsync(CourseSubject cs);
        Task<bool> UpdateAsync(CourseSubject cs, Guid tenantId);
        Task<bool> DeleteAsync(Guid courseId, Guid subjectId, Guid tenantId);
    }
}
