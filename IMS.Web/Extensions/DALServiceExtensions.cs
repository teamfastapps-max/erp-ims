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

            services.AddScoped<IStudentDAL, StudentDAL>();
            services.AddScoped<IGuardianDAL, GuardianDAL>();

            return services;
        }
    }
}
