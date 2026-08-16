using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using IMS.Models.Session;
using IMS.Services.Interfaces;

namespace IMS.Services
{
    public class UserSessionService : IUserSessionService
    {
        private readonly IRedisService _redis;
        private readonly IUserApiService _userApi;
        private readonly ILogger<UserSessionService> _logger;

        public UserSessionService(IRedisService redis, IUserApiService userApi, ILogger<UserSessionService> logger)
        {
            _redis = redis;
            _userApi = userApi;
            _logger = logger;
        }

        public async Task StoreUserSessionAsync(ClaimsPrincipal principal, string accessToken)
        {
            var identity = principal.Identity as ClaimsIdentity;
            var username = identity?.Name ?? "unknown";

            if (username == "unknown")
            {
                _logger.LogWarning("Cannot store session — username claim missing");
                return;
            }

            var profile = await _userApi.GetMyProfileAsync(accessToken);

            if (profile == null)
            {
                _logger.LogWarning("Profile fetch failed for user {Username} — storing minimal fallback session", username);
                await _redis.SetUserAsync(username, new UserSessionModel { PreferredUsername = username });
                return;
            }

            // Claims used for authorization decisions
            if (!string.IsNullOrEmpty(profile.UserType))
                identity?.AddClaim(new Claim("roles", profile.UserType));

            if (!string.IsNullOrEmpty(profile.CustomRoleId))
                identity?.AddClaim(new Claim("custom_role_id", profile.CustomRoleId));

            if (!string.IsNullOrEmpty(profile.TenantId))
                identity?.AddClaim(new Claim("tenant_id", profile.TenantId));

            // ★ Populate Redis permission cache directly from my-profile response — no extra API call
            if (!string.IsNullOrEmpty(profile.CustomRoleId) && profile.CustomRolePermissions != null && profile.CustomRolePermissions.Count > 0 && !string.Equals(profile.UserType, "TENANT_ADMIN", StringComparison.OrdinalIgnoreCase))
            {
                await _redis.SetPermissionSetAsync(profile.CustomRoleId,profile.CustomRolePermissions,TimeSpan.FromHours(1));
            }

            var sessionData = new UserSessionModel
            {
                Email = profile.Email,
                FirstName = profile.FirstName,
                LastName = profile.LastName,
                TenantId = profile.TenantId,
                TenantName = profile.TenantDetails?.Name,
                UserType = profile.UserType,
                CustomRoleId = profile.CustomRoleId,
                CustomRoleName = profile.CustomRoleName,
                Permissions = string.Join(",", profile.CustomRolePermissions ?? new List<string>()),
                KeycloakId = profile.KeycloakId,
                UserId = profile.Id,
                PreferredUsername = username
            };

            await _redis.SetUserAsync(username, sessionData);
        }

        public async Task RemoveUserSessionAsync(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                _logger.LogWarning("Logout called with empty username — skipping Redis cleanup");
                return;
            }

            await _redis.RemoveUserAsync(username);
           
        }
    }
}
