-- ============================================================================
-- ERP-IMS: Student & Guardian Portal Stored Procedures
-- Script Date: 05-09-2026
-- ============================================================================
USE [IMS];
GO

-- ============================================================================
-- 1. SP_Portal_Authenticate
-- Authenticates a user by email across Students_S and Guardians_G
-- Returns User Details in Result Set 1, and Linked Students in Result Set 2
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.SP_Portal_Authenticate
    @Email NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @UserId UNIQUEIDENTIFIER = NULL;
    DECLARE @UserType NVARCHAR(20) = NULL;

    -- Step 1: Check Students_S
    SELECT TOP 1
        @UserId = s.S_Id,
        @UserType = 'STUDENT'
    FROM dbo.Students_S s
    WHERE s.S_Email = @Email
      AND s.S_DeletedAt IS NULL
      AND (ISNULL(s.S_IsActive, 1) = 1 OR s.S_Status = 'Active');

    IF @UserId IS NOT NULL
    BEGIN
        -- Result Set 1: User details
        SELECT
            'STUDENT' AS UserType,
            s.S_Id AS UserId,
            s.S_Id AS StudentId,
            s.S_TenantId AS TenantId,
            s.S_BranchId AS BranchId,
            s.S_FirstName + ' ' + ISNULL(s.S_LastName, '') AS FullName,
            s.S_Email AS Email,
            s.S_Password AS StoredPassword,
            s.S_StudentCode AS StudentCode,
            s.S_AdmissionNumber AS AdmissionNumber,
            ISNULL(b.B_Name, 'Main Campus') AS BranchName
        FROM dbo.Students_S s
        LEFT JOIN dbo.Branches_B b ON s.S_BranchId = b.B_Id
        WHERE s.S_Id = @UserId;

        -- Result Set 2: Self as linked student
        SELECT
            s.S_Id AS StudentId,
            s.S_FirstName + ' ' + ISNULL(s.S_LastName, '') AS StudentName,
            s.S_StudentCode AS StudentCode,
            s.S_AdmissionNumber AS AdmissionNumber,
            COALESCE(c.C_Name, 'Academic Course') AS CourseName,
            COALESCE(bt.BT_Name, 'General Batch') AS BatchName,
            s.S_BranchId AS BranchId
        FROM dbo.Students_S s
        LEFT JOIN dbo.Enrollments_E e ON s.S_Id = e.E_StudentId AND e.E_Status = 'Active'
        LEFT JOIN dbo.BatchStudents_BS bs ON s.S_Id = bs.BS_StudentId AND bs.BS_LeftAt IS NULL
        LEFT JOIN dbo.Batches_BT bt ON COALESCE(e.E_BatchId, bs.BS_BatchId) = bt.BT_Id
        LEFT JOIN dbo.Courses_C c ON COALESCE(e.E_CourseId, bt.BT_CourseId) = c.C_Id
        WHERE s.S_Id = @UserId;

        RETURN;
    END

    -- Step 2: Check Guardians_G
    SELECT TOP 1
        @UserId = g.G_Id,
        @UserType = 'GUARDIAN'
    FROM dbo.Guardians_G g
    WHERE g.G_Email = @Email
      AND ISNULL(g.G_IsActive, 1) = 1;

    IF @UserId IS NOT NULL
    BEGIN
        -- Result Set 1: User details
        SELECT
            'GUARDIAN' AS UserType,
            g.G_Id AS UserId,
            -- Pick first linked student as primary initial context
            COALESCE(
                (SELECT TOP 1 sg.SG_StudentId FROM dbo.Students_Guardians sg WHERE sg.SG_GuardianId = g.G_Id),
                (SELECT TOP 1 sg2.SG_StudentId FROM dbo.StudentGuardians_SG sg2 WHERE sg2.SG_GuardianId = g.G_Id)
            ) AS StudentId,
            g.G_TenantId AS TenantId,
            (SELECT TOP 1 s.S_BranchId FROM dbo.Students_Guardians sg INNER JOIN dbo.Students_S s ON sg.SG_StudentId = s.S_Id WHERE sg.SG_GuardianId = g.G_Id) AS BranchId,
            g.G_FirstName + ' ' + ISNULL(g.G_LastName, '') AS FullName,
            g.G_Email AS Email,
            g.G_Password AS StoredPassword,
            NULL AS StudentCode,
            NULL AS AdmissionNumber,
            NULL AS BranchName
        FROM dbo.Guardians_G g
        WHERE g.G_Id = @UserId;

        -- Result Set 2: All connected wards (students)
        SELECT
            s.S_Id AS StudentId,
            s.S_FirstName + ' ' + ISNULL(s.S_LastName, '') AS StudentName,
            s.S_StudentCode AS StudentCode,
            s.S_AdmissionNumber AS AdmissionNumber,
            COALESCE(c.C_Name, 'Academic Course') AS CourseName,
            COALESCE(bt.BT_Name, 'General Batch') AS BatchName,
            s.S_BranchId AS BranchId
        FROM (
            SELECT SG_StudentId, SG_GuardianId, SG_IsPrimary FROM dbo.Students_Guardians WHERE SG_GuardianId = @UserId
            UNION
            SELECT SG_StudentId, SG_GuardianId, SG_IsPrimary FROM dbo.StudentGuardians_SG WHERE SG_GuardianId = @UserId
        ) w
        INNER JOIN dbo.Students_S s ON w.SG_StudentId = s.S_Id AND s.S_DeletedAt IS NULL
        LEFT JOIN dbo.Enrollments_E e ON s.S_Id = e.E_StudentId AND e.E_Status = 'Active'
        LEFT JOIN dbo.BatchStudents_BS bs ON s.S_Id = bs.BS_StudentId AND bs.BS_LeftAt IS NULL
        LEFT JOIN dbo.Batches_BT bt ON COALESCE(e.E_BatchId, bs.BS_BatchId) = bt.BT_Id
        LEFT JOIN dbo.Courses_C c ON COALESCE(e.E_CourseId, bt.BT_CourseId) = c.C_Id
        ORDER BY w.SG_IsPrimary DESC, s.S_FirstName;

        RETURN;
    END

    -- If no user found, return empty results
    SELECT NULL AS UserId WHERE 1 = 0;
    SELECT NULL AS StudentId WHERE 1 = 0;
END
GO

