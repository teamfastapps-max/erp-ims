using IMS.DAL.Common;
using IMS.DAL.Interfaces;
using IMS.DAL.Repositories;

namespace IMS.Web.Extensions
{
    public static class DALServiceExtensions
    {
        public static IServiceCollection AddIMSDataAccess(this IServiceCollection services)
        {
            services.AddScoped<DBHelper>();

            services.AddScoped<IVendorDAL, VendorDAL>();
            // Future: services.AddScoped<IInventoryDAL, InventoryDAL>();
            // Future: services.AddScoped<IPurchaseOrderDAL, PurchaseOrderDAL>();

            return services;
        }
    }
}
