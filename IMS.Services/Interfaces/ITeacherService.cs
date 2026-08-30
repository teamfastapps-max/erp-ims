using IMS.Models.Teacher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Services.Interfaces
{
    public interface ITeacherService
    {
        Task<TeacherIndexViewModel> GetTeacherListAsync(Guid tenantId, string accessToken, string searchTerm, string status, Guid? branchId, int pageNumber, int pageSize);
        Task<TeacherDetailsViewModel> GetTeacherDetailsAsync(Guid id, Guid tenantId, string accessToken);

        Task<TeacherFormViewModel> GetNewTeacherFormAsync(Guid tenantId, string accessToken);
        Task<TeacherFormViewModel> GetTeacherForEditAsync(Guid id, Guid tenantId, string accessToken);

        Task<ServiceResult> CreateTeacherAsync(TeacherFormViewModel model, Guid tenantId, string accessToken);
        Task<ServiceResult> UpdateTeacherAsync(TeacherFormViewModel model, Guid tenantId, string accessToken);
        Task<ServiceResult> DeleteTeacherAsync(Guid id, Guid tenantId, string accessToken);
    }
}