-- ============================================================================
-- 2. SP_Portal_GetDashboard
-- Returns comprehensive 360-degree dashboard stats for a student
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.SP_Portal_GetDashboard
    @StudentId  UNIQUEIDENTIFIER,
    @TenantId   UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    -- Result Set 1: Student Brief & Header Info
    SELECT
        s.S_Id AS StudentId,
        s.S_FirstName + ' ' + ISNULL(s.S_LastName, '') AS StudentName,
        s.S_StudentCode AS StudentCode,
        s.S_AdmissionNumber AS AdmissionNumber,
        s.S_Gender AS Gender,
        ISNULL(s.S_BloodGroup, 'N/A') AS BloodGroup,
        ISNULL(b.B_Name, 'Main Campus') AS BranchName,
        ISNULL(o.O_Name, 'INSTITUTE OF EDUCATION') AS OrganizationName,
        COALESCE(e.E_BatchId, bs.BS_BatchId) AS BatchId,
        COALESCE(c.C_Name, 'Academic Course') AS CourseName,
        COALESCE(bt.BT_Name, 'General Batch') AS BatchName,
        ay.AY_Name AS AcademicYearName
    FROM dbo.Students_S s
    LEFT JOIN dbo.Branches_B b ON s.S_BranchId = b.B_Id
    LEFT JOIN dbo.Organizations_O o ON s.S_TenantId = o.O_Id
    LEFT JOIN dbo.Enrollments_E e ON s.S_Id = e.E_StudentId AND e.E_Status = 'Active'
    LEFT JOIN dbo.BatchStudents_BS bs ON s.S_Id = bs.BS_StudentId AND bs.BS_LeftAt IS NULL
    LEFT JOIN dbo.Batches_BT bt ON COALESCE(e.E_BatchId, bs.BS_BatchId) = bt.BT_Id
    LEFT JOIN dbo.Courses_C c ON COALESCE(e.E_CourseId, bt.BT_CourseId) = c.C_Id
    LEFT JOIN dbo.AcademicYears_AY ay ON bt.BT_AcademicYearId = ay.AY_Id
    WHERE s.S_Id = @StudentId AND s.S_TenantId = @TenantId;

    -- Result Set 2: Current Month Attendance Snapshot
    DECLARE @StartOfMonth DATE = DATEFROMPARTS(YEAR(GETUTCDATE()), MONTH(GETUTCDATE()), 1);
    DECLARE @EndOfMonth DATE = EOMONTH(GETUTCDATE());

    SELECT
        COUNT(ar.AR_Id) AS TotalSessions,
        COALESCE(SUM(CASE WHEN ar.AR_Status = 'present' THEN 1 ELSE 0 END), 0) AS PresentSessions,
        COALESCE(SUM(CASE WHEN ar.AR_Status = 'absent' THEN 1 ELSE 0 END), 0) AS AbsentSessions,
        COALESCE(SUM(CASE WHEN ar.AR_Status = 'half_day' THEN 1 ELSE 0 END), 0) AS HalfDaySessions,
        COALESCE(SUM(CASE WHEN ar.AR_Status = 'late' THEN 1 ELSE 0 END), 0) AS LateSessions,
        CASE
            WHEN COUNT(ar.AR_Id) > 0 THEN ROUND((CAST(COALESCE(SUM(CASE WHEN ar.AR_Status = 'present' THEN 1 ELSE 0 END), 0) AS FLOAT) / COUNT(ar.AR_Id)) * 100.0, 1)
            ELSE 0.0
        END AS AttendancePercentage
    FROM dbo.AttendanceRecords_AR ar
    INNER JOIN dbo.AttendanceSessions_AS asess ON ar.AR_AttendanceSessionId = asess.AS_Id
    WHERE ar.AR_StudentId = @StudentId
      AND asess.AS_AttendanceDate >= @StartOfMonth
      AND asess.AS_AttendanceDate <= @EndOfMonth;

    -- Result Set 3: Financial Summary Snapshot
    SELECT
        ISNULL((SELECT COUNT(FI_Id) FROM dbo.FeeInvoices_FI WHERE FI_StudentId = @StudentId AND FI_TenantId = @TenantId AND FI_Status != 'Cancelled'), 0) AS TotalInvoices,
        ISNULL((SELECT SUM(FI_TotalAmount) FROM dbo.FeeInvoices_FI WHERE FI_StudentId = @StudentId AND FI_TenantId = @TenantId AND FI_Status != 'Cancelled'), 0) AS TotalBilled,
        ISNULL((SELECT SUM(FI_PaidAmount) FROM dbo.FeeInvoices_FI WHERE FI_StudentId = @StudentId AND FI_TenantId = @TenantId AND FI_Status != 'Cancelled'), 0) AS TotalPaid,
        ISNULL((SELECT SUM(FI_BalanceAmount) FROM dbo.FeeInvoices_FI WHERE FI_StudentId = @StudentId AND FI_TenantId = @TenantId AND FI_Status != 'Cancelled'), 0) AS OutstandingBalance,
        ISNULL((SELECT COUNT(FI_Id) FROM dbo.FeeInvoices_FI WHERE FI_StudentId = @StudentId AND FI_TenantId = @TenantId AND FI_Status IN ('issued', 'partially_paid', 'unpaid', 'pending')), 0) AS PendingInvoicesCount;

    -- Result Set 4: Today's Timetable / Schedule
    DECLARE @TodayDayOfWeek SMALLINT = DATEPART(WEEKDAY, GETUTCDATE());
    SELECT
        tt.TT_Id AS TimetableId,
        tt.TT_DayOfWeek AS DayOfWeek,
        tt.TT_StartTime AS StartTime,
        tt.TT_EndTime AS EndTime,
        sb.SB_Name AS SubjectName,
        sb.SB_Code AS SubjectCode,
        st.ST_FirstName + ' ' + ISNULL(st.ST_LastName, '') AS TeacherName,
        cr.CR_Name AS ClassroomName
    FROM (
        SELECT BS_BatchId FROM dbo.BatchStudents_BS WHERE BS_StudentId = @StudentId AND BS_LeftAt IS NULL
        UNION
        SELECT E_BatchId AS BS_BatchId FROM dbo.Enrollments_E WHERE E_StudentId = @StudentId AND E_Status = 'Active'
    ) bs
    INNER JOIN dbo.Timetables_TT tt ON bs.BS_BatchId = tt.TT_BatchId
    LEFT JOIN dbo.Subjects_SB sb ON tt.TT_SubjectId = sb.SB_Id
    LEFT JOIN dbo.Staff_ST st ON tt.TT_StaffId = st.ST_Id
    LEFT JOIN dbo.Classrooms_CR cr ON tt.TT_ClassroomId = cr.CR_Id
    WHERE tt.TT_DayOfWeek = @TodayDayOfWeek
    ORDER BY tt.TT_StartTime;

    -- Result Set 5: Recent Announcements
    SELECT TOP 5
        ANN_Id AS NoticeId,
        ANN_Title AS Title,
        ANN_Content AS Content,
        ANN_PublishedAt AS PublishedAt
    FROM dbo.Announcements_ANN
    WHERE ANN_TenantId = @TenantId
      AND (ANN_ExpiresAt IS NULL OR ANN_ExpiresAt >= SYSUTCDATETIME())
    ORDER BY ANN_PublishedAt DESC;

    -- Result Set 6: Active Home Tasks
    SELECT TOP 5
        ht.HT_Id AS TaskId,
        ht.HT_Title AS Title,
        sb.SB_Name AS SubjectName,
        ht.HT_DueDate AS DueDate,
        ISNULL(hts.HTS_Status, 'Pending') AS SubmissionStatus
    FROM (
        SELECT BS_BatchId FROM dbo.BatchStudents_BS WHERE BS_StudentId = @StudentId AND BS_LeftAt IS NULL
        UNION
        SELECT E_BatchId AS BS_BatchId FROM dbo.Enrollments_E WHERE E_StudentId = @StudentId AND E_Status = 'Active'
    ) bs
    INNER JOIN dbo.HomeTasks_HT ht ON bs.BS_BatchId = ht.HT_BatchId AND ht.HT_Status = 'Active'
    LEFT JOIN dbo.Subjects_SB sb ON ht.HT_SubjectId = sb.SB_Id
    LEFT JOIN dbo.HomeTaskSubmissions_HTS hts ON ht.HT_Id = hts.HTS_HomeTaskId AND hts.HTS_StudentId = @StudentId
    ORDER BY ht.HT_DueDate ASC;
END
GO

-- ============================================================================
-- 3. SP_Portal_GetStudentProfile
-- Returns full bio, address, guardians, and academic allocation
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.SP_Portal_GetStudentProfile
    @StudentId  UNIQUEIDENTIFIER,
    @TenantId   UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    -- Student record
    SELECT
        s.*,
        ISNULL(b.B_Name, 'Main Campus') AS BranchName,
        COALESCE(bt.BT_Name, 'General Batch') AS BatchName,
        COALESCE(c.C_Name, 'Academic Course') AS CourseName,
        ay.AY_Name AS AcademicYearName
    FROM dbo.Students_S s
    LEFT JOIN dbo.Branches_B b ON s.S_BranchId = b.B_Id
    LEFT JOIN dbo.Enrollments_E e ON s.S_Id = e.E_StudentId AND e.E_Status = 'Active'
    LEFT JOIN dbo.BatchStudents_BS bs ON s.S_Id = bs.BS_StudentId AND bs.BS_LeftAt IS NULL
    LEFT JOIN dbo.Batches_BT bt ON COALESCE(e.E_BatchId, bs.BS_BatchId) = bt.BT_Id
    LEFT JOIN dbo.Courses_C c ON COALESCE(e.E_CourseId, bt.BT_CourseId) = c.C_Id
    LEFT JOIN dbo.AcademicYears_AY ay ON bt.BT_AcademicYearId = ay.AY_Id
    WHERE s.S_Id = @StudentId AND s.S_TenantId = @TenantId;

    -- Guardians
    SELECT
        g.G_Id AS GuardianId,
        g.G_FirstName + ' ' + ISNULL(g.G_LastName, '') AS GuardianName,
        g.G_Phone AS Phone,
        g.G_Email AS Email,
        g.G_Occupation AS Occupation,
        COALESCE(sg.SG_Relation, sg2.SG_Relationship, 'Guardian') AS Relationship,
        COALESCE(sg.SG_IsPrimary, sg2.SG_IsPrimary, 0) AS IsPrimary
    FROM (
        SELECT SG_StudentId, SG_GuardianId FROM dbo.Students_Guardians WHERE SG_StudentId = @StudentId
        UNION
        SELECT SG_StudentId, SG_GuardianId FROM dbo.StudentGuardians_SG WHERE SG_StudentId = @StudentId
    ) w
    INNER JOIN dbo.Guardians_G g ON w.SG_GuardianId = g.G_Id
    LEFT JOIN dbo.Students_Guardians sg ON w.SG_StudentId = sg.SG_StudentId AND sg.SG_GuardianId = g.G_Id
    LEFT JOIN dbo.StudentGuardians_SG sg2 ON w.SG_StudentId = sg2.SG_StudentId AND sg2.SG_GuardianId = g.G_Id
    ORDER BY COALESCE(sg.SG_IsPrimary, sg2.SG_IsPrimary, 0) DESC;
END
GO

