using IMS.Models.Teacher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.DAL.Interfaces
{
    public interface ITeacherDAL
    {
        Task<Teacher> GetByIdAsync(Guid id, Guid tenantId);
        Task<List<Teacher>> GetProfilesByIdsAsync(List<Guid> ids, Guid tenantId);

        Task<bool> IsEmployeeCodeTakenAsync(Guid tenantId, string employeeCode, Guid? excludeId);
        Task<bool> AddEditTeacherProfileAsync(Teacher teacher);
        Task<bool> SoftDeleteAsync(Guid id, Guid tenantId);
    }
}
