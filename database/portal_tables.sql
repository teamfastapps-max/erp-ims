-- ============================================================================
-- ERP-IMS: Student & Guardian Portal Database Schema Additions
-- Script Date: 05-09-2026
-- ============================================================================
USE [IMS];
GO

-- 1. Add S_Password & S_IsActive to Students_S if not present
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Students_S') AND name = 'S_Password')
BEGIN
    ALTER TABLE dbo.Students_S ADD [S_Password] NVARCHAR(255) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Students_S') AND name = 'S_IsActive')
BEGIN
    ALTER TABLE dbo.Students_S ADD [S_IsActive] BIT NOT NULL CONSTRAINT DF_Students_S_IsActive DEFAULT 1;
END
GO

-- 2. Add G_Password & G_IsActive to Guardians_G if not present
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Guardians_G') AND name = 'G_Password')
BEGIN
    ALTER TABLE dbo.Guardians_G ADD [G_Password] NVARCHAR(255) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Guardians_G') AND name = 'G_IsActive')
BEGIN
    ALTER TABLE dbo.Guardians_G ADD [G_IsActive] BIT NOT NULL CONSTRAINT DF_Guardians_G_IsActive DEFAULT 1;
END
GO

-- Compatibility: If G_IsPortalActive exists, copy data or sync
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Guardians_G') AND name = 'G_IsPortalActive')
BEGIN
    EXEC sp_executesql N'UPDATE dbo.Guardians_G SET G_IsActive = ISNULL(G_IsPortalActive, 1) WHERE G_IsActive IS NULL;';
END
GO

-- 3. Student Leaves Table
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'StudentLeaves_SL')
BEGIN
    CREATE TABLE dbo.StudentLeaves_SL (
        SL_Id           UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_StudentLeaves_SL PRIMARY KEY CLUSTERED,
        SL_TenantId     UNIQUEIDENTIFIER NOT NULL,
        SL_StudentId    UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_StudentLeaves_SL_StudentId REFERENCES dbo.Students_S(S_Id),
        SL_FromDate     DATE NOT NULL,
        SL_ToDate       DATE NOT NULL,
        SL_TotalDays    INT NOT NULL,
        SL_LeaveType    NVARCHAR(30) NOT NULL, -- Sick, Casual, Medical, Other
        SL_Reason       NVARCHAR(500) NOT NULL,
        SL_Status       NVARCHAR(20) NOT NULL CONSTRAINT DF_SL_Status DEFAULT 'Pending', -- Pending, Approved, Rejected, Cancelled
        SL_AppliedBy    NVARCHAR(20) NOT NULL, -- Student, Guardian
        SL_ApprovedBy   UNIQUEIDENTIFIER NULL,
        SL_ApprovedAt   DATETIME2 NULL,
        SL_RejectionReason NVARCHAR(500) NULL,
        SL_IsActive     BIT NOT NULL CONSTRAINT DF_SL_IsActive DEFAULT 1,
        SL_CreatedAt    DATETIME2 NOT NULL CONSTRAINT DF_SL_CreatedAt DEFAULT SYSUTCDATETIME(),
        SL_UpdatedAt    DATETIME2 NOT NULL CONSTRAINT DF_SL_UpdatedAt DEFAULT SYSUTCDATETIME()
    );

    CREATE NONCLUSTERED INDEX IX_StudentLeaves_SL_StudentId ON dbo.StudentLeaves_SL(SL_TenantId, SL_StudentId, SL_Status);
END
ELSE IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.StudentLeaves_SL') AND name = 'SL_IsActive')
BEGIN
    ALTER TABLE dbo.StudentLeaves_SL ADD [SL_IsActive] BIT NOT NULL CONSTRAINT DF_StudentLeaves_SL_IsActive DEFAULT 1;
END
GO