-- ============================================================================
-- 4. SP_Portal_GetIdCardData
-- Returns Student & Institution details for digital ID Card & QR rendering
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.SP_Portal_GetIdCardData
    @StudentId  UNIQUEIDENTIFIER,
    @TenantId   UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        s.S_Id AS StudentId,
        s.S_FirstName + ' ' + ISNULL(s.S_LastName, '') AS StudentName,
        s.S_StudentCode AS StudentCode,
        s.S_AdmissionNumber AS AdmissionNumber,
        s.S_DateOfBirth AS DateOfBirth,
        ISNULL(s.S_BloodGroup, 'N/A') AS BloodGroup,
        s.S_Phone AS StudentPhone,
        s.S_Email AS StudentEmail,
        s.S_AddressLine1 AS Address,
        s.S_City AS City,
        COALESCE(c.C_Name, 'Academic Course') AS CourseName,
        COALESCE(bt.BT_Name, 'General Batch') AS BatchName,
        ay.AY_Name AS AcademicYearName,
        ISNULL(b.B_Name, 'Main Campus') AS BranchName,
        b.B_Phone AS BranchPhone,
        b.B_Email AS BranchEmail,
        ISNULL(b.B_AddressLine1, '123 College Road') AS BranchAddress,
        ISNULL(o.O_Name, 'INSTITUTE OF EDUCATION') AS OrganizationName,
        o.O_LogoUrl AS OrganizationLogoUrl,
        -- Primary guardian emergency contact
        COALESCE(
            (SELECT TOP 1 g.G_FirstName + ' ' + ISNULL(g.G_LastName, '') + ' (' + ISNULL(g.G_Phone, '') + ')'
             FROM dbo.Students_Guardians sg
             INNER JOIN dbo.Guardians_G g ON sg.SG_GuardianId = g.G_Id
             WHERE sg.SG_StudentId = s.S_Id
             ORDER BY sg.SG_IsPrimary DESC),
            (SELECT TOP 1 g.G_FirstName + ' ' + ISNULL(g.G_LastName, '') + ' (' + ISNULL(g.G_Phone, '') + ')'
             FROM dbo.StudentGuardians_SG sg2
             INNER JOIN dbo.Guardians_G g ON sg2.SG_GuardianId = g.G_Id
             WHERE sg2.SG_StudentId = s.S_Id
             ORDER BY sg2.SG_IsPrimary DESC),
            'School Office (9000000001)'
        ) AS EmergencyContact
    FROM dbo.Students_S s
    LEFT JOIN dbo.Branches_B b ON s.S_BranchId = b.B_Id
    LEFT JOIN dbo.Organizations_O o ON s.S_TenantId = o.O_Id
    LEFT JOIN dbo.Enrollments_E e ON s.S_Id = e.E_StudentId AND e.E_Status = 'Active'
    LEFT JOIN dbo.BatchStudents_BS bs ON s.S_Id = bs.BS_StudentId AND bs.BS_LeftAt IS NULL
    LEFT JOIN dbo.Batches_BT bt ON COALESCE(e.E_BatchId, bs.BS_BatchId) = bt.BT_Id
    LEFT JOIN dbo.Courses_C c ON COALESCE(e.E_CourseId, bt.BT_CourseId) = c.C_Id
    LEFT JOIN dbo.AcademicYears_AY ay ON bt.BT_AcademicYearId = ay.AY_Id
    WHERE s.S_Id = @StudentId AND s.S_TenantId = @TenantId;
END
GO

-- ============================================================================
-- 5. SP_Portal_GetGuardianIdCardData
-- Returns Guardian details and linked wards (supports lookup by GuardianId or StudentId)
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.SP_Portal_GetGuardianIdCardData
    @GuardianId UNIQUEIDENTIFIER = NULL,
    @StudentId  UNIQUEIDENTIFIER = NULL,
    @TenantId   UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    -- If GuardianId is null or empty, look up primary guardian from StudentId
    IF (@GuardianId IS NULL OR @GuardianId = '00000000-0000-0000-0000-000000000000') AND @StudentId IS NOT NULL
    BEGIN
        SELECT TOP 1 @GuardianId = g.G_Id
        FROM (
            SELECT SG_GuardianId, SG_IsPrimary FROM dbo.Students_Guardians WHERE SG_StudentId = @StudentId
            UNION
            SELECT SG_GuardianId, SG_IsPrimary FROM dbo.StudentGuardians_SG WHERE SG_StudentId = @StudentId
        ) w
        INNER JOIN dbo.Guardians_G g ON w.SG_GuardianId = g.G_Id
        ORDER BY w.SG_IsPrimary DESC;
    END

    -- If still null, try finding any guardian associated with this tenant
    IF @GuardianId IS NULL OR @GuardianId = '00000000-0000-0000-0000-000000000000'
    BEGIN
        SELECT TOP 1 @GuardianId = G_Id FROM dbo.Guardians_G WHERE G_TenantId = @TenantId;
    END

    -- Guardian details
    SELECT TOP 1
        g.G_Id AS GuardianId,
        g.G_FirstName + ' ' + ISNULL(g.G_LastName, '') AS GuardianName,
        ISNULL(g.G_Phone, 'N/A') AS Phone,
        g.G_Email AS Email,
        ISNULL(g.G_Occupation, 'Parent / Guardian') AS Occupation,
        ISNULL(o.O_Name, 'INSTITUTE OF EDUCATION') AS OrganizationName,
        o.O_LogoUrl AS OrganizationLogoUrl,
        ISNULL(b.B_Name, 'Main Campus') AS BranchName,
        ISNULL(b.B_AddressLine1, '123 College Road') AS BranchAddress,
        b.B_Phone AS BranchPhone
    FROM dbo.Guardians_G g
    LEFT JOIN dbo.Organizations_O o ON g.G_TenantId = o.O_Id
    LEFT JOIN dbo.Branches_B b ON b.B_TenantId = g.G_TenantId
    WHERE g.G_Id = @GuardianId;

    -- Wards
    SELECT
        s.S_Id AS StudentId,
        s.S_FirstName + ' ' + ISNULL(s.S_LastName, '') AS StudentName,
        s.S_StudentCode AS StudentCode,
        s.S_AdmissionNumber AS AdmissionNumber,
        COALESCE(c.C_Name, 'Academic Course') AS CourseName,
        COALESCE(bt.BT_Name, 'General Batch') AS BatchName,
        COALESCE(sg.SG_Relation, sg2.SG_Relationship, 'Ward') AS Relationship
    FROM (
        SELECT SG_StudentId, SG_GuardianId FROM dbo.Students_Guardians WHERE SG_GuardianId = @GuardianId
        UNION
        SELECT SG_StudentId, SG_GuardianId FROM dbo.StudentGuardians_SG WHERE SG_GuardianId = @GuardianId
    ) w
    INNER JOIN dbo.Students_S s ON w.SG_StudentId = s.S_Id
    LEFT JOIN dbo.Students_Guardians sg ON s.S_Id = sg.SG_StudentId AND sg.SG_GuardianId = @GuardianId
    LEFT JOIN dbo.StudentGuardians_SG sg2 ON s.S_Id = sg2.SG_StudentId AND sg2.SG_GuardianId = @GuardianId
    LEFT JOIN dbo.Enrollments_E e ON s.S_Id = e.E_StudentId AND e.E_Status = 'Active'
    LEFT JOIN dbo.BatchStudents_BS bs ON s.S_Id = bs.BS_StudentId AND bs.BS_LeftAt IS NULL
    LEFT JOIN dbo.Batches_BT bt ON COALESCE(e.E_BatchId, bs.BS_BatchId) = bt.BT_Id
    LEFT JOIN dbo.Courses_C c ON COALESCE(e.E_CourseId, bt.BT_CourseId) = c.C_Id
    ORDER BY s.S_FirstName;
END
GO

-- ============================================================================
-- 6. SP_Portal_GetAttendanceCalendar
-- Monthly calendar attendance entries and stats
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.SP_Portal_GetAttendanceCalendar
    @StudentId  UNIQUEIDENTIFIER,
    @TenantId   UNIQUEIDENTIFIER,
    @Month      INT,
    @Year       INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Daily Records
    SELECT
        asess.AS_AttendanceDate AS AttendanceDate,
        ar.AR_Status AS Status,
        ar.AR_Remarks AS Remarks,
        sb.SB_Name AS SubjectName,
        asess.AS_StartTime AS StartTime,
        asess.AS_EndTime AS EndTime
    FROM dbo.AttendanceRecords_AR ar
    INNER JOIN dbo.AttendanceSessions_AS asess ON ar.AR_AttendanceSessionId = asess.AS_Id
    LEFT JOIN dbo.Subjects_SB sb ON asess.AS_SubjectId = sb.SB_Id
    WHERE ar.AR_StudentId = @StudentId
      AND asess.AS_TenantId = @TenantId
      AND MONTH(asess.AS_AttendanceDate) = @Month
      AND YEAR(asess.AS_AttendanceDate) = @Year
    ORDER BY asess.AS_AttendanceDate ASC;

    -- Month Summary Stats
    SELECT
        COUNT(*) AS TotalDaysInMonth,
        --SUM(CASE WHEN ar.AR_Status = 'present' THEN 1 ELSE 0 END) AS PresentCount,
        --SUM(CASE WHEN ar.AR_Status = 'absent' THEN 1 ELSE 0 END) AS AbsentCount,
        --SUM(CASE WHEN ar.AR_Status = 'half_day' THEN 1 ELSE 0 END) AS HalfDayCount,
        --SUM(CASE WHEN ar.AR_Status = 'late' THEN 1 ELSE 0 END) AS LateCount,
        --SUM(CASE WHEN ar.AR_Status = 'excused' THEN 1 ELSE 0 END) AS ExcusedCount
        COALESCE(SUM(CASE WHEN ar.AR_Status = 'present' THEN 1 ELSE 0 END), 0) AS PresentCount,
        COALESCE(SUM(CASE WHEN ar.AR_Status = 'absent' THEN 1 ELSE 0 END), 0) AS AbsentCount,
        COALESCE(SUM(CASE WHEN ar.AR_Status = 'half_day' THEN 1 ELSE 0 END), 0) AS HalfDayCount,
        COALESCE(SUM(CASE WHEN ar.AR_Status = 'late' THEN 1 ELSE 0 END), 0) AS LateCount,
        COALESCE(SUM(CASE WHEN ar.AR_Status = 'excused' THEN 1 ELSE 0 END), 0) AS ExcusedCount
    FROM dbo.AttendanceRecords_AR ar
    INNER JOIN dbo.AttendanceSessions_AS asess ON ar.AR_AttendanceSessionId = asess.AS_Id
    WHERE ar.AR_StudentId = @StudentId
      AND asess.AS_TenantId = @TenantId
      AND MONTH(asess.AS_AttendanceDate) = @Month
      AND YEAR(asess.AS_AttendanceDate) = @Year;
