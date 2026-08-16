using Microsoft.AspNetCore.Authorization;
using IMS.Web.Authorization;

namespace IMS.Web.Extensions
{
    public static class AuthorizationExtensions
    {
        public static IServiceCollection AddPermissionAuthorization(this IServiceCollection services)
        {
            services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
            services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>(); 
            return services;
        }
    }
}
