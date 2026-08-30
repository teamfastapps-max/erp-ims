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

            services.AddScoped<IMasterService, MasterService>();
            services.AddScoped<IDropdownService, DropdownService>();
            services.AddScoped<IStudentService, StudentService>();
            services.AddScoped<IGuardianService, GuardianService>();
            services.AddScoped<IBatchService, BatchService>();
            services.AddScoped<ICourseSubjectService, CourseSubjectService>();
            services.AddScoped<IEnrollmentService, EnrollmentService>();
            services.AddScoped<ITimetableService, TimetableService>();
            services.AddScoped<IAdmissionApplicationService, AdmissionApplicationService>();
            services.AddScoped<ITeacherService, TeacherService>();
            services.AddScoped<IExpenseService, ExpenseService>();
            services.AddScoped<IStaffService, StaffService>();
            services.AddScoped<IAttendanceService, AttendanceService>();
            services.AddScoped<IExamService, ExamService>();
            services.AddScoped<IFeeService, FeeService>();

            return services;
        }
    }
}