END
GO

-- ============================================================================
-- 7. SP_Portal_GetTimetable
-- Returns full weekly class schedule for the student's current batch
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.SP_Portal_GetTimetable
    @StudentId  UNIQUEIDENTIFIER,
    @TenantId   UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        tt.TT_Id AS TimetableId,
        tt.TT_DayOfWeek AS DayOfWeek, -- 1=Sun, 2=Mon, 3=Tue, 4=Wed, 5=Thu, 6=Fri, 7=Sat
        tt.TT_StartTime AS StartTime,
        tt.TT_EndTime AS EndTime,
        sb.SB_Name AS SubjectName,
        sb.SB_Code AS SubjectCode,
        st.ST_FirstName + ' ' + st.ST_LastName AS TeacherName,
        cr.CR_Name AS ClassroomName,
        cr.CR_Location AS ClassroomLocation
    FROM dbo.BatchStudents_BS bs
    INNER JOIN dbo.Timetables_TT tt ON bs.BS_BatchId = tt.TT_BatchId
    LEFT JOIN dbo.Subjects_SB sb ON tt.TT_SubjectId = sb.SB_Id
    LEFT JOIN dbo.Staff_ST st ON tt.TT_StaffId = st.ST_Id
    LEFT JOIN dbo.Classrooms_CR cr ON tt.TT_ClassroomId = cr.CR_Id
    WHERE bs.BS_StudentId = @StudentId
      AND bs.BS_LeftAt IS NULL
      AND tt.TT_TenantId = @TenantId
    ORDER BY tt.TT_DayOfWeek, tt.TT_StartTime;
END
GO

-- ============================================================================
-- 8. SP_Portal_GetClassDetails
-- Returns batch, course, classroom, and enrolled subjects with teachers
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.SP_Portal_GetClassDetails
    @StudentId  UNIQUEIDENTIFIER,
    @TenantId   UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    -- Batch & Course Overview
    SELECT
        bt.BT_Id AS BatchId,
        bt.BT_Name AS BatchName,
        bt.BT_Code AS BatchCode,
        bt.BT_StartDate AS StartDate,
        bt.BT_EndDate AS EndDate,
        c.C_Name AS CourseName,
        c.C_Code AS CourseCode,
        c.C_Description AS CourseDescription,
        ay.AY_Name AS AcademicYearName,
        b.B_Name AS BranchName
    FROM dbo.BatchStudents_BS bs
    INNER JOIN dbo.Batches_BT bt ON bs.BS_BatchId = bt.BT_Id
    INNER JOIN dbo.Courses_C c ON bt.BT_CourseId = c.C_Id
    INNER JOIN dbo.AcademicYears_AY ay ON bt.BT_AcademicYearId = ay.AY_Id
    LEFT JOIN dbo.Branches_B b ON bt.BT_BranchId = b.B_Id
    WHERE bs.BS_StudentId = @StudentId AND bs.BS_LeftAt IS NULL;

    -- Enrolled Subjects
    SELECT
        sb.SB_Id AS SubjectId,
        sb.SB_Name AS SubjectName,
        sb.SB_Code AS SubjectCode,
        sb.SB_Credits AS Credits,
        cs.CS_MaxMarks AS MaxMarks,
        cs.CS_PassMarks AS PassMarks,
        cs.CS_IsMandatory AS IsMandatory
    FROM dbo.BatchStudents_BS bs
    INNER JOIN dbo.Batches_BT bt ON bs.BS_BatchId = bt.BT_Id
    INNER JOIN dbo.CourseSubjects_CS cs ON bt.BT_CourseId = cs.CS_CourseId
    INNER JOIN dbo.Subjects_SB sb ON cs.CS_SubjectId = sb.SB_Id
    WHERE bs.BS_StudentId = @StudentId AND bs.BS_LeftAt IS NULL
    ORDER BY cs.CS_SequenceNo;
END
GO

-- ============================================================================
-- 9. SP_Portal_GetHomeTasks
-- Retrieves active and past homework assignments with student submission status
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.SP_Portal_GetHomeTasks
    @StudentId  UNIQUEIDENTIFIER,
    @TenantId   UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ht.HT_Id AS TaskId,
        ht.HT_Title AS Title,
        ht.HT_Description AS Description,
        ht.HT_AssignedDate AS AssignedDate,
        ht.HT_DueDate AS DueDate,
        ht.HT_AttachmentUrl AS TeacherAttachmentUrl,
        ht.HT_MaxMarks AS MaxMarks,
        sb.SB_Name AS SubjectName,
        sb.SB_Code AS SubjectCode,
        hts.HTS_Id AS SubmissionId,
        hts.HTS_SubmissionDate AS SubmissionDate,
        hts.HTS_Content AS SubmissionContent,
        hts.HTS_AttachmentUrl AS StudentAttachmentUrl,
        hts.HTS_MarksObtained AS MarksObtained,
        hts.HTS_TeacherRemarks AS TeacherRemarks,
        ISNULL(hts.HTS_Status, 'Pending') AS SubmissionStatus,
        CASE WHEN hts.HTS_Id IS NOT NULL THEN 1 ELSE 0 END AS IsSubmitted,
        CASE WHEN hts.HTS_Id IS NULL AND ht.HT_DueDate < CAST(SYSUTCDATETIME() AS DATE) THEN 1 ELSE 0 END AS IsOverdue
    FROM dbo.BatchStudents_BS bs
    INNER JOIN dbo.HomeTasks_HT ht ON bs.BS_BatchId = ht.HT_BatchId
    LEFT JOIN dbo.Subjects_SB sb ON ht.HT_SubjectId = sb.SB_Id
    LEFT JOIN dbo.HomeTaskSubmissions_HTS hts ON ht.HT_Id = hts.HTS_HomeTaskId AND hts.HTS_StudentId = @StudentId
    WHERE bs.BS_StudentId = @StudentId
      AND bs.BS_LeftAt IS NULL
      AND ht.HT_TenantId = @TenantId
      AND ISNULL(ht.HT_IsActive, 1) = 1
    ORDER BY ht.HT_DueDate DESC;
END
GO

-- ============================================================================
-- 10. SP_Portal_SubmitHomeTask
-- Submits or updates student homework
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.SP_Portal_SubmitHomeTask
    @HomeTaskId     UNIQUEIDENTIFIER,
    @StudentId      UNIQUEIDENTIFIER,
    @Content        NVARCHAR(MAX) = NULL,
    @AttachmentUrl  NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    MERGE dbo.HomeTaskSubmissions_HTS AS target
    USING (SELECT @HomeTaskId AS TaskId, @StudentId AS StudentId) AS src
        ON target.HTS_HomeTaskId = src.TaskId AND target.HTS_StudentId = src.StudentId
    WHEN MATCHED THEN
        UPDATE SET
            HTS_Content = @Content,
            HTS_AttachmentUrl = ISNULL(@AttachmentUrl, HTS_AttachmentUrl),
            HTS_SubmissionDate = SYSUTCDATETIME(),
            HTS_Status = 'Submitted'
    WHEN NOT MATCHED THEN
        INSERT (HTS_Id, HTS_HomeTaskId, HTS_StudentId, HTS_SubmissionDate, HTS_Content, HTS_AttachmentUrl, HTS_Status)
        VALUES (NEWID(), @HomeTaskId, @StudentId, SYSUTCDATETIME(), @Content, @AttachmentUrl, 'Submitted');

    SELECT 1 AS Success;
END
GO

-- ============================================================================
-- 11. SP_Portal_GetSyllabus
-- Returns subject syllabus units
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.SP_Portal_GetSyllabus
    @StudentId  UNIQUEIDENTIFIER,
    @TenantId   UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ss.SS_Id AS SyllabusId,
        ss.SS_UnitNumber AS UnitNumber,
        ss.SS_UnitTitle AS UnitTitle,
        ss.SS_Description AS Description,
        ss.SS_TotalHours AS TotalHours,
        ss.SS_FileUrl AS FileUrl,
        ss.SS_IsCompleted AS IsCompleted,
        sb.SB_Id AS SubjectId,
        sb.SB_Name AS SubjectName,
        sb.SB_Code AS SubjectCode
    FROM dbo.BatchStudents_BS bs
    INNER JOIN dbo.Batches_BT bt ON bs.BS_BatchId = bt.BT_Id
    INNER JOIN dbo.SubjectSyllabus_SS ss ON bt.BT_CourseId = ss.SS_CourseId
    INNER JOIN dbo.Subjects_SB sb ON ss.SS_SubjectId = sb.SB_Id
    WHERE bs.BS_StudentId = @StudentId
      AND bs.BS_LeftAt IS NULL
      AND ss.SS_TenantId = @TenantId
      AND ISNULL(ss.SS_IsActive, 1) = 1
    ORDER BY sb.SB_Name, ss.SS_UnitNumber;
