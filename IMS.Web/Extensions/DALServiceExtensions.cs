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

            services.AddScoped<IMasterDAL, MasterDAL>();
            services.AddScoped<IDropdownDAL, DropdownDAL>();
            services.AddScoped<IStudentDAL, StudentDAL>();
            services.AddScoped<IGuardianDAL, GuardianDAL>();
            services.AddScoped<IBatchDAL, BatchDAL>();
            services.AddScoped<ICourseSubjectDAL, CourseSubjectDAL>();
            services.AddScoped<IEnrollmentDAL, EnrollmentDAL>();
            services.AddScoped<ITimetableDAL, TimetableDAL>();
            services.AddScoped<IAdmissionApplicationDAL, AdmissionApplicationDAL>();

            services.AddScoped<IExpenseDAL, ExpenseDAL>();
            services.AddScoped<IExpenseCategoryDAL, ExpenseCategoryDAL>();
            services.AddScoped<IStaffDAL, StaffDAL>();
            services.AddScoped<IAttendanceSessionDAL, AttendanceSessionDAL>();
            services.AddScoped<IAttendanceRecordDAL, AttendanceRecordDAL>();
            services.AddScoped<IExamDAL, ExamDAL>();
            services.AddScoped<IExamSubjectDAL, ExamSubjectDAL>();
            services.AddScoped<IMarkDAL, MarkDAL>();
            services.AddScoped<IResultDAL, ResultDAL>();
            services.AddScoped<IFeeStructureDAL, FeeStructureDAL>();
            services.AddScoped<IFeeStructureItemDAL, FeeStructureItemDAL>();
            services.AddScoped<IFeeInvoiceDAL, FeeInvoiceDAL>();
            services.AddScoped<IPaymentDAL, PaymentDAL>();

            return services;
        }
    }
}
