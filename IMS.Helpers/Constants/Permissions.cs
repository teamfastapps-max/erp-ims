namespace IMS.Helpers.Constants
{
    public class AppPermissionDefinition
    {
        public string Id { get; set; }
        public string FeatureKey { get; set; }
        public string FeatureDisplayName { get; set; }
        public string Description { get; set; }
        public string ServiceName { get; set; }
    }



    public static class Permissions
    {
        // ─── Dashboard ───────────────────────────────────────────────────
        public const string ViewDashboard = "ViewDashboard";

        // ─── Students & Admissions ───────────────────────────────────────
        public const string ViewAdmissionApplication = "ViewAdmissionApplication";
        public const string AddAdmissionApplication = "AddAdmissionApplication";
        public const string UpdateAdmissionApplication = "UpdateAdmissionApplication";
        public const string DeleteAdmissionApplication = "DeleteAdmissionApplication";

        public const string ViewEnrollment = "ViewEnrollment";
        public const string AddEnrollment = "AddEnrollment";
        public const string UpdateEnrollment = "UpdateEnrollment";
        public const string DeleteEnrollment = "DeleteEnrollment";

        public const string ViewStudent = "ViewStudent";
        public const string AddStudent = "AddStudent";
        public const string UpdateStudent = "UpdateStudent";
        public const string DeleteStudent = "DeleteStudent";

        public const string ViewStudentLeave = "ViewStudentLeave";
        public const string ApproveStudentLeave = "ApproveStudentLeave";
        public const string ViewTransferCertificate = "ViewTransferCertificate";

        // ─── Academic Delivery ───────────────────────────────────────────
        public const string ViewBatch = "ViewBatch";
        public const string AddBatch = "AddBatch";
        public const string UpdateBatch = "UpdateBatch";
        public const string DeleteBatch = "DeleteBatch";

        public const string ViewCourseSubject = "ViewCourseSubject";
        public const string AddCourseSubject = "AddCourseSubject";
        public const string UpdateCourseSubject = "UpdateCourseSubject";
        public const string DeleteCourseSubject = "DeleteCourseSubject";

        public const string ViewSyllabus = "ViewSyllabus";
        public const string AddSyllabus = "AddSyllabus";
        public const string EditSyllabus = "EditSyllabus";
        public const string DeleteSyllabus = "DeleteSyllabus";

        public const string ViewTimetable = "ViewTimetable";
        public const string AddTimetable = "AddTimetable";
        public const string UpdateTimetable = "UpdateTimetable";
        public const string DeleteTimetable = "DeleteTimetable";

        public const string ViewHomeTasks = "ViewHomeTasks";
        public const string AddHomeTasks = "AddHomeTasks";
        public const string UpdateHomeTasks = "UpdateHomeTasks";
        public const string DeleteHomeTasks = "DeleteHomeTasks";

        public const string ViewMockTests = "ViewMockTests";
        public const string AddMockTests = "AddMockTests";
        public const string UpdateMockTests = "UpdateMockTests";
        public const string DeleteMockTests = "DeleteMockTests";

        // ─── Attendance Centre ───────────────────────────────────────────
        public const string ViewAttendance = "ViewAttendance";
        public const string ManageAttendance = "ManageAttendance";
        public const string ViewTeacherAttendance = "ViewTeacherAttendance";
        public const string ViewOwnTeacherAttendance = "ViewOwnTeacherAttendance";
        public const string ManageTeacherAttendance = "ManageTeacherAttendance";
        public const string MarkOwnTeacherAttendance = "MarkOwnTeacherAttendance";

        // ─── HR & Faculty ────────────────────────────────────────────────
        public const string ViewTeacher = "ViewTeacher";
        public const string AddTeacher = "AddTeacher";
        public const string UpdateTeacher = "UpdateTeacher";
        public const string DeleteTeacher = "DeleteTeacher";

        public const string ViewStaff = "ViewStaff";
        public const string AddStaff = "AddStaff";
        public const string UpdateStaff = "UpdateStaff";
        public const string DeleteStaff = "DeleteStaff";

        public const string ViewTeacherLeave = "ViewTeacherLeave";
        public const string ApplyTeacherLeave = "ApplyTeacherLeave";
        public const string ApproveTeacherLeave = "ApproveTeacherLeave";

        // ─── Examinations ────────────────────────────────────────────────
        public const string ViewExam = "ViewExam";
        public const string AddExam = "AddExam";
        public const string UpdateExam = "UpdateExam";
        public const string DeleteExam = "DeleteExam";

        // ─── Finance ─────────────────────────────────────────────────────
        public const string ViewFees = "ViewFees";
        public const string AddFees = "AddFees";
        public const string UpdateFees = "UpdateFees";
        public const string DeleteFees = "DeleteFees";

        public const string ViewExpense = "ViewExpense";
        public const string AddExpense = "AddExpense";
        public const string UpdateExpense = "UpdateExpense";
        public const string DeleteExpense = "DeleteExpense";

        // ─── Communication ───────────────────────────────────────────────
        public const string ViewAnnouncement = "ViewAnnouncement";
        public const string AddAnnouncement = "AddAnnouncement";
        public const string EditAnnouncement = "EditAnnouncement";
        public const string DeleteAnnouncement = "DeleteAnnouncement";

        public const string ViewNotificationConfig = "ViewNotificationConfig";
        public const string ManageNotificationConfig = "ManageNotificationConfig";

        // ─── System Setup ────────────────────────────────────────────────
        public const string ViewMaster = "ViewMaster";
        public const string ManageMaster = "ManageMaster";

        public const string ViewDataBackup = "ViewDataBackup";
        public const string ManageDataBackup = "ManageDataBackup";

        public const string ViewUserRoles = "ViewUserRoles";
        public const string ManageUserRoles = "ManageUserRoles";

        public const string ViewSidebarMenu = "ViewSidebarMenu";
        public const string ManageSidebarMenu = "ManageSidebarMenu";

        // ─── Insights ────────────────────────────────────────────────────
        public const string ViewReports = "ViewReports";

        // ─── Feature Key & Display Name ──────────────────────────────────
        private const string Feature = "IMS_FEATURE_MANAGEMENT";
        private const string FeatureDisplay = "IMS Feature Management";

        private const string ServiceName = "erp-ims-service";

        public static readonly List<AppPermissionDefinition> All = new()
        {
            // Dashboard
            new() { Id = ViewDashboard, FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "View dashboard", ServiceName = ServiceName },

            // Students & Admissions
            new() { Id = ViewAdmissionApplication,   FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "View admission applications", ServiceName = ServiceName },
            new() { Id = AddAdmissionApplication,    FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Create admission application", ServiceName = ServiceName },
            new() { Id = UpdateAdmissionApplication, FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Update admission application", ServiceName = ServiceName },
            new() { Id = DeleteAdmissionApplication, FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Delete admission application", ServiceName = ServiceName },

            new() { Id = ViewEnrollment,   FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "View student enrollments", ServiceName = ServiceName },
            new() { Id = AddEnrollment,    FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Enroll a student", ServiceName = ServiceName },
            new() { Id = UpdateEnrollment, FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Update student enrollment", ServiceName = ServiceName },
            new() { Id = DeleteEnrollment, FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Delete student enrollment", ServiceName = ServiceName },

            new() { Id = ViewStudent,   FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "View students",  ServiceName = ServiceName },
            new() { Id = AddStudent,    FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Add a student",  ServiceName = ServiceName },
            new() { Id = UpdateStudent, FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Update a student", ServiceName = ServiceName },
            new() { Id = DeleteStudent, FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Delete a student", ServiceName = ServiceName },

            new() { Id = ViewStudentLeave,        FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "View student leave applications", ServiceName = ServiceName },
            new() { Id = ApproveStudentLeave,     FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Approve or reject student leave", ServiceName = ServiceName },
            new() { Id = ViewTransferCertificate, FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "View transfer certificate applications", ServiceName = ServiceName },

            // Academic Delivery
            new() { Id = ViewBatch,   FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "View batches", ServiceName = ServiceName },
            new() { Id = AddBatch,    FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Add a batch", ServiceName = ServiceName },
            new() { Id = UpdateBatch, FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Update a batch", ServiceName = ServiceName },
            new() { Id = DeleteBatch, FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Delete a batch", ServiceName = ServiceName },

            new() { Id = ViewCourseSubject,   FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "View course subjects", ServiceName = ServiceName },
            new() { Id = AddCourseSubject,    FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Add a course subject", ServiceName = ServiceName },
            new() { Id = UpdateCourseSubject, FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Update a course subject", ServiceName = ServiceName },
            new() { Id = DeleteCourseSubject, FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Delete a course subject", ServiceName = ServiceName },

            new() { Id = ViewSyllabus,   FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "View subject syllabus", ServiceName = ServiceName },
            new() { Id = AddSyllabus,    FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Add a syllabus unit", ServiceName = ServiceName },
            new() { Id = EditSyllabus,   FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Edit a syllabus unit", ServiceName = ServiceName },
            new() { Id = DeleteSyllabus, FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Delete a syllabus unit", ServiceName = ServiceName },

            new() { Id = ViewTimetable,   FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "View timetable schedules", ServiceName = ServiceName },
            new() { Id = AddTimetable,    FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Add a timetable schedule", ServiceName = ServiceName },
            new() { Id = UpdateTimetable, FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Update a timetable schedule", ServiceName = ServiceName },
            new() { Id = DeleteTimetable, FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Delete a timetable schedule", ServiceName = ServiceName },

            new() { Id = ViewHomeTasks,   FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "View home tasks", ServiceName = ServiceName },
            new() { Id = AddHomeTasks,    FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Add a home task", ServiceName = ServiceName },
            new() { Id = UpdateHomeTasks, FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Update a home task", ServiceName = ServiceName },
            new() { Id = DeleteHomeTasks, FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Delete a home task", ServiceName = ServiceName },

            new() { Id = ViewMockTests,   FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "View mock tests", ServiceName = ServiceName },
            new() { Id = AddMockTests,    FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Create a mock test", ServiceName = ServiceName },
            new() { Id = UpdateMockTests, FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Update a mock test", ServiceName = ServiceName },
            new() { Id = DeleteMockTests, FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Delete a mock test", ServiceName = ServiceName },

            // Attendance Centre
            new() { Id = ViewAttendance,           FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "View student attendance", ServiceName = ServiceName },
            new() { Id = ManageAttendance,         FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Mark and manage student attendance", ServiceName = ServiceName },
            new() { Id = ViewTeacherAttendance,    FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "View teacher attendance", ServiceName = ServiceName },
            new() { Id = ViewOwnTeacherAttendance, FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "View own attendance", ServiceName = ServiceName },
            new() { Id = ManageTeacherAttendance,  FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Mark and manage teacher attendance", ServiceName = ServiceName },
            new() { Id = MarkOwnTeacherAttendance, FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Self-mark today's attendance", ServiceName = ServiceName },

            // HR & Faculty
            new() { Id = ViewTeacher,        FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "View teachers",  ServiceName = ServiceName },
            new() { Id = AddTeacher,         FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Add a teacher",  ServiceName = ServiceName },
            new() { Id = UpdateTeacher,      FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Update a teacher", ServiceName = ServiceName },
            new() { Id = DeleteTeacher,      FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Delete a teacher", ServiceName = ServiceName },
            new() { Id = ViewTeacherLeave,   FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "View teacher leaves", ServiceName = ServiceName },
            new() { Id = ApplyTeacherLeave,  FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Apply for own leave", ServiceName = ServiceName },
            new() { Id = ApproveTeacherLeave,FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Approve or reject teacher leave", ServiceName = ServiceName },

            new() { Id = ViewStaff,     FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "View staff",  ServiceName = ServiceName },
            new() { Id = AddStaff,      FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Add a staff member",  ServiceName = ServiceName },
            new() { Id = UpdateStaff,   FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Update a staff member", ServiceName = ServiceName },
            new() { Id = DeleteStaff,   FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Delete a staff member", ServiceName = ServiceName },

            // Examinations
            new() { Id = ViewExam,   FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "View examinations", ServiceName = ServiceName },
            new() { Id = AddExam,    FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Schedule examinations", ServiceName = ServiceName },
            new() { Id = UpdateExam, FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Update examinations", ServiceName = ServiceName },
            new() { Id = DeleteExam, FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Delete examinations", ServiceName = ServiceName },

            // Finance
            new() { Id = ViewFees,     FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "View fees and payments", ServiceName = ServiceName },
            new() { Id = AddFees,      FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Collect and record fees", ServiceName = ServiceName },
            new() { Id = UpdateFees,   FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Update fees and payment records", ServiceName = ServiceName },
            new() { Id = DeleteFees,   FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Delete fees and payment records", ServiceName = ServiceName },

            new() { Id = ViewExpense,   FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "View expenses", ServiceName = ServiceName },
            new() { Id = AddExpense,    FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Record an expense", ServiceName = ServiceName },
            new() { Id = UpdateExpense, FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Update an expense", ServiceName = ServiceName },
            new() { Id = DeleteExpense, FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Delete an expense", ServiceName = ServiceName },

            // Communication
            new() { Id = ViewAnnouncement,   FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "View announcements", ServiceName = ServiceName },
            new() { Id = AddAnnouncement,    FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Add an announcement", ServiceName = ServiceName },
            new() { Id = EditAnnouncement,   FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Edit an announcement", ServiceName = ServiceName },
            new() { Id = DeleteAnnouncement, FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Delete an announcement", ServiceName = ServiceName },

            new() { Id = ViewNotificationConfig,   FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "View notification hub configuration", ServiceName = ServiceName },
            new() { Id = ManageNotificationConfig, FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Manage notification hub configuration", ServiceName = ServiceName },

            // System Setup
            new() { Id = ViewMaster,   FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "View master management", ServiceName = ServiceName },
            new() { Id = ManageMaster, FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Manage master data items", ServiceName = ServiceName },

            new() { Id = ViewDataBackup,   FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "View data backup and exports", ServiceName = ServiceName },
            new() { Id = ManageDataBackup, FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Perform data backup and export", ServiceName = ServiceName },

            new() { Id = ViewUserRoles,   FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "View users and roles", ServiceName = ServiceName },
            new() { Id = ManageUserRoles, FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Manage users and roles", ServiceName = ServiceName },

            new() { Id = ViewSidebarMenu,   FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "View sidebar menu configuration", ServiceName = ServiceName },
            new() { Id = ManageSidebarMenu, FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "Add, edit, reorder, and toggle sidebar menu items", ServiceName = ServiceName },

            // Insights / Reports
            new() { Id = ViewReports, FeatureKey = Feature, FeatureDisplayName = FeatureDisplay, Description = "View reports and analytics", ServiceName = ServiceName },
        };
    }
}