END
GO

-- ============================================================================
-- 12. SP_Portal_GetMockTests
-- Returns mock tests and completed scores
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.SP_Portal_GetMockTests
    @StudentId  UNIQUEIDENTIFIER,
    @TenantId   UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        mt.MT_Id AS TestId,
        mt.MT_Title AS Title,
        mt.MT_Description AS Description,
        mt.MT_TestDate AS TestDate,
        mt.MT_DurationMinutes AS DurationMinutes,
        mt.MT_TotalMarks AS TotalMarks,
        mt.MT_PassMarks AS PassMarks,
        mt.MT_Status AS TestStatus,
        sb.SB_Name AS SubjectName,
        mtr.MTR_Score AS Score,
        mtr.MTR_Percentage AS Percentage,
        mtr.MTR_Grade AS Grade,
        mtr.MTR_Status AS StudentResultStatus,
        mtr.MTR_CompletedAt AS CompletedAt
    FROM dbo.BatchStudents_BS bs
    INNER JOIN dbo.MockTests_MT mt ON bs.BS_BatchId = mt.MT_BatchId
    LEFT JOIN dbo.Subjects_SB sb ON mt.MT_SubjectId = sb.SB_Id
    LEFT JOIN dbo.MockTestResults_MTR mtr ON mt.MT_Id = mtr.MTR_MockTestId AND mtr.MTR_StudentId = @StudentId
    WHERE bs.BS_StudentId = @StudentId
      AND bs.BS_LeftAt IS NULL
      AND mt.MT_TenantId = @TenantId
      AND ISNULL(mt.MT_IsActive, 1) = 1
    ORDER BY mt.MT_TestDate DESC;
END
GO

-- ============================================================================
-- 13. SP_Portal_GetAdmitCard
-- Returns upcoming examination hall ticket details and schedule
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.SP_Portal_GetAdmitCard
    @StudentId  UNIQUEIDENTIFIER,
    @TenantId   UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    -- Exam & Hall Ticket Header
    SELECT TOP 1
        ex.EX_Id AS ExamId,
        ex.EX_Name AS ExamName,
        ex.EX_Code AS ExamCode,
        ex.EX_StartDate AS StartDate,
        ex.EX_EndDate AS EndDate,
        s.S_FirstName + ' ' + s.S_LastName AS StudentName,
        s.S_StudentCode AS StudentCode,
        s.S_AdmissionNumber AS RollNumber,
        c.C_Name AS CourseName,
        bt.BT_Name AS BatchName,
        b.B_Name AS CenterName,
        b.B_AddressLine1 AS CenterAddress,
        o.O_Name AS OrganizationName,
        o.O_LogoUrl AS OrganizationLogoUrl
    FROM dbo.BatchStudents_BS bs
    INNER JOIN dbo.Exams_EX ex ON bs.BS_BatchId = ex.EX_BatchId
    INNER JOIN dbo.Students_S s ON bs.BS_StudentId = s.S_Id
    LEFT JOIN dbo.Batches_BT bt ON bs.BS_BatchId = bt.BT_Id
    LEFT JOIN dbo.Courses_C c ON bt.BT_CourseId = c.C_Id
    LEFT JOIN dbo.Branches_B b ON ex.EX_TenantId = b.B_TenantId
    LEFT JOIN dbo.Organizations_O o ON ex.EX_TenantId = o.O_Id
    WHERE bs.BS_StudentId = @StudentId
      AND ex.EX_TenantId = @TenantId
      AND ex.EX_Status IN ('scheduled', 'ongoing')
    ORDER BY ex.EX_StartDate ASC;

    -- Exam Paper Timetable / Schedule
    SELECT
        es.ES_Id AS ExamSubjectId,
        sb.SB_Name AS SubjectName,
        sb.SB_Code AS SubjectCode,
        es.ES_MaxMarks AS MaxMarks,
        es.ES_PassMarks AS PassMarks,
        esc.ESC_ExamDate AS ExamDate,
        esc.ESC_StartTime AS StartTime,
        esc.ESC_EndTime AS EndTime,
        cr.CR_Name AS RoomNumber
    FROM dbo.BatchStudents_BS bs
    INNER JOIN dbo.Exams_EX ex ON bs.BS_BatchId = ex.EX_BatchId
    INNER JOIN dbo.ExamSubjects_ES es ON ex.EX_Id = es.ES_ExamId
    INNER JOIN dbo.Subjects_SB sb ON es.ES_SubjectId = sb.SB_Id
    LEFT JOIN dbo.ExamSchedules_ESC esc ON es.ES_Id = esc.ESC_ExamSubjectId
    LEFT JOIN dbo.Classrooms_CR cr ON esc.ESC_ClassroomId = cr.CR_Id
    WHERE bs.BS_StudentId = @StudentId
      AND ex.EX_TenantId = @TenantId
      AND ex.EX_Status IN ('scheduled', 'ongoing')
    ORDER BY esc.ESC_ExamDate, esc.ESC_StartTime;
END
GO

-- ============================================================================
-- 14. SP_Portal_GetMarkSheet
-- Returns completed exam results and subject marks
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.SP_Portal_GetMarkSheet
    @StudentId  UNIQUEIDENTIFIER,
    @TenantId   UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    -- Results List
    SELECT
        r.R_Id AS ResultId,
        r.R_ExamId AS ExamId,
        ex.EX_Name AS ExamName,
        ex.EX_Code AS ExamCode,
        r.R_TotalMarks AS TotalMarks,
        r.R_MarksObtained AS MarksObtained,
        r.R_Percentage AS Percentage,
        r.R_Grade AS OverallGrade,
        r.R_ResultStatus AS ResultStatus,
        r.R_Remarks AS Remarks,
        r.R_PublishedAt AS PublishedAt
    FROM dbo.Results_R r
    INNER JOIN dbo.Exams_EX ex ON r.R_ExamId = ex.EX_Id
    WHERE r.R_StudentId = @StudentId
      AND ex.EX_TenantId = @TenantId
    ORDER BY r.R_PublishedAt DESC;

    -- Subject-wise marks for published exams
    SELECT
        m.M_Id AS MarkId,
        ex.EX_Id AS ExamId,
        sb.SB_Name AS SubjectName,
        sb.SB_Code AS SubjectCode,
        es.ES_MaxMarks AS MaxMarks,
        es.ES_PassMarks AS PassMarks,
        m.M_MarksObtained AS MarksObtained,
        m.M_Percentage AS Percentage,
        gsi.GSI_Grade AS Grade,
        m.M_Remarks AS Remarks
    FROM dbo.Marks_M m
    INNER JOIN dbo.ExamSubjects_ES es ON m.M_ExamSubjectId = es.ES_Id
    INNER JOIN dbo.Exams_EX ex ON es.ES_ExamId = ex.EX_Id
    INNER JOIN dbo.Subjects_SB sb ON es.ES_SubjectId = sb.SB_Id
    LEFT JOIN dbo.GradeScaleItems_GSI gsi ON m.M_GradeScaleItemId = gsi.GSI_Id
    WHERE m.M_StudentId = @StudentId
      AND ex.EX_TenantId = @TenantId
    ORDER BY ex.EX_Name, sb.SB_Name;
END
GO

-- ============================================================================
-- 15. SP_Portal_GetFeeTransactions
-- Itemized fee invoices, payment history, and balances
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.SP_Portal_GetFeeTransactions
    @StudentId  UNIQUEIDENTIFIER,
    @TenantId   UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    -- Invoices
    SELECT
        fi.FI_Id AS InvoiceId,
        fi.FI_InvoiceNumber AS InvoiceNumber,
        fi.FI_InvoiceDate AS InvoiceDate,
        fi.FI_DueDate AS DueDate,
        fi.FI_Subtotal AS Subtotal,
        fi.FI_DiscountAmount AS DiscountAmount,
        fi.FI_TaxAmount AS TaxAmount,
        fi.FI_TotalAmount AS TotalAmount,
        fi.FI_PaidAmount AS PaidAmount,
        fi.FI_BalanceAmount AS BalanceAmount,
        fi.FI_Status AS Status,
        fi.FI_Notes AS Notes
    FROM dbo.FeeInvoices_FI fi
    WHERE fi.FI_StudentId = @StudentId
      AND fi.FI_TenantId = @TenantId
    ORDER BY fi.FI_InvoiceDate DESC;

    -- Payments & Transactions
    SELECT
        pay.PAY_Id AS PaymentId,
        pay.PAY_PaymentNumber AS PaymentNumber,
        pay.PAY_PaymentDate AS PaymentDate,
        pay.PAY_Amount AS Amount,
        pay.PAY_Status AS Status,
        pay.PAY_TransactionReference AS TransactionReference,
        pm.PM_Name AS PaymentMethodName,
        pay.PAY_Notes AS Notes
    FROM dbo.Payments_PAY pay
    LEFT JOIN dbo.PaymentMethods_PM pm ON pay.PAY_PaymentMethodId = pm.PM_Id
    WHERE pay.PAY_StudentId = @StudentId
      AND pay.PAY_TenantId = @TenantId
    ORDER BY pay.PAY_PaymentDate DESC;
