using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Threading.Tasks;
using IMS.DAL.Common;
using IMS.DAL.Interfaces;
using IMS.Models.Portal;

namespace IMS.DAL
{
    public class PortalDAL : IPortalDAL
    {
        private readonly DBHelper _dbHelper;

        public PortalDAL(DBHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task<PortalAuthResult> AuthenticateByEmailAsync(string email)
        {
            var result = new PortalAuthResult { Email = email };
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_Portal_Authenticate", conn);
            cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 255).Value = email.Trim();

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            // Result set 1: User info
            if (await reader.ReadAsync())
            {
                result.IsAuthenticated = true;
                result.UserType = reader["UserType"]?.ToString() ?? "STUDENT";
                result.UserId = (Guid)reader["UserId"];
                result.ActiveStudentId = reader["StudentId"] != DBNull.Value ? (Guid)reader["StudentId"] : Guid.Empty;
                result.TenantId = (Guid)reader["TenantId"];
                result.BranchId = reader["BranchId"] != DBNull.Value ? (Guid?)reader["BranchId"] : null;
                result.FullName = reader["FullName"]?.ToString() ?? string.Empty;
                result.Email = reader["Email"]?.ToString() ?? email;
                result.StoredPassword = reader["StoredPassword"]?.ToString();
                result.StudentCode = reader["StudentCode"]?.ToString();
                result.AdmissionNumber = reader["AdmissionNumber"]?.ToString();
                result.BranchName = reader["BranchName"]?.ToString();
            }
            else
            {
                result.IsAuthenticated = false;
                result.ErrorMessage = "No account found matching this email address.";
                return result;
            }

            // Result set 2: Linked students (wards)
            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    result.LinkedStudents.Add(new LinkedStudentDto
                    {
                        StudentId = (Guid)reader["StudentId"],
                        StudentName = reader["StudentName"]?.ToString() ?? string.Empty,
                        StudentCode = reader["StudentCode"]?.ToString(),
                        AdmissionNumber = reader["AdmissionNumber"]?.ToString(),
                        CourseName = reader["CourseName"]?.ToString(),
                        BatchName = reader["BatchName"]?.ToString(),
                        BranchId = reader["BranchId"] != DBNull.Value ? (Guid?)reader["BranchId"] : null
                    });
                }
            }

            if (result.ActiveStudentId == Guid.Empty && result.LinkedStudents.Count > 0)
            {
                result.ActiveStudentId = result.LinkedStudents[0].StudentId;
            }

