using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IMS.DAL.Interfaces;
using IMS.Models.ViewModels;
using IMS.Services.Interfaces;

namespace IMS.Services
{
    public class GuardianService : IGuardianService
    {
        private readonly IGuardianDAL _repo;

        public GuardianService(IGuardianDAL repo)
        {
            _repo = repo;
        }

        public async Task<List<GuardianSearchResultViewModel>> SearchAsync(Guid tenantId, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Trim().Length < 2)
                return new List<GuardianSearchResultViewModel>();

            var results = await _repo.SearchAsync(tenantId, searchTerm.Trim());

            return results.Select(g => new GuardianSearchResultViewModel
            {
                G_Id = g.G_Id,
                FullName = string.Join(" ", new[] { g.G_FirstName, g.G_LastName }.Where(p => !string.IsNullOrWhiteSpace(p))),
                Phone = g.G_Phone,
                Email = g.G_Email,
                Occupation = g.G_Occupation
            }).ToList();
        }
    }
}