END
GO

-- ============================================================================
-- 16. SP_Portal_GetReceiptDetails
-- Full voucher details for printing payment receipts
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.SP_Portal_GetReceiptDetails
    @PaymentId  UNIQUEIDENTIFIER,
    @TenantId   UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        pay.PAY_Id AS PaymentId,
        pay.PAY_PaymentNumber AS ReceiptNumber,
        pay.PAY_PaymentDate AS PaymentDate,
        pay.PAY_Amount AS PaidAmount,
        pay.PAY_TransactionReference AS TransactionRef,
        pm.PM_Name AS PaymentMode,
        s.S_FirstName + ' ' + s.S_LastName AS StudentName,
        s.S_StudentCode AS StudentCode,
        s.S_AdmissionNumber AS AdmissionNumber,
        c.C_Name AS CourseName,
        bt.BT_Name AS BatchName,
        b.B_Name AS BranchName,
        b.B_AddressLine1 AS BranchAddress,
        b.B_Phone AS BranchPhone,
        o.O_Name AS OrganizationName,
        o.O_LogoUrl AS OrganizationLogoUrl
    FROM dbo.Payments_PAY pay
    INNER JOIN dbo.Students_S s ON pay.PAY_StudentId = s.S_Id
    LEFT JOIN dbo.PaymentMethods_PM pm ON pay.PAY_PaymentMethodId = pm.PM_Id
    LEFT JOIN dbo.Branches_B b ON pay.PAY_TenantId = b.B_TenantId
    LEFT JOIN dbo.Organizations_O o ON pay.PAY_TenantId = o.O_Id
    LEFT JOIN dbo.BatchStudents_BS bs ON s.S_Id = bs.BS_StudentId AND bs.BS_LeftAt IS NULL
    LEFT JOIN dbo.Batches_BT bt ON bs.BS_BatchId = bt.BT_Id
    LEFT JOIN dbo.Courses_C c ON bt.BT_CourseId = c.C_Id
    WHERE pay.PAY_Id = @PaymentId AND pay.PAY_TenantId = @TenantId;

    -- Allocated Invoices
    SELECT
        fi.FI_InvoiceNumber AS InvoiceNumber,
        pa.PA_AllocatedAmount AS AmountPaid
    FROM dbo.PaymentAllocations_PA pa
    INNER JOIN dbo.FeeInvoices_FI fi ON pa.PA_InvoiceId = fi.FI_Id
    WHERE pa.PA_PaymentId = @PaymentId;
END
GO

-- ============================================================================
-- 17. SP_Portal_ApplyLeave & SP_Portal_GetLeaves
-- Student Leave Management
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.SP_Portal_ApplyLeave
    @SL_Id          UNIQUEIDENTIFIER,
    @SL_TenantId    UNIQUEIDENTIFIER,
    @SL_StudentId   UNIQUEIDENTIFIER,
    @SL_FromDate    DATE,
    @SL_ToDate      DATE,
    @SL_LeaveType   NVARCHAR(30),
    @SL_Reason      NVARCHAR(500),
    @SL_AppliedBy   NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    IF (@SL_ToDate < @SL_FromDate)
    BEGIN
        RAISERROR('End date cannot be prior to start date.', 16, 1);
        RETURN;
    END

    DECLARE @TotalDays INT = DATEDIFF(DAY, @SL_FromDate, @SL_ToDate) + 1;

    INSERT INTO dbo.StudentLeaves_SL
        (SL_Id, SL_TenantId, SL_StudentId, SL_FromDate, SL_ToDate, SL_TotalDays, SL_LeaveType, SL_Reason, SL_Status, SL_AppliedBy)
    VALUES
        (@SL_Id, @SL_TenantId, @SL_StudentId, @SL_FromDate, @SL_ToDate, @TotalDays, @SL_LeaveType, @SL_Reason, 'Pending', @SL_AppliedBy);

    SELECT @SL_Id AS LeaveId;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_Portal_GetLeaves
    @StudentId  UNIQUEIDENTIFIER,
    @TenantId   UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        SL_Id AS LeaveId,
        SL_FromDate AS FromDate,
        SL_ToDate AS ToDate,
        SL_TotalDays AS TotalDays,
        SL_LeaveType AS LeaveType,
        SL_Reason AS Reason,
        SL_Status AS Status,
        SL_AppliedBy AS AppliedBy,
        SL_RejectionReason AS RejectionReason,
        SL_CreatedAt AS AppliedAt
    FROM dbo.StudentLeaves_SL
    WHERE SL_StudentId = @StudentId
      AND SL_TenantId = @TenantId
      AND ISNULL(SL_IsActive, 1) = 1
    ORDER BY SL_CreatedAt DESC;
END
GO

-- ============================================================================
-- 18. SP_Portal_GetTransportDetails
-- Returns bus, driver, route, and stop information
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.SP_Portal_GetTransportDetails
    @StudentId  UNIQUEIDENTIFIER,
    @TenantId   UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        stp.STP_Id AS TransportId,
        stp.STP_StopName AS StopName,
        stp.STP_PickupTime AS PickupTime,
        stp.STP_DropTime AS DropTime,
        stp.STP_Status AS AllocationStatus,
        tr.TR_RouteName AS RouteName,
        tr.TR_RouteCode AS RouteCode,
        tr.TR_VehicleNumber AS VehicleNumber,
        tr.TR_DriverName AS DriverName,
        tr.TR_DriverPhone AS DriverPhone,
        tr.TR_HelperName AS HelperName,
        tr.TR_HelperPhone AS HelperPhone,
        tr.TR_StartLocation AS StartLocation,
        tr.TR_EndLocation AS EndLocation,
        tr.TR_MorningPickupTime AS RouteStartTime,
        tr.TR_EveningDropTime AS RouteEndTime
    FROM dbo.StudentTransport_STP stp
    INNER JOIN dbo.TransportRoutes_TR tr ON stp.STP_RouteId = tr.TR_Id
    WHERE stp.STP_StudentId = @StudentId
      AND stp.STP_TenantId = @TenantId
      AND stp.STP_Status = 'Active'
      AND ISNULL(stp.STP_IsActive, 1) = 1
      AND ISNULL(tr.TR_IsActive, 1) = 1;
END
GO

-- ============================================================================
-- 19. SP_Portal_ApplyTC & SP_Portal_GetTCStatus
-- Transfer Certificate application and status
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.SP_Portal_ApplyTC
    @TC_Id                  UNIQUEIDENTIFIER,
    @TC_TenantId            UNIQUEIDENTIFIER,
    @TC_StudentId           UNIQUEIDENTIFIER,
    @TC_Reason              NVARCHAR(500),
    @TC_ExpectedLeavingDate DATE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @AppNumber NVARCHAR(50) = 'TC-' + CAST(YEAR(SYSUTCDATETIME()) AS NVARCHAR(4)) + '-' + RIGHT(CAST(NEWID() AS NVARCHAR(36)), 6);

    INSERT INTO dbo.TransferCertificates_TC
        (TC_Id, TC_TenantId, TC_StudentId, TC_ApplicationNumber, TC_ApplicationDate, TC_Reason, TC_ExpectedLeavingDate, TC_Status)
    VALUES
        (@TC_Id, @TC_TenantId, @TC_StudentId, @AppNumber, CAST(SYSUTCDATETIME() AS DATE), @TC_Reason, @TC_ExpectedLeavingDate, 'Submitted');

    SELECT @TC_Id AS TCId, @AppNumber AS ApplicationNumber;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_Portal_GetTCStatus
    @StudentId  UNIQUEIDENTIFIER,
    @TenantId   UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        TC_Id AS TCId,
        TC_ApplicationNumber AS ApplicationNumber,
        TC_ApplicationDate AS ApplicationDate,
        TC_Reason AS Reason,
        TC_ExpectedLeavingDate AS ExpectedLeavingDate,
        TC_LibraryClearance AS LibraryClearance,
        TC_FeeClearance AS FeeClearance,
        TC_LabClearance AS LabClearance,
        TC_Status AS Status,
        TC_CertificateNumber AS CertificateNumber,
        TC_IssuedDate AS IssuedDate,
        TC_Remarks AS Remarks
    FROM dbo.TransferCertificates_TC
    WHERE TC_StudentId = @StudentId
      AND TC_TenantId = @TenantId
      AND ISNULL(TC_IsActive, 1) = 1
    ORDER BY TC_CreatedAt DESC;
END
GO

-- ============================================================================
-- 20. SP_Portal_GetNotices
-- Returns all published announcements
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.SP_Portal_GetNotices
    @TenantId   UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ANN_Id AS NoticeId,
        ANN_Title AS Title,
        ANN_Content AS Content,
        ANN_PublishedAt AS PublishedAt,
        ANN_ExpiresAt AS ExpiresAt
    FROM dbo.Announcements_ANN
    WHERE ANN_TenantId = @TenantId
      AND (ANN_ExpiresAt IS NULL OR ANN_ExpiresAt >= SYSUTCDATETIME())
    ORDER BY ANN_PublishedAt DESC;
END
GO

