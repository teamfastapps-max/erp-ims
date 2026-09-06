using Microsoft.AspNetCore.Http.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public const string ViewStudent = "ViewStudent";      
        public const string AddStudent = "AddStudent";
        public const string UpdateStudent = "UpdateStudent";
        public const string DeleteStudent = "DeleteStudent";

        public const string ViewStudentLeave = "ViewStudentLeave";
        public const string ApproveStudentLeave = "ApproveStudentLeave";
        public const string ViewTransferCertificate = "ViewTransferCertificate";


        public const string ViewTeacher = "ViewTeacher";     
        public const string AddTeacher = "AddTeacher";
        public const string UpdateTeacher = "UpdateTeacher";
        public const string DeleteTeacher = "DeleteTeacher";

        public const string ViewStaff = "ViewStaff";
        public const string AddStaff = "AddStaff";
        public const string UpdateStaff = "UpdateStaff";
        public const string DeleteStaff = "DeleteStaff";

        public const string ApplyTeacherLeave = "ApplyTeacherLeave";      
        public const string ApproveTeacherLeave = "ApproveTeacherLeave";  

        public const string ViewOwnTeacherAttendance = "ViewOwnTeacherAttendance";
        public const string ManageTeacherAttendance = "ManageTeacherAttendance";
        public const string MarkOwnTeacherAttendance = "MarkOwnTeacherAttendance";


        private const string StudentFeature = "STUDENT_MANAGEMENT";
        private const string StudentFeatureDisplay = "Student Management";

        private const string TeacherFeature = "TEACHER_MANAGEMENT";
        private const string TeacherFeatureDisplay = "Teacher Management";

        private const string StaffFeature = "STAFF_MANAGEMENT";
        private const string StaffFeatureDisplay = "Staff Management";

        private const string ServiceName = "erp-ims-service";

        public static readonly List<AppPermissionDefinition> All = new()
        {
            new() { Id = ViewStudent,   FeatureKey = StudentFeature, FeatureDisplayName = StudentFeatureDisplay, Description = "View students",  ServiceName = ServiceName },
            new() { Id = AddStudent,    FeatureKey = StudentFeature, FeatureDisplayName = StudentFeatureDisplay, Description = "Add a student",  ServiceName = ServiceName },
            new() { Id = UpdateStudent, FeatureKey = StudentFeature, FeatureDisplayName = StudentFeatureDisplay, Description = "Update a student", ServiceName = ServiceName },
            new() { Id = DeleteStudent, FeatureKey = StudentFeature, FeatureDisplayName = StudentFeatureDisplay, Description = "Delete a student", ServiceName = ServiceName },

            new() { Id = ViewStudentLeave,        FeatureKey = StudentFeature, FeatureDisplayName = StudentFeatureDisplay, Description = "View student leave applications", ServiceName = ServiceName },
            new() { Id = ApproveStudentLeave,     FeatureKey = StudentFeature, FeatureDisplayName = StudentFeatureDisplay, Description = "Approve or reject student leave", ServiceName = ServiceName },
            new() { Id = ViewTransferCertificate, FeatureKey = StudentFeature, FeatureDisplayName = StudentFeatureDisplay, Description = "View transfer certificate applications", ServiceName = ServiceName },


            new() { Id = ViewTeacher,   FeatureKey = TeacherFeature, FeatureDisplayName = TeacherFeatureDisplay, Description = "View teachers",  ServiceName = ServiceName },
            new() { Id = AddTeacher,    FeatureKey = TeacherFeature, FeatureDisplayName = TeacherFeatureDisplay, Description = "Add a teacher",  ServiceName = ServiceName },
            new() { Id = UpdateTeacher, FeatureKey = TeacherFeature, FeatureDisplayName = TeacherFeatureDisplay, Description = "Update a teacher", ServiceName = ServiceName },
            new() { Id = DeleteTeacher, FeatureKey = TeacherFeature, FeatureDisplayName = TeacherFeatureDisplay, Description = "Delete a teacher", ServiceName = ServiceName },
            
            new() { Id = ViewStaff,     FeatureKey = StaffFeature, FeatureDisplayName = StaffFeatureDisplay, Description = "View staff",  ServiceName = ServiceName },
            new() { Id = AddStaff,      FeatureKey = StaffFeature, FeatureDisplayName = StaffFeatureDisplay, Description = "Add a staff member",  ServiceName = ServiceName },
            new() { Id = UpdateStaff,   FeatureKey = StaffFeature, FeatureDisplayName = StaffFeatureDisplay, Description = "Update a staff member", ServiceName = ServiceName },
            new() { Id = DeleteStaff,   FeatureKey = StaffFeature, FeatureDisplayName = StaffFeatureDisplay, Description = "Delete a staff member", ServiceName = ServiceName },


            new() { Id = ApplyTeacherLeave,        FeatureKey = TeacherFeature, FeatureDisplayName = TeacherFeatureDisplay, Description = "Apply for own leave", ServiceName = ServiceName },
            new() { Id = ApproveTeacherLeave,      FeatureKey = TeacherFeature, FeatureDisplayName = TeacherFeatureDisplay, Description = "Approve or reject teacher leave", ServiceName = ServiceName },
            new() { Id = ViewOwnTeacherAttendance, FeatureKey = TeacherFeature, FeatureDisplayName = TeacherFeatureDisplay, Description = "View own attendance", ServiceName = ServiceName },
            new() { Id = ManageTeacherAttendance,  FeatureKey = TeacherFeature, FeatureDisplayName = TeacherFeatureDisplay, Description = "Mark and manage teacher attendance", ServiceName = ServiceName },
            new() { Id = MarkOwnTeacherAttendance, FeatureKey = TeacherFeature, FeatureDisplayName = TeacherFeatureDisplay, Description = "Self-mark today's attendance", ServiceName = ServiceName },

        };

    }
}
