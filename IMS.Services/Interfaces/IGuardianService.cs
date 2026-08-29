using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IMS.Models.ViewModels;

namespace IMS.Services.Interfaces
{
    public interface IGuardianService
    {
        Task<List<GuardianSearchResultViewModel>> SearchAsync(Guid tenantId, string searchTerm);
    }
}
