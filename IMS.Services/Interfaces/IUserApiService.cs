using IMS.Models.Auth;
using IMS.Models.Common;
using IMS.Models.TenantUser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Services.Interfaces
{
    public interface IUserApiService
    {
        Task<UserProfileModel> GetMyProfileAsync(string accessToken);
        Task<PagedResult<TenantUserModel>> GetTenantUsersAsync(int page, int limit, string accessToken);
        Task<TenantUserModel> GetTenantUserByIdAsync(string id, string accessToken);
        Task<ApiResult<TenantUserModel>> CreateTenantUserAsync(CreateTenantUserRequest request, string accessToken);
        Task<ApiResult<TenantUserModel>> UpdateTenantUserAsync(string id, UpdateTenantUserRequest request, string accessToken);
        Task<bool> DeleteTenantUserAsync(string id, string accessToken);
        Task<ApiResult<UserProfileModel>> UpdateMyProfileAsync(string userId, UpdateMyProfileRequest request, string accessToken);
    }
}