-- ============================================================================
-- 21. SP_Portal_UpdatePassword
-- Updates password for Student or Guardian
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.SP_Portal_UpdatePassword
    @UserId     UNIQUEIDENTIFIER,
    @UserType   NVARCHAR(20),
    @Password   NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    IF (@UserType = 'STUDENT')
    BEGIN
        UPDATE dbo.Students_S
        SET S_Password = @Password, S_UpdatedAt = SYSUTCDATETIME()
        WHERE S_Id = @UserId;
    END
    ELSE IF (@UserType = 'GUARDIAN')
    BEGIN
        UPDATE dbo.Guardians_G
        SET G_Password = @Password, G_UpdatedAt = SYSUTCDATETIME()
        WHERE G_Id = @UserId;
    END

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- ============================================================================
-- 22. SP_Portal_Admin_SetPassword
-- Administrative procedure to set or reset student / guardian password directly
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.SP_Portal_Admin_SetPassword
    @UserId         UNIQUEIDENTIFIER,
    @UserType       NVARCHAR(20), -- 'STUDENT' or 'GUARDIAN'
    @PasswordHash   NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    IF (@UserType = 'STUDENT')
    BEGIN
        UPDATE dbo.Students_S
        SET S_Password = @PasswordHash, S_UpdatedAt = SYSUTCDATETIME()
        WHERE S_Id = @UserId;
    END
    ELSE IF (@UserType = 'GUARDIAN')
    BEGIN
        UPDATE dbo.Guardians_G
        SET G_Password = @PasswordHash, G_UpdatedAt = SYSUTCDATETIME()
        WHERE G_Id = @UserId;
    END

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- ============================================================================
-- 23. SP_Portal_ForgotPassword_GenerateToken
-- Validates email, invalidates previous tokens, and generates new reset token
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.SP_Portal_ForgotPassword_GenerateToken
    @Email          NVARCHAR(255),
    @Token          NVARCHAR(200),
    @OtpCode        NVARCHAR(10) = NULL,
    @ExpiryMinutes  INT = 30
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @UserId UNIQUEIDENTIFIER = NULL;
    DECLARE @UserType NVARCHAR(20) = NULL;
    DECLARE @TenantId UNIQUEIDENTIFIER = NULL;
    DECLARE @FullName NVARCHAR(200) = NULL;

    -- Step 1: Check Students_S
    SELECT TOP 1
        @UserId = s.S_Id,
        @UserType = 'STUDENT',
        @TenantId = s.S_TenantId,
        @FullName = s.S_FirstName + ' ' + s.S_LastName
    FROM dbo.Students_S s
    WHERE s.S_Email = @Email
      AND s.S_DeletedAt IS NULL
      AND (ISNULL(s.S_IsActive, 1) = 1 OR s.S_Status = 'Active');

    -- Step 2: Check Guardians_G
    IF (@UserId IS NULL)
    BEGIN
        SELECT TOP 1
            @UserId = g.G_Id,
            @UserType = 'GUARDIAN',
            @TenantId = g.G_TenantId,
            @FullName = g.G_FirstName + ' ' + g.G_LastName
        FROM dbo.Guardians_G g
        WHERE g.G_Email = @Email
          AND ISNULL(g.G_IsActive, 1) = 1;
    END

    -- If neither found
    IF (@UserId IS NULL)
    BEGIN
        SELECT
            0 AS [Success],
            'No active student or guardian account registered with this email address.' AS [Message],
            NULL AS Token,
            NULL AS FullName,
            NULL AS UserType;
        RETURN;
    END

    -- Invalidate any existing active tokens for this email
    UPDATE dbo.PasswordResetTokens_PRT
    SET PRT_IsActive = 0
    WHERE PRT_Email = @Email AND PRT_IsActive = 1;

    -- Insert new token
    INSERT INTO dbo.PasswordResetTokens_PRT
        (PRT_Id, PRT_TenantId, PRT_Email, PRT_UserType, PRT_UserId, PRT_Token, PRT_OtpCode, PRT_ExpiresAt, PRT_IsUsed, PRT_IsActive, PRT_CreatedAt)
    VALUES
        (NEWID(), @TenantId, @Email, @UserType, @UserId, @Token, @OtpCode, DATEADD(MINUTE, @ExpiryMinutes, SYSUTCDATETIME()), 0, 1, SYSUTCDATETIME());

    SELECT
        1 AS [Success],
        'Password reset link generated successfully.' AS [Message],
        @Token AS Token,
        @FullName AS FullName,
        @UserType AS UserType;
END
GO

-- ============================================================================
-- 24. SP_Portal_ForgotPassword_ResetWithToken
-- Validates token & expiry, updates password in respective table, marks token used
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.SP_Portal_ForgotPassword_ResetWithToken
    @Token              NVARCHAR(200),
    @NewPasswordHash    NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @TokenId UNIQUEIDENTIFIER = NULL;
    DECLARE @UserId UNIQUEIDENTIFIER = NULL;
    DECLARE @UserType NVARCHAR(20) = NULL;
    DECLARE @Email NVARCHAR(255) = NULL;

    SELECT TOP 1
        @TokenId = PRT_Id,
        @UserId = PRT_UserId,
        @UserType = PRT_UserType,
        @Email = PRT_Email
    FROM dbo.PasswordResetTokens_PRT
    WHERE PRT_Token = @Token
      AND PRT_IsActive = 1
      AND PRT_IsUsed = 0
      AND PRT_ExpiresAt > SYSUTCDATETIME();

    IF (@TokenId IS NULL)
    BEGIN
        SELECT
            0 AS [Success],
            'The password reset link is invalid or has expired. Please request a new one.' AS [Message];
        RETURN;
    END

    -- Update user password
    IF (@UserType = 'STUDENT')
    BEGIN
        UPDATE dbo.Students_S
        SET S_Password = @NewPasswordHash, S_UpdatedAt = SYSUTCDATETIME()
        WHERE S_Id = @UserId;
    END
    ELSE IF (@UserType = 'GUARDIAN')
    BEGIN
        UPDATE dbo.Guardians_G
        SET G_Password = @NewPasswordHash, G_UpdatedAt = SYSUTCDATETIME()
        WHERE G_Id = @UserId;
    END

    -- Invalidate and mark token as used
    UPDATE dbo.PasswordResetTokens_PRT
    SET PRT_IsUsed = 1, PRT_IsActive = 0
    WHERE PRT_Id = @TokenId;

    SELECT
        1 AS [Success],
        'Your password has been updated successfully. You can now sign in with your new password.' AS [Message];
END
GO

-- ============================================================================
-- 25. SP_StudentLeaves_GetPaged (Admin / Teacher Panel)
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.SP_StudentLeaves_GetPaged
    @TenantId   UNIQUEIDENTIFIER,
    @Status     NVARCHAR(20) = NULL,
    @Search     NVARCHAR(100) = NULL,
    @PageNumber INT = 1,
    @PageSize   INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    SELECT
        sl.SL_Id AS LeaveId,
        sl.SL_StudentId AS StudentId,
        s.S_FirstName + ' ' + ISNULL(s.S_LastName, '') AS StudentName,
        s.S_StudentCode AS StudentCode,
        s.S_AdmissionNumber AS AdmissionNumber,
        COALESCE(c.C_Name, 'Standard Course') AS CourseName,
        COALESCE(bt.BT_Name, 'General Batch') AS BatchName,
        sl.SL_FromDate AS FromDate,
        sl.SL_ToDate AS ToDate,
        sl.SL_TotalDays AS TotalDays,
        sl.SL_LeaveType AS LeaveType,
        sl.SL_Reason AS Reason,
        sl.SL_Status AS Status,
        sl.SL_AppliedBy AS AppliedBy,
        sl.SL_ApprovedAt AS ApprovedAt,
        sl.SL_RejectionReason AS RejectionReason,
        sl.SL_CreatedAt AS AppliedAt,
        COUNT(*) OVER() AS TotalCount
    FROM dbo.StudentLeaves_SL sl
    INNER JOIN dbo.Students_S s ON sl.SL_StudentId = s.S_Id
    LEFT JOIN dbo.Enrollments_E e ON s.S_Id = e.E_StudentId AND e.E_Status = 'Active'
    LEFT JOIN dbo.BatchStudents_BS bs ON s.S_Id = bs.BS_StudentId AND bs.BS_LeftAt IS NULL
    LEFT JOIN dbo.Batches_BT bt ON COALESCE(e.E_BatchId, bs.BS_BatchId) = bt.BT_Id
    LEFT JOIN dbo.Courses_C c ON COALESCE(e.E_CourseId, bt.BT_CourseId) = c.C_Id
    WHERE sl.SL_TenantId = @TenantId
      AND sl.SL_IsActive = 1
      AND (@Status IS NULL OR @Status = '' OR sl.SL_Status = @Status)
      AND (@Search IS NULL OR @Search = '' OR s.S_FirstName LIKE '%' + @Search + '%' OR s.S_LastName LIKE '%' + @Search + '%' OR s.S_StudentCode LIKE '%' + @Search + '%')
    ORDER BY sl.SL_CreatedAt DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- ============================================================================
