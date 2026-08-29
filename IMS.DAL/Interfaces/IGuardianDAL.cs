using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IMS.Models.Entities;

namespace IMS.DAL.Interfaces
{
    public interface IGuardianDAL
    {
        Task<List<GuardianSearchResult>> SearchAsync(Guid tenantId, string searchTerm);
        Task<Guardian> GetByIdAsync(Guid id, Guid tenantId);
    }
}