-- 4. Home Tasks / Homework Table
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'HomeTasks_HT')
BEGIN
    CREATE TABLE dbo.HomeTasks_HT (
        HT_Id           UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_HomeTasks_HT PRIMARY KEY CLUSTERED,
        HT_TenantId     UNIQUEIDENTIFIER NOT NULL,
        HT_BatchId      UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_HomeTasks_HT_BatchId REFERENCES dbo.Batches_BT(BT_Id),
        HT_SubjectId    UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_HomeTasks_HT_SubjectId REFERENCES dbo.Subjects_SB(SB_Id),
        HT_TeacherId    UNIQUEIDENTIFIER NULL,
        HT_Title        NVARCHAR(200) NOT NULL,
        HT_Description  NVARCHAR(MAX) NOT NULL,
        HT_AssignedDate DATE NOT NULL,
        HT_DueDate      DATE NOT NULL,
        HT_AttachmentUrl NVARCHAR(500) NULL,
        HT_MaxMarks     NUMERIC(8,2) NULL,
        HT_Status       NVARCHAR(20) NOT NULL CONSTRAINT DF_HT_Status DEFAULT 'Active', -- Active, Closed
        HT_IsActive     BIT NOT NULL CONSTRAINT DF_HT_IsActive DEFAULT 1,
        HT_CreatedAt    DATETIME2 NOT NULL CONSTRAINT DF_HT_CreatedAt DEFAULT SYSUTCDATETIME(),
        HT_UpdatedAt    DATETIME2 NOT NULL CONSTRAINT DF_HT_UpdatedAt DEFAULT SYSUTCDATETIME()
    );

    CREATE NONCLUSTERED INDEX IX_HomeTasks_HT_BatchSubject ON dbo.HomeTasks_HT(HT_TenantId, HT_BatchId, HT_DueDate);
END
ELSE IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.HomeTasks_HT') AND name = 'HT_IsActive')
BEGIN
    ALTER TABLE dbo.HomeTasks_HT ADD [HT_IsActive] BIT NOT NULL CONSTRAINT DF_HomeTasks_HT_IsActive DEFAULT 1;
END
GO

-- 5. Home Task Submissions Table
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'HomeTaskSubmissions_HTS')
BEGIN
    CREATE TABLE dbo.HomeTaskSubmissions_HTS (
        HTS_Id          UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_HomeTaskSubmissions_HTS PRIMARY KEY CLUSTERED,
        HTS_HomeTaskId  UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_HTS_HomeTaskId REFERENCES dbo.HomeTasks_HT(HT_Id) ON DELETE CASCADE,
        HTS_StudentId   UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_HTS_StudentId REFERENCES dbo.Students_S(S_Id),
        HTS_SubmissionDate DATETIME2 NOT NULL CONSTRAINT DF_HTS_Date DEFAULT SYSUTCDATETIME(),
        HTS_Content     NVARCHAR(MAX) NULL,
        HTS_AttachmentUrl NVARCHAR(500) NULL,
        HTS_MarksObtained NUMERIC(8,2) NULL,
        HTS_TeacherRemarks NVARCHAR(500) NULL,
        HTS_Status      NVARCHAR(20) NOT NULL CONSTRAINT DF_HTS_Status DEFAULT 'Submitted', -- Submitted, Evaluated, Late
        HTS_IsActive    BIT NOT NULL CONSTRAINT DF_HTS_IsActive DEFAULT 1,
        CONSTRAINT UQ_HTS_Task_Student UNIQUE (HTS_HomeTaskId, HTS_StudentId)
    );
END
ELSE IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.HomeTaskSubmissions_HTS') AND name = 'HTS_IsActive')
BEGIN
    ALTER TABLE dbo.HomeTaskSubmissions_HTS ADD [HTS_IsActive] BIT NOT NULL CONSTRAINT DF_HomeTaskSubmissions_HTS_IsActive DEFAULT 1;
END
GO

