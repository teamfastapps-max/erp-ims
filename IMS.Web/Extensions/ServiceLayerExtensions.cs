using IMS.Services;
using IMS.Services.Interfaces;

namespace IMS.Web.Extensions
{
    public static class ServiceLayerCollectionExtensions
    {
        public static IServiceCollection AddIMSBusinessServices(this IServiceCollection services)
        {
            services.AddScoped<IRedisService, RedisService>();
            services.AddScoped<IUserApiService, UserApiService>();
            services.AddScoped<IUserSessionService, UserSessionService>();
            services.AddScoped<IRoleApiService, RoleApiService>();
            services.AddScoped<IPermissionSyncService, PermissionSyncService>();

            services.AddScoped<IStudentService, StudentService>();
            services.AddScoped<IGuardianService, GuardianService>();


            return services;
        }
    }
}