-- 26. SP_StudentLeaves_Review (Admin / Teacher Panel)
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.SP_StudentLeaves_Review
    @LeaveId        UNIQUEIDENTIFIER,
    @TenantId       UNIQUEIDENTIFIER,
    @ApprovedBy     UNIQUEIDENTIFIER,
    @Status         NVARCHAR(20), -- 'Approved' or 'Rejected'
    @RejectionReason NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.StudentLeaves_SL
    SET
        SL_Status = @Status,
        SL_ApprovedBy = @ApprovedBy,
        SL_ApprovedAt = SYSUTCDATETIME(),
        SL_RejectionReason = @RejectionReason,
        SL_UpdatedAt = SYSUTCDATETIME()
    WHERE SL_Id = @LeaveId AND SL_TenantId = @TenantId;

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- ============================================================================
-- 27. SP_TransferCertificates_GetPaged (Admin Panel)
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.SP_TransferCertificates_GetPaged
    @TenantId   UNIQUEIDENTIFIER,
    @Status     NVARCHAR(30) = NULL,
    @Search     NVARCHAR(100) = NULL,
    @PageNumber INT = 1,
    @PageSize   INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    SELECT
        tc.TC_Id AS TCId,
        tc.TC_StudentId AS StudentId,
        s.S_FirstName + ' ' + ISNULL(s.S_LastName, '') AS StudentName,
        s.S_StudentCode AS StudentCode,
        s.S_AdmissionNumber AS AdmissionNumber,
        COALESCE(c.C_Name, 'Standard Course') AS CourseName,
        COALESCE(bt.BT_Name, 'General Batch') AS BatchName,
        tc.TC_ApplicationNumber AS ApplicationNumber,
        tc.TC_ApplicationDate AS ApplicationDate,
        tc.TC_ExpectedLeavingDate AS ExpectedLeavingDate,
        tc.TC_Reason AS Reason,
        tc.TC_LibraryClearance AS LibraryClearance,
        tc.TC_FeeClearance AS FeeClearance,
        tc.TC_LabClearance AS LabClearance,
        tc.TC_Status AS Status,
        tc.TC_CertificateNumber AS CertificateNumber,
        tc.TC_IssuedDate AS IssuedDate,
        tc.TC_Remarks AS Remarks,
        tc.TC_CreatedAt AS CreatedAt,
        COUNT(*) OVER() AS TotalCount
    FROM dbo.TransferCertificates_TC tc
    INNER JOIN dbo.Students_S s ON tc.TC_StudentId = s.S_Id
    LEFT JOIN dbo.Enrollments_E e ON s.S_Id = e.E_StudentId AND e.E_Status = 'Active'
    LEFT JOIN dbo.BatchStudents_BS bs ON s.S_Id = bs.BS_StudentId AND bs.BS_LeftAt IS NULL
    LEFT JOIN dbo.Batches_BT bt ON COALESCE(e.E_BatchId, bs.BS_BatchId) = bt.BT_Id
    LEFT JOIN dbo.Courses_C c ON COALESCE(e.E_CourseId, bt.BT_CourseId) = c.C_Id
    WHERE tc.TC_TenantId = @TenantId
      AND tc.TC_IsActive = 1
      AND (@Status IS NULL OR @Status = '' OR tc.TC_Status = @Status)
      AND (@Search IS NULL OR @Search = '' OR s.S_FirstName LIKE '%' + @Search + '%' OR s.S_LastName LIKE '%' + @Search + '%' OR tc.TC_ApplicationNumber LIKE '%' + @Search + '%')
    ORDER BY tc.TC_CreatedAt DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- ============================================================================
-- 28. SP_TransferCertificates_Review (Admin Panel)
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.SP_TransferCertificates_Review
    @TCId               UNIQUEIDENTIFIER,
    @TenantId           UNIQUEIDENTIFIER,
    @LibraryClearance   BIT,
    @FeeClearance       BIT,
    @LabClearance       BIT,
    @Status             NVARCHAR(30),
    @Remarks            NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CertNumber NVARCHAR(50) = NULL;
    DECLARE @IssuedDate DATE = NULL;

    IF (@Status = 'Approved' OR @Status = 'Issued')
    BEGIN
        SET @IssuedDate = CAST(GETUTCDATE() AS DATE);
        SET @CertNumber = 'TC-' + CAST(YEAR(GETUTCDATE()) AS NVARCHAR(4)) + '-' + RIGHT(CAST(NEWID() AS NVARCHAR(36)), 6);
    END

    UPDATE dbo.TransferCertificates_TC
    SET
        TC_LibraryClearance = @LibraryClearance,
        TC_FeeClearance = @FeeClearance,
        TC_LabClearance = @LabClearance,
        TC_Status = @Status,
        TC_CertificateNumber = COALESCE(TC_CertificateNumber, @CertNumber),
        TC_IssuedDate = COALESCE(TC_IssuedDate, @IssuedDate),
        TC_Remarks = @Remarks,
        TC_UpdatedAt = SYSUTCDATETIME()
    WHERE TC_Id = @TCId AND TC_TenantId = @TenantId;

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- ============================================================================
-- 29. SP_StudentLeaves_Delete
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.SP_StudentLeaves_Delete
    @LeaveId    UNIQUEIDENTIFIER,
    @TenantId   UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.StudentLeaves_SL
    SET SL_IsActive = 0, SL_UpdatedAt = SYSUTCDATETIME()
    WHERE SL_Id = @LeaveId AND SL_TenantId = @TenantId;

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- ============================================================================
-- 30. SP_TransferCertificates_Delete
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.SP_TransferCertificates_Delete
    @TCId       UNIQUEIDENTIFIER,
    @TenantId   UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.TransferCertificates_TC
    SET TC_IsActive = 0, TC_UpdatedAt = SYSUTCDATETIME()
    WHERE TC_Id = @TCId AND TC_TenantId = @TenantId;

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- ============================================================================
-- 31. SP_TransferCertificates_GetById (For Printing and Official Issuance)
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.SP_TransferCertificates_GetById
    @TCId       UNIQUEIDENTIFIER,
    @TenantId   UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        tc.TC_Id AS TCId,
        tc.TC_StudentId AS StudentId,
        s.S_FirstName + ' ' + ISNULL(s.S_LastName, '') AS StudentName,
        s.S_StudentCode AS StudentCode,
        s.S_AdmissionNumber AS AdmissionNumber,
        s.S_Gender AS Gender,
        s.S_DateOfBirth AS DateOfBirth,
        ISNULL(s.S_BloodGroup, 'N/A') AS BloodGroup,
        ISNULL(s.S_AddressLine1, '') AS Address,
        s.S_Phone AS Phone,
        ISNULL(s.S_Email, '') AS Email,
        g.GuardianName AS GuardianName,
        COALESCE(c.C_Name, 'Academic Course') AS CourseName,
        COALESCE(bt.BT_Name, 'General Batch') AS BatchName,
        ay.AY_Name AS AcademicYearName,
        ISNULL(b.B_Name, 'Main Campus') AS BranchName,
        ISNULL(o.O_Name, 'INSTITUTE OF EDUCATION') AS OrganizationName,
        --ISNULL(o.O_Address, '') AS OrganizationAddress,
        '' AS OrganizationAddress,
        ISNULL(o.O_Phone, '') AS OrganizationPhone,
        ISNULL(o.O_Email, '') AS OrganizationEmail,
        tc.TC_ApplicationNumber AS ApplicationNumber,
        tc.TC_ApplicationDate AS ApplicationDate,
        tc.TC_ExpectedLeavingDate AS ExpectedLeavingDate,
        tc.TC_Reason AS Reason,
        tc.TC_LibraryClearance AS LibraryClearance,
        tc.TC_FeeClearance AS FeeClearance,
        tc.TC_LabClearance AS LabClearance,
        tc.TC_Status AS Status,
        COALESCE(tc.TC_CertificateNumber, 'TC-DRAFT-' + RIGHT(CAST(tc.TC_Id AS NVARCHAR(36)), 6)) AS CertificateNumber,
        COALESCE(tc.TC_IssuedDate, CAST(GETUTCDATE() AS DATE)) AS IssuedDate,
        tc.TC_Remarks AS Remarks,
        'Good' AS Conduct
    FROM dbo.TransferCertificates_TC tc
    INNER JOIN dbo.Students_S s ON tc.TC_StudentId = s.S_Id
    LEFT JOIN dbo.Organizations_O o ON tc.TC_TenantId = o.O_Id
    LEFT JOIN dbo.Branches_B b ON s.S_BranchId = b.B_Id
    LEFT JOIN dbo.Enrollments_E e ON s.S_Id = e.E_StudentId AND e.E_Status = 'Active'
    LEFT JOIN dbo.BatchStudents_BS bs ON s.S_Id = bs.BS_StudentId AND bs.BS_LeftAt IS NULL
    LEFT JOIN dbo.Batches_BT bt ON COALESCE(e.E_BatchId, bs.BS_BatchId) = bt.BT_Id
    LEFT JOIN dbo.Courses_C c ON COALESCE(e.E_CourseId, bt.BT_CourseId) = c.C_Id
    LEFT JOIN dbo.AcademicYears_AY ay ON bt.BT_AcademicYearId = ay.AY_Id
    OUTER APPLY (
        SELECT TOP 1 g.G_FirstName + ' ' + ISNULL(g.G_LastName, '') AS GuardianName
        FROM dbo.StudentGuardians_SG sg
        INNER JOIN dbo.Guardians_G g ON sg.SG_GuardianId = g.G_Id
        WHERE sg.SG_StudentId = s.S_Id
    ) g
    WHERE tc.TC_Id = @TCId AND tc.TC_TenantId = @TenantId;
END
GO