-- 6. Subject Syllabus Table
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'SubjectSyllabus_SS')
BEGIN
    CREATE TABLE dbo.SubjectSyllabus_SS (
        SS_Id           UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_SubjectSyllabus_SS PRIMARY KEY CLUSTERED,
        SS_TenantId     UNIQUEIDENTIFIER NOT NULL,
        SS_CourseId     UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_SS_CourseId REFERENCES dbo.Courses_C(C_Id),
        SS_SubjectId    UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_SS_SubjectId REFERENCES dbo.Subjects_SB(SB_Id),
        SS_UnitNumber   INT NOT NULL,
        SS_UnitTitle    NVARCHAR(200) NOT NULL,
        SS_Description  NVARCHAR(MAX) NULL,
        SS_TotalHours   INT NULL,
        SS_FileUrl      NVARCHAR(500) NULL,
        SS_IsCompleted  BIT NOT NULL CONSTRAINT DF_SS_Completed DEFAULT 0,
        SS_IsActive     BIT NOT NULL CONSTRAINT DF_SS_IsActive DEFAULT 1,
        SS_CreatedAt    DATETIME2 NOT NULL CONSTRAINT DF_SS_CreatedAt DEFAULT SYSUTCDATETIME()
    );
END
ELSE IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.SubjectSyllabus_SS') AND name = 'SS_IsActive')
BEGIN
    ALTER TABLE dbo.SubjectSyllabus_SS ADD [SS_IsActive] BIT NOT NULL CONSTRAINT DF_SubjectSyllabus_SS_IsActive DEFAULT 1;
END
GO

-- 7. Mock Tests Table
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MockTests_MT')
BEGIN
    CREATE TABLE dbo.MockTests_MT (
        MT_Id           UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_MockTests_MT PRIMARY KEY CLUSTERED,
        MT_TenantId     UNIQUEIDENTIFIER NOT NULL,
        MT_BatchId      UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_MT_BatchId REFERENCES dbo.Batches_BT(BT_Id),
        MT_SubjectId    UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_MT_SubjectId REFERENCES dbo.Subjects_SB(SB_Id),
        MT_Title        NVARCHAR(200) NOT NULL,
        MT_Description  NVARCHAR(MAX) NULL,
        MT_TestDate     DATETIME2 NOT NULL,
        MT_DurationMinutes INT NOT NULL,
        MT_TotalMarks   NUMERIC(8,2) NOT NULL,
        MT_PassMarks    NUMERIC(8,2) NOT NULL,
        MT_Status       NVARCHAR(20) NOT NULL CONSTRAINT DF_MT_Status DEFAULT 'Scheduled', -- Scheduled, Ongoing, Completed, Cancelled
        MT_IsActive     BIT NOT NULL CONSTRAINT DF_MT_IsActive DEFAULT 1,
        MT_CreatedAt    DATETIME2 NOT NULL CONSTRAINT DF_MT_CreatedAt DEFAULT SYSUTCDATETIME()
    );
END
ELSE IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.MockTests_MT') AND name = 'MT_IsActive')
BEGIN
    ALTER TABLE dbo.MockTests_MT ADD [MT_IsActive] BIT NOT NULL CONSTRAINT DF_MockTests_MT_IsActive DEFAULT 1;
END
GO

-- 8. Mock Test Results Table
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MockTestResults_MTR')
BEGIN
    CREATE TABLE dbo.MockTestResults_MTR (
        MTR_Id          UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_MockTestResults_MTR PRIMARY KEY CLUSTERED,
        MTR_MockTestId  UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_MTR_MockTestId REFERENCES dbo.MockTests_MT(MT_Id) ON DELETE CASCADE,
        MTR_StudentId   UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_MTR_StudentId REFERENCES dbo.Students_S(S_Id),
        MTR_Score       NUMERIC(8,2) NOT NULL,
        MTR_Percentage  NUMERIC(5,2) NOT NULL,
        MTR_Grade       NVARCHAR(10) NULL,
        MTR_Status      NVARCHAR(20) NOT NULL, -- Pass, Fail, Absent
        MTR_IsActive    BIT NOT NULL CONSTRAINT DF_MTR_IsActive DEFAULT 1,
        MTR_CompletedAt DATETIME2 NOT NULL CONSTRAINT DF_MTR_CompletedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT UQ_MTR_Test_Student UNIQUE (MTR_MockTestId, MTR_StudentId)
    );
