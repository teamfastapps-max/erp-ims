using IMS.Services;
using IMS.Services.Interfaces;
using IMS.Services.Services;

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

            // Domain business services
            services.AddScoped<IVendorService, VendorService>();
            // Future: services.AddScoped<IInventoryService, InventoryService>();
            // Future: services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();

            return services;
        }
    }
}