            return result;
        }

        public async Task<PortalDashboardViewModel> GetDashboardAsync(Guid studentId, Guid tenantId)
        {
            var vm = new PortalDashboardViewModel();
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_Portal_GetDashboard", conn);
            cmd.Parameters.Add("@StudentId", SqlDbType.UniqueIdentifier).Value = studentId;
            cmd.Parameters.Add("@TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            // Result Set 1: Student Header
            if (await reader.ReadAsync())
            {
                vm.Student = new StudentHeaderDto
                {
                    StudentId = (Guid)reader["StudentId"],
                    StudentName = reader["StudentName"]?.ToString() ?? string.Empty,
                    StudentCode = reader["StudentCode"]?.ToString(),
                    AdmissionNumber = reader["AdmissionNumber"]?.ToString(),
                    Gender = reader["Gender"]?.ToString(),
                    BloodGroup = reader["BloodGroup"]?.ToString(),
                    BranchName = reader["BranchName"]?.ToString(),
                    BatchId = reader["BatchId"] != DBNull.Value ? (Guid?)reader["BatchId"] : null,
                    BatchName = reader["BatchName"]?.ToString(),
                    CourseName = reader["CourseName"]?.ToString(),
                    AcademicYearName = reader["AcademicYearName"]?.ToString()
                };
            }

            // Result Set 2: Attendance Metrics
            if (await reader.NextResultAsync() && await reader.ReadAsync())
            {
                vm.Attendance = new AttendanceMetricDto
                {
                    TotalSessions = Convert.ToInt32(reader["TotalSessions"]),
                    PresentSessions = Convert.ToInt32(reader["PresentSessions"]),
                    AbsentSessions = Convert.ToInt32(reader["AbsentSessions"]),
                    HalfDaySessions = Convert.ToInt32(reader["HalfDaySessions"]),
                    AttendancePercentage = Convert.ToDecimal(reader["AttendancePercentage"])
                };
            }

            // Result Set 3: Finance
            if (await reader.NextResultAsync() && await reader.ReadAsync())
            {
                vm.Finance = new FinancialSummaryDto
                {
                    TotalInvoices = Convert.ToInt32(reader["TotalInvoices"]),
                    TotalBilled = Convert.ToDecimal(reader["TotalBilled"]),
                    TotalPaid = Convert.ToDecimal(reader["TotalPaid"]),
                    OutstandingBalance = Convert.ToDecimal(reader["OutstandingBalance"]),
                    PendingInvoicesCount = Convert.ToInt32(reader["PendingInvoicesCount"])
                };
            }

            // Result Set 4: Today's Schedule
            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    vm.TodaySchedule.Add(new TimetableSlotDto
                    {
                        TimetableId = (Guid)reader["TimetableId"],
                        DayOfWeek = Convert.ToInt16(reader["DayOfWeek"]),
                        StartTime = (TimeSpan)reader["StartTime"],
                        EndTime = (TimeSpan)reader["EndTime"],
                        SubjectName = reader["SubjectName"]?.ToString() ?? string.Empty,
                        SubjectCode = reader["SubjectCode"]?.ToString(),
                        TeacherName = reader["TeacherName"]?.ToString(),
                        ClassroomName = reader["ClassroomName"]?.ToString()
                    });
                }
            }

            // Result Set 5: Notices
            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    vm.RecentNotices.Add(new PortalNoticeDto
                    {
                        NoticeId = (Guid)reader["NoticeId"],
                        Title = reader["Title"]?.ToString() ?? string.Empty,
                        Content = reader["Content"]?.ToString() ?? string.Empty,
                        PublishedAt = reader["PublishedAt"] != DBNull.Value ? (DateTime?)reader["PublishedAt"] : null
                    });
                }
            }

            // Result Set 6: Active Tasks
            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    vm.ActiveHomeTasks.Add(new HomeTaskDto
                    {
                        TaskId = (Guid)reader["TaskId"],
                        Title = reader["Title"]?.ToString() ?? string.Empty,
                        SubjectName = reader["SubjectName"]?.ToString() ?? string.Empty,
                        DueDate = Convert.ToDateTime(reader["DueDate"]),
                        SubmissionStatus = reader["SubmissionStatus"]?.ToString() ?? "Pending"
                    });
                }
            }

            return vm;
        }

        public async Task<StudentProfileViewModel> GetStudentProfileAsync(Guid studentId, Guid tenantId)
        {
            var vm = new StudentProfileViewModel();
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_Portal_GetStudentProfile", conn);
            cmd.Parameters.Add("@StudentId", SqlDbType.UniqueIdentifier).Value = studentId;
            cmd.Parameters.Add("@TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                vm.Student = new StudentHeaderDto
                {
                    StudentId = (Guid)reader["S_Id"],
                    StudentName = $"{reader["S_FirstName"]} {reader["S_LastName"]}".Trim(),
                    StudentCode = reader["S_StudentCode"]?.ToString(),
                    AdmissionNumber = reader["S_AdmissionNumber"]?.ToString(),
                    Gender = reader["S_Gender"]?.ToString(),
                    BloodGroup = reader["S_BloodGroup"]?.ToString(),
                    BranchName = reader["BranchName"]?.ToString(),
                    BatchName = reader["BatchName"]?.ToString(),
                    CourseName = reader["CourseName"]?.ToString(),
                    AcademicYearName = reader["AcademicYearName"]?.ToString()
                };
                vm.DateOfBirth = reader["S_DateOfBirth"] != DBNull.Value ? Convert.ToDateTime(reader["S_DateOfBirth"]).ToString("dd MMM yyyy") : null;
                vm.Email = reader["S_Email"]?.ToString();
                vm.Phone = reader["S_Phone"]?.ToString();
                vm.AddressLine1 = reader["S_AddressLine1"]?.ToString();
                vm.AddressLine2 = reader["S_AddressLine2"]?.ToString();
                vm.City = reader["S_City"]?.ToString();
                vm.State = reader["S_State"]?.ToString();
                vm.PostalCode = reader["S_PostalCode"]?.ToString();
                vm.Country = reader["S_Country"]?.ToString();
                vm.AdmissionDate = reader["S_AdmissionDate"] != DBNull.Value ? Convert.ToDateTime(reader["S_AdmissionDate"]).ToString("dd MMM yyyy") : null;
                vm.Status = reader["S_Status"]?.ToString();
            }

            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    vm.Guardians.Add(new GuardianInfoDto
                    {
                        GuardianId = (Guid)reader["GuardianId"],
                        GuardianName = reader["GuardianName"]?.ToString() ?? string.Empty,
                        Phone = reader["Phone"]?.ToString() ?? string.Empty,
                        Email = reader["Email"]?.ToString(),
                        Occupation = reader["Occupation"]?.ToString(),
                        Relationship = reader["Relationship"]?.ToString() ?? string.Empty,
                        IsPrimary = Convert.ToBoolean(reader["IsPrimary"])
                    });
                }
            }

            return vm;
        }

        public async Task<StudentIdCardViewModel> GetStudentIdCardDataAsync(Guid studentId, Guid tenantId)
        {
            var vm = new StudentIdCardViewModel();
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_Portal_GetIdCardData", conn);
            cmd.Parameters.Add("@StudentId", SqlDbType.UniqueIdentifier).Value = studentId;
            cmd.Parameters.Add("@TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                vm.StudentId = (Guid)reader["StudentId"];
                vm.StudentName = reader["StudentName"]?.ToString() ?? string.Empty;
                vm.StudentCode = reader["StudentCode"]?.ToString();
                vm.AdmissionNumber = reader["AdmissionNumber"]?.ToString();
                vm.DateOfBirth = reader["DateOfBirth"] != DBNull.Value ? Convert.ToDateTime(reader["DateOfBirth"]).ToString("dd/MM/yyyy") : null;
                vm.BloodGroup = reader["BloodGroup"]?.ToString();
                vm.StudentPhone = reader["StudentPhone"]?.ToString();
                vm.StudentEmail = reader["StudentEmail"]?.ToString();
                vm.Address = reader["Address"]?.ToString();
                vm.City = reader["City"]?.ToString();
                vm.CourseName = reader["CourseName"]?.ToString();
                vm.BatchName = reader["BatchName"]?.ToString();
                vm.AcademicYearName = reader["AcademicYearName"]?.ToString();
                vm.BranchName = reader["BranchName"]?.ToString();
                vm.BranchPhone = reader["BranchPhone"]?.ToString();
                vm.BranchEmail = reader["BranchEmail"]?.ToString();
                vm.BranchAddress = reader["BranchAddress"]?.ToString();
                vm.OrganizationName = reader["OrganizationName"]?.ToString();
                vm.OrganizationLogoUrl = reader["OrganizationLogoUrl"]?.ToString();
                vm.EmergencyContact = reader["EmergencyContact"]?.ToString();
            }

            return vm;
        }

        public async Task<GuardianIdCardViewModel> GetGuardianIdCardDataAsync(Guid guardianId, Guid tenantId)
        {
            var vm = new GuardianIdCardViewModel();
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_Portal_GetGuardianIdCardData", conn);
            cmd.Parameters.Add("@GuardianId", SqlDbType.UniqueIdentifier).Value = guardianId;
            cmd.Parameters.Add("@TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                vm.GuardianId = (Guid)reader["GuardianId"];
                vm.GuardianName = reader["GuardianName"]?.ToString() ?? string.Empty;
                vm.Phone = reader["Phone"]?.ToString() ?? string.Empty;
                vm.Email = reader["Email"]?.ToString();
                vm.Occupation = reader["Occupation"]?.ToString();
                vm.OrganizationName = reader["OrganizationName"]?.ToString();
                vm.OrganizationLogoUrl = reader["OrganizationLogoUrl"]?.ToString();
            }

            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    vm.Wards.Add(new LinkedStudentDto
                    {
                        StudentId = (Guid)reader["StudentId"],
                        StudentName = reader["StudentName"]?.ToString() ?? string.Empty,
                        StudentCode = reader["StudentCode"]?.ToString(),
                        AdmissionNumber = reader["AdmissionNumber"]?.ToString(),
                        CourseName = reader["CourseName"]?.ToString(),
                        BatchName = reader["BatchName"]?.ToString()
                    });
                }
            }

            return vm;
        }

        public async Task<PortalAttendanceViewModel> GetAttendanceCalendarAsync(Guid studentId, Guid tenantId, int month, int year)
        {
            var vm = new PortalAttendanceViewModel
            {
                Month = month,
                Year = year,
                MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month)
            };

            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_Portal_GetAttendanceCalendar", conn);
            cmd.Parameters.Add("@StudentId", SqlDbType.UniqueIdentifier).Value = studentId;
            cmd.Parameters.Add("@TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;
            cmd.Parameters.Add("@Month", SqlDbType.Int).Value = month;
            cmd.Parameters.Add("@Year", SqlDbType.Int).Value = year;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            // Daily Records
            while (await reader.ReadAsync())
            {
                vm.Records.Add(new AttendanceRecordDto
                {
                    AttendanceDate = Convert.ToDateTime(reader["AttendanceDate"]),
                    Status = reader["Status"]?.ToString() ?? string.Empty,
                    Remarks = reader["Remarks"]?.ToString(),
                    SubjectName = reader["SubjectName"]?.ToString(),
                    StartTime = reader["StartTime"] != DBNull.Value ? (TimeSpan?)reader["StartTime"] : null,
                    EndTime = reader["EndTime"] != DBNull.Value ? (TimeSpan?)reader["EndTime"] : null
                });
            }

            // Month Summary Stats
            if (await reader.NextResultAsync() && await reader.ReadAsync())
            {
                vm.TotalDaysInMonth = Convert.ToInt32(reader["TotalDaysInMonth"]);
                vm.PresentCount = Convert.ToInt32(reader["PresentCount"]);
                vm.AbsentCount = Convert.ToInt32(reader["AbsentCount"]);
                vm.HalfDayCount = Convert.ToInt32(reader["HalfDayCount"]);
                vm.LateCount = Convert.ToInt32(reader["LateCount"]);
                vm.ExcusedCount = Convert.ToInt32(reader["ExcusedCount"]);
                if (vm.TotalDaysInMonth > 0)
                {
                    vm.Percentage = Math.Round(((vm.PresentCount + (vm.HalfDayCount * 0.5m)) * 100m) / vm.TotalDaysInMonth, 2);
                }
            }

            return vm;
        }

        public async Task<PortalTimetableViewModel> GetTimetableAsync(Guid studentId, Guid tenantId)
        {
            var vm = new PortalTimetableViewModel();
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_Portal_GetTimetable", conn);
            cmd.Parameters.Add("@StudentId", SqlDbType.UniqueIdentifier).Value = studentId;
            cmd.Parameters.Add("@TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                vm.Slots.Add(new TimetableSlotDto
                {
                    TimetableId = (Guid)reader["TimetableId"],
                    DayOfWeek = Convert.ToInt16(reader["DayOfWeek"]),
                    StartTime = (TimeSpan)reader["StartTime"],
                    EndTime = (TimeSpan)reader["EndTime"],
                    SubjectName = reader["SubjectName"]?.ToString() ?? string.Empty,
                    SubjectCode = reader["SubjectCode"]?.ToString(),
                    TeacherName = reader["TeacherName"]?.ToString(),
                    ClassroomName = reader["ClassroomName"]?.ToString(),
                    ClassroomLocation = reader["ClassroomLocation"]?.ToString()
                });
            }

            return vm;
        }

        public async Task<PortalClassDetailsViewModel> GetClassDetailsAsync(Guid studentId, Guid tenantId)
        {
            var vm = new PortalClassDetailsViewModel();
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_Portal_GetClassDetails", conn);
            cmd.Parameters.Add("@StudentId", SqlDbType.UniqueIdentifier).Value = studentId;
            cmd.Parameters.Add("@TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                vm.BatchName = reader["BatchName"]?.ToString() ?? string.Empty;
                vm.BatchCode = reader["BatchCode"]?.ToString();
                vm.StartDate = Convert.ToDateTime(reader["StartDate"]);
                vm.EndDate = reader["EndDate"] != DBNull.Value ? (DateTime?)reader["EndDate"] : null;
                vm.CourseName = reader["CourseName"]?.ToString() ?? string.Empty;
                vm.CourseCode = reader["CourseCode"]?.ToString();
                vm.CourseDescription = reader["CourseDescription"]?.ToString();
                vm.AcademicYearName = reader["AcademicYearName"]?.ToString() ?? string.Empty;
                vm.BranchName = reader["BranchName"]?.ToString();
            }

            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    vm.Subjects.Add(new EnrolledSubjectDto
                    {
                        SubjectId = (Guid)reader["SubjectId"],
                        SubjectName = reader["SubjectName"]?.ToString() ?? string.Empty,
                        SubjectCode = reader["SubjectCode"]?.ToString(),
                        Credits = reader["Credits"] != DBNull.Value ? (decimal?)reader["Credits"] : null,
                        MaxMarks = reader["MaxMarks"] != DBNull.Value ? (decimal?)reader["MaxMarks"] : null,
                        PassMarks = reader["PassMarks"] != DBNull.Value ? (decimal?)reader["PassMarks"] : null,
                        IsMandatory = Convert.ToBoolean(reader["IsMandatory"])
                    });
                }
            }

            return vm;
        }

        public async Task<PortalHomeTaskViewModel> GetHomeTasksAsync(Guid studentId, Guid tenantId)
        {
            var vm = new PortalHomeTaskViewModel();
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_Portal_GetHomeTasks", conn);
            cmd.Parameters.Add("@StudentId", SqlDbType.UniqueIdentifier).Value = studentId;
            cmd.Parameters.Add("@TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                vm.Tasks.Add(new HomeTaskDto
                {
                    TaskId = (Guid)reader["TaskId"],
                    Title = reader["Title"]?.ToString() ?? string.Empty,
                    Description = reader["Description"]?.ToString() ?? string.Empty,
                    AssignedDate = Convert.ToDateTime(reader["AssignedDate"]),
                    DueDate = Convert.ToDateTime(reader["DueDate"]),
                    TeacherAttachmentUrl = reader["TeacherAttachmentUrl"]?.ToString(),
                    MaxMarks = reader["MaxMarks"] != DBNull.Value ? (decimal?)reader["MaxMarks"] : null,
                    SubjectName = reader["SubjectName"]?.ToString() ?? string.Empty,
                    SubjectCode = reader["SubjectCode"]?.ToString(),
                    SubmissionId = reader["SubmissionId"] != DBNull.Value ? (Guid?)reader["SubmissionId"] : null,
                    SubmissionDate = reader["SubmissionDate"] != DBNull.Value ? (DateTime?)reader["SubmissionDate"] : null,
                    SubmissionContent = reader["SubmissionContent"]?.ToString(),
                    StudentAttachmentUrl = reader["StudentAttachmentUrl"]?.ToString(),
                    MarksObtained = reader["MarksObtained"] != DBNull.Value ? (decimal?)reader["MarksObtained"] : null,
                    TeacherRemarks = reader["TeacherRemarks"]?.ToString(),
                    SubmissionStatus = reader["SubmissionStatus"]?.ToString() ?? "Pending",
                    IsSubmitted = Convert.ToInt32(reader["IsSubmitted"]) == 1,
                    IsOverdue = Convert.ToInt32(reader["IsOverdue"]) == 1
                });
            }

            return vm;
        }

        public async Task<bool> SubmitHomeTaskAsync(Guid taskId, Guid studentId, string? content, string? attachmentUrl)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_Portal_SubmitHomeTask", conn);
            cmd.Parameters.Add("@HomeTaskId", SqlDbType.UniqueIdentifier).Value = taskId;
            cmd.Parameters.Add("@StudentId", SqlDbType.UniqueIdentifier).Value = studentId;
            cmd.Parameters.Add("@Content", SqlDbType.NVarChar, -1).Value = (object?)content ?? DBNull.Value;
            cmd.Parameters.Add("@AttachmentUrl", SqlDbType.NVarChar, 500).Value = (object?)attachmentUrl ?? DBNull.Value;

            await conn.OpenAsync();
            var res = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(res) == 1;
        }

        public async Task<PortalSyllabusViewModel> GetSyllabusAsync(Guid studentId, Guid tenantId)
        {
            var vm = new PortalSyllabusViewModel();
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_Portal_GetSyllabus", conn);
            cmd.Parameters.Add("@StudentId", SqlDbType.UniqueIdentifier).Value = studentId;
            cmd.Parameters.Add("@TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                vm.Units.Add(new SyllabusUnitDto
                {
                    SyllabusId = (Guid)reader["SyllabusId"],
                    UnitNumber = Convert.ToInt32(reader["UnitNumber"]),
                    UnitTitle = reader["UnitTitle"]?.ToString() ?? string.Empty,
                    Description = reader["Description"]?.ToString(),
                    TotalHours = reader["TotalHours"] != DBNull.Value ? (int?)reader["TotalHours"] : null,
                    FileUrl = reader["FileUrl"]?.ToString(),
                    IsCompleted = Convert.ToBoolean(reader["IsCompleted"]),
                    SubjectId = (Guid)reader["SubjectId"],
                    SubjectName = reader["SubjectName"]?.ToString() ?? string.Empty,
                    SubjectCode = reader["SubjectCode"]?.ToString()
                });
            }

            return vm;
        }

        public async Task<PortalMockTestViewModel> GetMockTestsAsync(Guid studentId, Guid tenantId)
        {
            var vm = new PortalMockTestViewModel();
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_Portal_GetMockTests", conn);
            cmd.Parameters.Add("@StudentId", SqlDbType.UniqueIdentifier).Value = studentId;
            cmd.Parameters.Add("@TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                vm.Tests.Add(new MockTestDto
                {
                    TestId = (Guid)reader["TestId"],
                    Title = reader["Title"]?.ToString() ?? string.Empty,
                    Description = reader["Description"]?.ToString(),
                    TestDate = Convert.ToDateTime(reader["TestDate"]),
                    DurationMinutes = Convert.ToInt32(reader["DurationMinutes"]),
                    TotalMarks = Convert.ToDecimal(reader["TotalMarks"]),
                    PassMarks = Convert.ToDecimal(reader["PassMarks"]),
                    TestStatus = reader["TestStatus"]?.ToString() ?? string.Empty,
                    SubjectName = reader["SubjectName"]?.ToString() ?? string.Empty,
                    Score = reader["Score"] != DBNull.Value ? (decimal?)reader["Score"] : null,
                    Percentage = reader["Percentage"] != DBNull.Value ? (decimal?)reader["Percentage"] : null,
                    Grade = reader["Grade"]?.ToString(),
                    StudentResultStatus = reader["StudentResultStatus"]?.ToString(),
                    CompletedAt = reader["CompletedAt"] != DBNull.Value ? (DateTime?)reader["CompletedAt"] : null
                });
            }

            return vm;
        }

        public async Task<PortalAdmitCardViewModel> GetAdmitCardAsync(Guid studentId, Guid tenantId)
        {
            var vm = new PortalAdmitCardViewModel();
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_Portal_GetAdmitCard", conn);
            cmd.Parameters.Add("@StudentId", SqlDbType.UniqueIdentifier).Value = studentId;
            cmd.Parameters.Add("@TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                vm.HasAdmitCard = true;
                vm.ExamName = reader["ExamName"]?.ToString();
                vm.ExamCode = reader["ExamCode"]?.ToString();
                vm.StartDate = Convert.ToDateTime(reader["StartDate"]);
                vm.EndDate = Convert.ToDateTime(reader["EndDate"]);
                vm.StudentName = reader["StudentName"]?.ToString() ?? string.Empty;
                vm.StudentCode = reader["StudentCode"]?.ToString();
                vm.RollNumber = reader["RollNumber"]?.ToString();
                vm.CourseName = reader["CourseName"]?.ToString();
                vm.BatchName = reader["BatchName"]?.ToString();
                vm.CenterName = reader["CenterName"]?.ToString();
                vm.CenterAddress = reader["CenterAddress"]?.ToString();
                vm.OrganizationName = reader["OrganizationName"]?.ToString();
                vm.OrganizationLogoUrl = reader["OrganizationLogoUrl"]?.ToString();
            }

            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    vm.Schedules.Add(new AdmitCardScheduleDto
                    {
                        ExamSubjectId = (Guid)reader["ExamSubjectId"],
                        SubjectName = reader["SubjectName"]?.ToString() ?? string.Empty,
                        SubjectCode = reader["SubjectCode"]?.ToString(),
                        MaxMarks = Convert.ToDecimal(reader["MaxMarks"]),
                        PassMarks = Convert.ToDecimal(reader["PassMarks"]),
                        ExamDate = Convert.ToDateTime(reader["ExamDate"]),
                        StartTime = (TimeSpan)reader["StartTime"],
                        EndTime = (TimeSpan)reader["EndTime"],
                        RoomNumber = reader["RoomNumber"]?.ToString()
                    });
                }
            }

            return vm;
        }

        public async Task<PortalMarkSheetViewModel> GetMarkSheetAsync(Guid studentId, Guid tenantId)
        {
            var vm = new PortalMarkSheetViewModel();
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_Portal_GetMarkSheet", conn);
            cmd.Parameters.Add("@StudentId", SqlDbType.UniqueIdentifier).Value = studentId;
            cmd.Parameters.Add("@TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                vm.ExamResults.Add(new ExamResultDto
                {
                    ResultId = (Guid)reader["ResultId"],
                    ExamId = (Guid)reader["ExamId"],
                    ExamName = reader["ExamName"]?.ToString() ?? string.Empty,
                    ExamCode = reader["ExamCode"]?.ToString(),
                    TotalMarks = Convert.ToDecimal(reader["TotalMarks"]),
                    MarksObtained = Convert.ToDecimal(reader["MarksObtained"]),
                    Percentage = Convert.ToDecimal(reader["Percentage"]),
                    OverallGrade = reader["OverallGrade"]?.ToString(),
                    ResultStatus = reader["ResultStatus"]?.ToString() ?? string.Empty,
                    Remarks = reader["Remarks"]?.ToString(),
                    PublishedAt = reader["PublishedAt"] != DBNull.Value ? (DateTime?)reader["PublishedAt"] : null
                });
            }

            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    vm.SubjectMarks.Add(new SubjectMarkDto
                    {
                        MarkId = (Guid)reader["MarkId"],
                        ExamId = (Guid)reader["ExamId"],
                        SubjectName = reader["SubjectName"]?.ToString() ?? string.Empty,
                        SubjectCode = reader["SubjectCode"]?.ToString(),
                        MaxMarks = Convert.ToDecimal(reader["MaxMarks"]),
                        PassMarks = Convert.ToDecimal(reader["PassMarks"]),
                        MarksObtained = Convert.ToDecimal(reader["MarksObtained"]),
                        Percentage = reader["Percentage"] != DBNull.Value ? (decimal?)reader["Percentage"] : null,
                        Grade = reader["Grade"]?.ToString(),
                        Remarks = reader["Remarks"]?.ToString()
                    });
                }
            }

            return vm;
        }

        public async Task<PortalFeeTransactionViewModel> GetFeeTransactionsAsync(Guid studentId, Guid tenantId)
        {
            var vm = new PortalFeeTransactionViewModel();
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_Portal_GetFeeTransactions", conn);
            cmd.Parameters.Add("@StudentId", SqlDbType.UniqueIdentifier).Value = studentId;
            cmd.Parameters.Add("@TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            // Invoices
            while (await reader.ReadAsync())
            {
                var inv = new FeeInvoiceDto
                {
                    InvoiceId = (Guid)reader["InvoiceId"],
                    InvoiceNumber = reader["InvoiceNumber"]?.ToString() ?? string.Empty,
                    InvoiceDate = Convert.ToDateTime(reader["InvoiceDate"]),
                    DueDate = Convert.ToDateTime(reader["DueDate"]),
                    Subtotal = Convert.ToDecimal(reader["Subtotal"]),
                    DiscountAmount = Convert.ToDecimal(reader["DiscountAmount"]),
                    TaxAmount = Convert.ToDecimal(reader["TaxAmount"]),
                    TotalAmount = Convert.ToDecimal(reader["TotalAmount"]),
                    PaidAmount = Convert.ToDecimal(reader["PaidAmount"]),
                    BalanceAmount = Convert.ToDecimal(reader["BalanceAmount"]),
                    Status = reader["Status"]?.ToString() ?? string.Empty,
                    Notes = reader["Notes"]?.ToString()
                };
                vm.TotalBilled += inv.TotalAmount;
                vm.TotalPaid += inv.PaidAmount;
                vm.OutstandingBalance += inv.BalanceAmount;
                vm.Invoices.Add(inv);
            }

            // Payments
            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    vm.Payments.Add(new FeePaymentDto
                    {
                        PaymentId = (Guid)reader["PaymentId"],
                        PaymentNumber = reader["PaymentNumber"]?.ToString() ?? string.Empty,
                        PaymentDate = Convert.ToDateTime(reader["PaymentDate"]),
                        Amount = Convert.ToDecimal(reader["Amount"]),
                        Status = reader["Status"]?.ToString() ?? string.Empty,
                        TransactionReference = reader["TransactionReference"]?.ToString(),
                        PaymentMethodName = reader["PaymentMethodName"]?.ToString(),
                        Notes = reader["Notes"]?.ToString()
                    });
                }
            }

            return vm;
        }

        public async Task<PortalReceiptViewModel?> GetReceiptDetailsAsync(Guid paymentId, Guid tenantId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_Portal_GetReceiptDetails", conn);
            cmd.Parameters.Add("@PaymentId", SqlDbType.UniqueIdentifier).Value = paymentId;
            cmd.Parameters.Add("@TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync()) return null;

            var vm = new PortalReceiptViewModel
            {
                PaymentId = (Guid)reader["PaymentId"],
                ReceiptNumber = reader["ReceiptNumber"]?.ToString() ?? string.Empty,
                PaymentDate = Convert.ToDateTime(reader["PaymentDate"]),
                PaidAmount = Convert.ToDecimal(reader["PaidAmount"]),
                TransactionRef = reader["TransactionRef"]?.ToString(),
                PaymentMode = reader["PaymentMode"]?.ToString(),
                StudentName = reader["StudentName"]?.ToString() ?? string.Empty,
                StudentCode = reader["StudentCode"]?.ToString(),
                AdmissionNumber = reader["AdmissionNumber"]?.ToString(),
                CourseName = reader["CourseName"]?.ToString(),
                BatchName = reader["BatchName"]?.ToString(),
                BranchName = reader["BranchName"]?.ToString(),
                BranchAddress = reader["BranchAddress"]?.ToString(),
                BranchPhone = reader["BranchPhone"]?.ToString(),
                OrganizationName = reader["OrganizationName"]?.ToString(),
                OrganizationLogoUrl = reader["OrganizationLogoUrl"]?.ToString()
            };

            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    vm.AllocatedInvoices.Add(new ReceiptAllocationDto
                    {
                        InvoiceNumber = reader["InvoiceNumber"]?.ToString() ?? string.Empty,
                        AmountPaid = Convert.ToDecimal(reader["AmountPaid"])
                    });
                }
            }

            return vm;
        }

        public async Task<bool> ApplyLeaveAsync(Guid leaveId, Guid tenantId, Guid studentId, DateTime fromDate, DateTime toDate, string leaveType, string reason, string appliedBy)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_Portal_ApplyLeave", conn);
            cmd.Parameters.Add("@SL_Id", SqlDbType.UniqueIdentifier).Value = leaveId;
            cmd.Parameters.Add("@SL_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;
            cmd.Parameters.Add("@SL_StudentId", SqlDbType.UniqueIdentifier).Value = studentId;
            cmd.Parameters.Add("@SL_FromDate", SqlDbType.Date).Value = fromDate;
            cmd.Parameters.Add("@SL_ToDate", SqlDbType.Date).Value = toDate;
            cmd.Parameters.Add("@SL_LeaveType", SqlDbType.NVarChar, 30).Value = leaveType;
            cmd.Parameters.Add("@SL_Reason", SqlDbType.NVarChar, 500).Value = reason;
            cmd.Parameters.Add("@SL_AppliedBy", SqlDbType.NVarChar, 20).Value = appliedBy;

            await conn.OpenAsync();
            var res = await cmd.ExecuteScalarAsync();
            return res != null;
        }

        public async Task<List<StudentLeaveDto>> GetLeavesAsync(Guid studentId, Guid tenantId)
        {
            var list = new List<StudentLeaveDto>();
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_Portal_GetLeaves", conn);
            cmd.Parameters.Add("@StudentId", SqlDbType.UniqueIdentifier).Value = studentId;
            cmd.Parameters.Add("@TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new StudentLeaveDto
                {
                    LeaveId = (Guid)reader["LeaveId"],
                    FromDate = Convert.ToDateTime(reader["FromDate"]),
                    ToDate = Convert.ToDateTime(reader["ToDate"]),
                    TotalDays = Convert.ToInt32(reader["TotalDays"]),
                    LeaveType = reader["LeaveType"]?.ToString() ?? string.Empty,
                    Reason = reader["Reason"]?.ToString() ?? string.Empty,
                    Status = reader["Status"]?.ToString() ?? string.Empty,
                    AppliedBy = reader["AppliedBy"]?.ToString() ?? string.Empty,
                    RejectionReason = reader["RejectionReason"]?.ToString(),
                    AppliedAt = Convert.ToDateTime(reader["AppliedAt"])
                });
            }

            return list;
        }

        public async Task<PortalTransportViewModel> GetTransportDetailsAsync(Guid studentId, Guid tenantId)
        {
            var vm = new PortalTransportViewModel();
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_Portal_GetTransportDetails", conn);
            cmd.Parameters.Add("@StudentId", SqlDbType.UniqueIdentifier).Value = studentId;
            cmd.Parameters.Add("@TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                vm.IsAllocated = true;
                vm.StopName = reader["StopName"]?.ToString();
                vm.PickupTime = reader["PickupTime"] != DBNull.Value ? (TimeSpan?)reader["PickupTime"] : null;
                vm.DropTime = reader["DropTime"] != DBNull.Value ? (TimeSpan?)reader["DropTime"] : null;
                vm.RouteName = reader["RouteName"]?.ToString();
                vm.RouteCode = reader["RouteCode"]?.ToString();
                vm.VehicleNumber = reader["VehicleNumber"]?.ToString();
                vm.DriverName = reader["DriverName"]?.ToString();
                vm.DriverPhone = reader["DriverPhone"]?.ToString();
                vm.HelperName = reader["HelperName"]?.ToString();
                vm.HelperPhone = reader["HelperPhone"]?.ToString();
                vm.StartLocation = reader["StartLocation"]?.ToString();
                vm.EndLocation = reader["EndLocation"]?.ToString();
                vm.RouteStartTime = reader["RouteStartTime"] != DBNull.Value ? (TimeSpan?)reader["RouteStartTime"] : null;
                vm.RouteEndTime = reader["RouteEndTime"] != DBNull.Value ? (TimeSpan?)reader["RouteEndTime"] : null;
            }

            return vm;
        }

        public async Task<bool> ApplyTCAsync(Guid tcId, Guid tenantId, Guid studentId, string reason, DateTime expectedLeavingDate)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_Portal_ApplyTC", conn);
            cmd.Parameters.Add("@TC_Id", SqlDbType.UniqueIdentifier).Value = tcId;
            cmd.Parameters.Add("@TC_TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;
            cmd.Parameters.Add("@TC_StudentId", SqlDbType.UniqueIdentifier).Value = studentId;
            cmd.Parameters.Add("@TC_Reason", SqlDbType.NVarChar, 500).Value = reason;
            cmd.Parameters.Add("@TC_ExpectedLeavingDate", SqlDbType.Date).Value = expectedLeavingDate;

            await conn.OpenAsync();
            var res = await cmd.ExecuteScalarAsync();
            return res != null;
        }

        public async Task<List<TransferCertificateDto>> GetTCStatusAsync(Guid studentId, Guid tenantId)
        {
            var list = new List<TransferCertificateDto>();
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_Portal_GetTCStatus", conn);
            cmd.Parameters.Add("@StudentId", SqlDbType.UniqueIdentifier).Value = studentId;
            cmd.Parameters.Add("@TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new TransferCertificateDto
                {
                    TCId = (Guid)reader["TCId"],
                    ApplicationNumber = reader["ApplicationNumber"]?.ToString() ?? string.Empty,
                    ApplicationDate = Convert.ToDateTime(reader["ApplicationDate"]),
                    Reason = reader["Reason"]?.ToString() ?? string.Empty,
                    ExpectedLeavingDate = Convert.ToDateTime(reader["ExpectedLeavingDate"]),
                    LibraryClearance = Convert.ToBoolean(reader["LibraryClearance"]),
                    FeeClearance = Convert.ToBoolean(reader["FeeClearance"]),
                    LabClearance = Convert.ToBoolean(reader["LabClearance"]),
                    Status = reader["Status"]?.ToString() ?? string.Empty,
                    CertificateNumber = reader["CertificateNumber"]?.ToString(),
                    IssuedDate = reader["IssuedDate"] != DBNull.Value ? (DateTime?)reader["IssuedDate"] : null,
                    Remarks = reader["Remarks"]?.ToString()
                });
            }

            return list;
        }

        public async Task<List<PortalNoticeDto>> GetNoticesAsync(Guid tenantId)
        {
            var list = new List<PortalNoticeDto>();
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_Portal_GetNotices", conn);
            cmd.Parameters.Add("@TenantId", SqlDbType.UniqueIdentifier).Value = tenantId;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new PortalNoticeDto
                {
                    NoticeId = (Guid)reader["NoticeId"],
                    Title = reader["Title"]?.ToString() ?? string.Empty,
                    Content = reader["Content"]?.ToString() ?? string.Empty,
                    PublishedAt = reader["PublishedAt"] != DBNull.Value ? (DateTime?)reader["PublishedAt"] : null,
                    ExpiresAt = reader["ExpiresAt"] != DBNull.Value ? (DateTime?)reader["ExpiresAt"] : null
                });
            }

            return list;
        }

        public async Task<bool> UpdatePasswordAsync(Guid userId, string userType, string newPassword)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_Portal_UpdatePassword", conn);
            cmd.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = userId;
            cmd.Parameters.Add("@UserType", SqlDbType.NVarChar, 20).Value = userType;
            cmd.Parameters.Add("@Password", SqlDbType.NVarChar, 255).Value = newPassword;

            await conn.OpenAsync();
            var rows = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(rows) > 0;
        }

        public async Task<(bool Success, string Message, string? Token, string? FullName, string? UserType)> GeneratePasswordResetTokenAsync(string email, string token, string? otp, int expiryMinutes)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_Portal_ForgotPassword_GenerateToken", conn);
            cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 255).Value = email;
            cmd.Parameters.Add("@Token", SqlDbType.NVarChar, 200).Value = token;
            cmd.Parameters.Add("@OtpCode", SqlDbType.NVarChar, 10).Value = (object?)otp ?? DBNull.Value;
            cmd.Parameters.Add("@ExpiryMinutes", SqlDbType.Int).Value = expiryMinutes;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var success = Convert.ToInt32(reader["Success"]) == 1;
                var message = reader["Message"]?.ToString() ?? string.Empty;
                var retToken = reader["Token"] != DBNull.Value ? reader["Token"]?.ToString() : null;
                var fullName = reader["FullName"] != DBNull.Value ? reader["FullName"]?.ToString() : null;
                var userType = reader["UserType"] != DBNull.Value ? reader["UserType"]?.ToString() : null;
                return (success, message, retToken, fullName, userType);
            }
            return (false, "Unable to generate password reset request.", null, null, null);
        }

        public async Task<(bool Success, string Message)> ResetPasswordWithTokenAsync(string token, string newPasswordHash)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_Portal_ForgotPassword_ResetWithToken", conn);
            cmd.Parameters.Add("@Token", SqlDbType.NVarChar, 200).Value = token;
            cmd.Parameters.Add("@NewPasswordHash", SqlDbType.NVarChar, 255).Value = newPasswordHash;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var success = Convert.ToInt32(reader["Success"]) == 1;
                var message = reader["Message"]?.ToString() ?? string.Empty;
                return (success, message);
            }
            return (false, "An error occurred while resetting your password.");
        }

        public async Task<bool> SetUserPasswordAsync(Guid userId, string userType, string passwordHash)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = _dbHelper.CreateCommand("SP_Portal_Admin_SetPassword", conn);
            cmd.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = userId;
            cmd.Parameters.Add("@UserType", SqlDbType.NVarChar, 20).Value = userType;
            cmd.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 255).Value = passwordHash;

            await conn.OpenAsync();
            var rows = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(rows) > 0;
        }
    }
}