END
ELSE IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.MockTestResults_MTR') AND name = 'MTR_IsActive')
BEGIN
    ALTER TABLE dbo.MockTestResults_MTR ADD [MTR_IsActive] BIT NOT NULL CONSTRAINT DF_MockTestResults_MTR_IsActive DEFAULT 1;
END
GO

-- 9. Transport Routes & Bus Table
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TransportRoutes_TR')
BEGIN
    CREATE TABLE dbo.TransportRoutes_TR (
        TR_Id           UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_TransportRoutes_TR PRIMARY KEY CLUSTERED,
        TR_TenantId     UNIQUEIDENTIFIER NOT NULL,
        TR_RouteName    NVARCHAR(150) NOT NULL,
        TR_RouteCode    NVARCHAR(50) NOT NULL,
        TR_VehicleNumber NVARCHAR(50) NOT NULL,
        TR_DriverName   NVARCHAR(100) NOT NULL,
        TR_DriverPhone  NVARCHAR(30) NOT NULL,
        TR_HelperName   NVARCHAR(100) NULL,
        TR_HelperPhone  NVARCHAR(30) NULL,
        TR_StartLocation NVARCHAR(200) NOT NULL,
        TR_EndLocation  NVARCHAR(200) NOT NULL,
        TR_MorningPickupTime TIME(7) NOT NULL,
        TR_EveningDropTime TIME(7) NOT NULL,
        TR_MonthlyFee   NUMERIC(10,2) NOT NULL CONSTRAINT DF_TR_Fee DEFAULT 0,
        TR_IsActive     BIT NOT NULL CONSTRAINT DF_TR_IsActive DEFAULT 1,
        TR_CreatedAt    DATETIME2 NOT NULL CONSTRAINT DF_TR_CreatedAt DEFAULT SYSUTCDATETIME()
    );
END
ELSE IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TransportRoutes_TR') AND name = 'TR_IsActive')
BEGIN
    ALTER TABLE dbo.TransportRoutes_TR ADD [TR_IsActive] BIT NOT NULL CONSTRAINT DF_TransportRoutes_TR_IsActive DEFAULT 1;
END
GO

-- 10. Student Transport Allocation Table
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'StudentTransport_STP')
BEGIN
    CREATE TABLE dbo.StudentTransport_STP (
        STP_Id          UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_StudentTransport_STP PRIMARY KEY CLUSTERED,
        STP_TenantId    UNIQUEIDENTIFIER NOT NULL,
        STP_StudentId   UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_STP_StudentId REFERENCES dbo.Students_S(S_Id),
        STP_RouteId     UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_STP_RouteId REFERENCES dbo.TransportRoutes_TR(TR_Id),
        STP_StopName    NVARCHAR(150) NOT NULL,
        STP_PickupTime  TIME(7) NULL,
        STP_DropTime    TIME(7) NULL,
        STP_Status      NVARCHAR(20) NOT NULL CONSTRAINT DF_STP_Status DEFAULT 'Active',
        STP_StartDate   DATE NOT NULL,
        STP_EndDate     DATE NULL,
        STP_IsActive    BIT NOT NULL CONSTRAINT DF_STP_IsActive DEFAULT 1,
        CONSTRAINT UQ_STP_Student_Active UNIQUE (STP_StudentId, STP_RouteId)
    );
END
ELSE IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.StudentTransport_STP') AND name = 'STP_IsActive')
BEGIN
    ALTER TABLE dbo.StudentTransport_STP ADD [STP_IsActive] BIT NOT NULL CONSTRAINT DF_StudentTransport_STP_IsActive DEFAULT 1;
END
GO

