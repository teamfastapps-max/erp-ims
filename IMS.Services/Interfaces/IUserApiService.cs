using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Models.Auth;
using IMS.Models.TenantUser;

namespace IMS.Services.Interfaces
{
    public interface IUserApiService
    {
        Task<UserProfileModel> GetMyProfileAsync(string accessToken);
        Task<PagedResult<TenantUserModel>> GetTenantUsersAsync(int page, int limit, string accessToken);
        Task<TenantUserModel> GetTenantUserByIdAsync(string id, string accessToken);
        Task<TenantUserModel> CreateTenantUserAsync(CreateTenantUserRequest request, string accessToken);
        Task<TenantUserModel> UpdateTenantUserAsync(string id, UpdateTenantUserRequest request, string accessToken);
        Task<bool> DeleteTenantUserAsync(string id, string accessToken);
    }
}
