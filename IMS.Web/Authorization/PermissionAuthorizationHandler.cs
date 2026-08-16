using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using IMS.Helpers.Constants;
using IMS.Services.Interfaces;

namespace IMS.Web.Authorization
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private static readonly TimeSpan PermissionTtl = TimeSpan.FromHours(1);

        private readonly IRedisService _redis;
        private readonly IRoleApiService _roleApi;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<PermissionAuthorizationHandler> _logger;

        public PermissionAuthorizationHandler(IRedisService redis,IRoleApiService roleApi,IHttpContextAccessor httpContextAccessor,ILogger<PermissionAuthorizationHandler> logger)
        {
            _redis = redis;
            _roleApi = roleApi;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,PermissionRequirement requirement)
        {
            // Admin bypass — no permission lookup needed
            if (context.User.IsInRole(AppRoles.TenantAdmin) || context.User.IsInRole("SYSTEM_ADMIN"))
            {
                context.Succeed(requirement);
                return;
            }

            var roleId = context.User.FindFirst("custom_role_id")?.Value;
            if (string.IsNullOrEmpty(roleId))
            {
                _logger.LogWarning("No custom_role_id claim for user {User}", context.User.Identity?.Name);
                return; // fails — no role, no access
            }

            // Fast path — Redis (populated at login, or by a previous fallback)
            var cachedPermissions = await _redis.GetPermissionSetAsync(roleId);
            if (cachedPermissions.Count > 0)
            {
                if (cachedPermissions.Contains(requirement.Permission))
                    context.Succeed(requirement);
                return;
            }

            // Redis MISS — fallback to Role API
            _logger.LogInformation("[PERMISSION] Redis MISS for role: {RoleId}. Falling back to API...", roleId);

            var httpContext = _httpContextAccessor.HttpContext;
            var accessToken = await httpContext.GetTokenAsync("access_token");

            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogWarning("No access token available for permission fallback");
                return;
            }

            var permissions = await _roleApi.GetRolePermissionsAsync(roleId, accessToken);

            if (permissions.Count > 0)
            {
                await _redis.SetPermissionSetAsync(roleId, permissions, PermissionTtl);

                if (permissions.Contains(requirement.Permission))
                    context.Succeed(requirement);
            }
        }
    }
}