-- 11. Transfer Certificate (TC) Applications Table
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TransferCertificates_TC')
BEGIN
    CREATE TABLE dbo.TransferCertificates_TC (
        TC_Id           UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_TransferCertificates_TC PRIMARY KEY CLUSTERED,
        TC_TenantId     UNIQUEIDENTIFIER NOT NULL,
        TC_StudentId    UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_TC_StudentId REFERENCES dbo.Students_S(S_Id),
        TC_ApplicationNumber NVARCHAR(50) NOT NULL,
        TC_ApplicationDate DATE NOT NULL,
        TC_Reason       NVARCHAR(500) NOT NULL,
        TC_ExpectedLeavingDate DATE NOT NULL,
        TC_LibraryClearance BIT NOT NULL CONSTRAINT DF_TC_Lib DEFAULT 0,
        TC_FeeClearance BIT NOT NULL CONSTRAINT DF_TC_Fee DEFAULT 0,
        TC_LabClearance BIT NOT NULL CONSTRAINT DF_TC_Lab DEFAULT 0,
        TC_Status       NVARCHAR(30) NOT NULL CONSTRAINT DF_TC_Status DEFAULT 'Submitted', -- Submitted, UnderReview, Approved, Issued, Rejected
        TC_CertificateNumber NVARCHAR(50) NULL,
        TC_IssuedDate   DATE NULL,
        TC_Remarks      NVARCHAR(MAX) NULL,
        TC_IsActive     BIT NOT NULL CONSTRAINT DF_TC_IsActive DEFAULT 1,
        TC_CreatedAt    DATETIME2 NOT NULL CONSTRAINT DF_TC_CreatedAt DEFAULT SYSUTCDATETIME(),
        TC_UpdatedAt    DATETIME2 NOT NULL CONSTRAINT DF_TC_UpdatedAt DEFAULT SYSUTCDATETIME()
    );

    CREATE NONCLUSTERED INDEX IX_TC_StudentId ON dbo.TransferCertificates_TC(TC_TenantId, TC_StudentId, TC_Status);
END
ELSE IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TransferCertificates_TC') AND name = 'TC_IsActive')
BEGIN
    ALTER TABLE dbo.TransferCertificates_TC ADD [TC_IsActive] BIT NOT NULL CONSTRAINT DF_TransferCertificates_TC_IsActive DEFAULT 1;
END
GO

-- 12. Password Reset Tokens Table (Forgot Password Flow)
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PasswordResetTokens_PRT')
BEGIN
    CREATE TABLE dbo.PasswordResetTokens_PRT (
        PRT_Id          UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_PasswordResetTokens_PRT PRIMARY KEY CLUSTERED DEFAULT NEWID(),
        PRT_TenantId    UNIQUEIDENTIFIER NOT NULL,
        PRT_Email       NVARCHAR(255) NOT NULL,
        PRT_UserType    NVARCHAR(20) NOT NULL, -- 'STUDENT' or 'GUARDIAN'
        PRT_UserId      UNIQUEIDENTIFIER NOT NULL,
        PRT_Token       NVARCHAR(200) NOT NULL,
        PRT_OtpCode     NVARCHAR(10) NULL,
        PRT_ExpiresAt   DATETIME2 NOT NULL,
        PRT_IsUsed      BIT NOT NULL CONSTRAINT DF_PRT_IsUsed DEFAULT 0,
        PRT_IsActive    BIT NOT NULL CONSTRAINT DF_PRT_IsActive DEFAULT 1,
        PRT_CreatedAt   DATETIME2 NOT NULL CONSTRAINT DF_PRT_CreatedAt DEFAULT SYSUTCDATETIME()
    );

    CREATE NONCLUSTERED INDEX IX_PRT_Token ON dbo.PasswordResetTokens_PRT(PRT_Token, PRT_IsActive, PRT_IsUsed);
    CREATE NONCLUSTERED INDEX IX_PRT_Email ON dbo.PasswordResetTokens_PRT(PRT_Email, PRT_IsActive);
END
ELSE IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PasswordResetTokens_PRT') AND name = 'PRT_IsActive')
BEGIN
    ALTER TABLE dbo.PasswordResetTokens_PRT ADD [PRT_IsActive] BIT NOT NULL CONSTRAINT DF_PasswordResetTokens_PRT_IsActive DEFAULT 1;
END
GO

PRINT 'Student & Guardian Portal Schema (Area: StudentPortal) additions and IsActive constraints created successfully.';
