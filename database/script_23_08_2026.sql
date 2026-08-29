USE [IMS]
GO

-- =========================================================================================
-- IMS Complete Database Architecture Schema
-- Updated in accordance with database_doc.md
-- Standardized on UNIQUEIDENTIFIER (GUID) Primary Keys across all entity tables
-- =========================================================================================

-- 1. Organizations_O
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Organizations_O]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Organizations_O](
        [O_Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_Organizations_O_Id] DEFAULT (newid()),
        [O_Name] [nvarchar](200) NOT NULL,
        [O_Code] [nvarchar](50) NOT NULL,
        [O_Currency] [nvarchar](10) NOT NULL CONSTRAINT [DF_Organizations_O_Currency] DEFAULT ('INR'),
        [O_TimeZone] [nvarchar](100) NOT NULL CONSTRAINT [DF_Organizations_O_TimeZone] DEFAULT ('Asia/Kolkata'),
        [O_IsActive] [bit] NOT NULL CONSTRAINT [DF_Organizations_O_IsActive] DEFAULT ((1)),
        [O_CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_Organizations_O_CreatedAt] DEFAULT (sysutcdatetime()),
        [O_UpdatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_Organizations_O_UpdatedAt] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_Organizations_O] PRIMARY KEY CLUSTERED ([O_Id] ASC),
        CONSTRAINT [UQ_Organizations_O_Code] UNIQUE NONCLUSTERED ([O_Code] ASC)
    );
END
GO

-- 2. OrganizationSettings_OS
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrganizationSettings_OS]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[OrganizationSettings_OS](
        [OS_Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_OrganizationSettings_OS_Id] DEFAULT (newid()),
        [OS_TenantId] [uniqueidentifier] NOT NULL,
        [OS_SettingKey] [nvarchar](100) NOT NULL,
        [OS_SettingValue] [nvarchar](max) NOT NULL,
        [OS_CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_OrganizationSettings_OS_CreatedAt] DEFAULT (sysutcdatetime()),
        [OS_UpdatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_OrganizationSettings_OS_UpdatedAt] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_OrganizationSettings_OS] PRIMARY KEY CLUSTERED ([OS_Id] ASC),
        CONSTRAINT [UQ_OrganizationSettings_OS_Tenant_Key] UNIQUE NONCLUSTERED ([OS_TenantId] ASC, [OS_SettingKey] ASC)
    );
END
GO

-- 3. Branches_B
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Branches_B]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Branches_B](
        [B_Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_Branches_B_Id] DEFAULT (newid()),
        [B_TenantId] [uniqueidentifier] NOT NULL,
        [B_Name] [nvarchar](200) NOT NULL,
        [B_Code] [nvarchar](50) NOT NULL,
        [B_Address] [nvarchar](max) NULL,
        [B_Phone] [nvarchar](30) NULL,
        [B_Email] [nvarchar](255) NULL,
        [B_IsActive] [bit] NOT NULL CONSTRAINT [DF_Branches_B_IsActive] DEFAULT ((1)),
        [B_CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_Branches_B_CreatedAt] DEFAULT (sysutcdatetime()),
        [B_UpdatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_Branches_B_UpdatedAt] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_Branches_B] PRIMARY KEY CLUSTERED ([B_Id] ASC),
        CONSTRAINT [UQ_Branches_B_Org_Code] UNIQUE NONCLUSTERED ([B_TenantId] ASC, [B_Code] ASC)
    );
END
GO

-- 4. AcademicYears_AY
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AcademicYears_AY]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AcademicYears_AY](
        [AY_Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_AcademicYears_AY_Id] DEFAULT (newid()),
        [AY_TenantId] [uniqueidentifier] NOT NULL,
        [AY_Name] [nvarchar](100) NOT NULL,
        [AY_Code] [nvarchar](50) NOT NULL,
        [AY_StartDate] [date] NOT NULL,
        [AY_EndDate] [date] NOT NULL,
        [AY_IsCurrent] [bit] NOT NULL CONSTRAINT [DF_AcademicYears_AY_IsCurrent] DEFAULT ((0)),
        [AY_IsActive] [bit] NOT NULL CONSTRAINT [DF_AcademicYears_AY_IsActive] DEFAULT ((1)),
        [AY_CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_AcademicYears_AY_CreatedAt] DEFAULT (sysutcdatetime()),
        [AY_UpdatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_AcademicYears_AY_UpdatedAt] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_AcademicYears_AY] PRIMARY KEY CLUSTERED ([AY_Id] ASC),
        CONSTRAINT [UQ_AcademicYears_AY_Org_Code] UNIQUE NONCLUSTERED ([AY_TenantId] ASC, [AY_Code] ASC)
    );
END
GO

-- 5. Programs_P
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Programs_P]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Programs_P](
        [P_Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_Programs_P_Id] DEFAULT (newid()),
        [P_TenantId] [uniqueidentifier] NOT NULL,
        [P_Name] [nvarchar](200) NOT NULL,
        [P_Code] [nvarchar](50) NOT NULL,
        [P_DurationValue] [int] NULL,
        [P_DurationUnit] [nvarchar](20) NULL,
        [P_Description] [nvarchar](max) NULL,
        [P_IsActive] [bit] NOT NULL CONSTRAINT [DF_Programs_P_IsActive] DEFAULT ((1)),
        [P_CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_Programs_P_CreatedAt] DEFAULT (sysutcdatetime()),
        [P_UpdatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_Programs_P_UpdatedAt] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_Programs_P] PRIMARY KEY CLUSTERED ([P_Id] ASC),
        CONSTRAINT [UQ_Programs_P_Org_Code] UNIQUE NONCLUSTERED ([P_TenantId] ASC, [P_Code] ASC)
    );
END
GO

-- 6. Courses_C
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Courses_C]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Courses_C](
        [C_Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_Courses_C_Id] DEFAULT (newid()),
        [C_TenantId] [uniqueidentifier] NOT NULL,
        [C_ProgramId] [uniqueidentifier] NULL,
        [C_Name] [nvarchar](200) NOT NULL,
        [C_Code] [nvarchar](50) NOT NULL,
        [C_DurationYears] [int] NULL,
        [C_Description] [nvarchar](max) NULL,
        [C_IsActive] [bit] NOT NULL CONSTRAINT [DF_Courses_C_IsActive] DEFAULT ((1)),
        [C_CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_Courses_C_CreatedAt] DEFAULT (sysutcdatetime()),
        [C_UpdatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_Courses_C_UpdatedAt] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_Courses_C] PRIMARY KEY CLUSTERED ([C_Id] ASC),
        CONSTRAINT [UQ_Courses_C_Org_Code] UNIQUE NONCLUSTERED ([C_TenantId] ASC, [C_Code] ASC)
    );
END
GO

-- 7. Subjects_SB
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Subjects_SB]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Subjects_SB](
        [SB_Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_Subjects_SB_Id] DEFAULT (newid()),
        [SB_TenantId] [uniqueidentifier] NOT NULL,
        [SB_Name] [nvarchar](200) NOT NULL,
        [SB_Code] [nvarchar](50) NOT NULL,
        [SB_Credits] [int] NULL,
        [SB_MaxMarks] [decimal](18, 2) NULL,
        [SB_PassMarks] [decimal](18, 2) NULL,
        [SB_Description] [nvarchar](max) NULL,
        [SB_IsActive] [bit] NOT NULL CONSTRAINT [DF_Subjects_SB_IsActive] DEFAULT ((1)),
        [SB_CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_Subjects_SB_CreatedAt] DEFAULT (sysutcdatetime()),
        [SB_UpdatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_Subjects_SB_UpdatedAt] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_Subjects_SB] PRIMARY KEY CLUSTERED ([SB_Id] ASC),
        CONSTRAINT [UQ_Subjects_SB_Org_Code] UNIQUE NONCLUSTERED ([SB_TenantId] ASC, [SB_Code] ASC)
    );
END
GO

-- 8. CourseSubjects_CS (Added CS_AcademicYearId per database_doc.md)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CourseSubjects_CS]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[CourseSubjects_CS](
        [CS_Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_CourseSubjects_CS_Id] DEFAULT (newid()),
        [CS_CourseId] [uniqueidentifier] NOT NULL,
        [CS_SubjectId] [uniqueidentifier] NOT NULL,
        [CS_AcademicYearId] [uniqueidentifier] NULL,
        [CS_SequenceNo] [int] NULL,
        [CS_MaxMarks] [decimal](18, 2) NULL,
        [CS_PassMarks] [decimal](18, 2) NULL,
        [CS_CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_CourseSubjects_CS_CreatedAt] DEFAULT (sysutcdatetime()),
        [CS_UpdatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_CourseSubjects_CS_UpdatedAt] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_CourseSubjects_CS] PRIMARY KEY CLUSTERED ([CS_Id] ASC)
    );
END
GO

-- 9. Batches_BT
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Batches_BT]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Batches_BT](
        [BT_Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_Batches_BT_Id] DEFAULT (newid()),
        [BT_TenantId] [uniqueidentifier] NOT NULL,
        [BT_BranchId] [uniqueidentifier] NOT NULL,
        [BT_CourseId] [uniqueidentifier] NOT NULL,
        [BT_AcademicYearId] [uniqueidentifier] NOT NULL,
        [BT_Name] [nvarchar](150) NOT NULL,
        [BT_Code] [nvarchar](50) NOT NULL,
        [BT_MaxCapacity] [int] NULL,
        [BT_IsActive] [bit] NOT NULL CONSTRAINT [DF_Batches_BT_IsActive] DEFAULT ((1)),
        [BT_CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_Batches_BT_CreatedAt] DEFAULT (sysutcdatetime()),
        [BT_UpdatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_Batches_BT_UpdatedAt] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_Batches_BT] PRIMARY KEY CLUSTERED ([BT_Id] ASC),
        CONSTRAINT [UQ_Batches_BT_Org_Branch_Year_Code] UNIQUE NONCLUSTERED ([BT_TenantId] ASC, [BT_BranchId] ASC, [BT_AcademicYearId] ASC, [BT_Code] ASC)
    );
END
GO

-- 10. Students_S
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Students_S]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Students_S](
        [S_Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_Students_S_Id] DEFAULT (newid()),
        [S_TenantId] [uniqueidentifier] NOT NULL,
        [S_BranchId] [uniqueidentifier] NOT NULL,
        [S_StudentCode] [nvarchar](50) NOT NULL,
        [S_FirstName] [nvarchar](100) NOT NULL,
        [S_LastName] [nvarchar](100) NOT NULL,
        [S_Gender] [nvarchar](20) NULL,
        [S_DateOfBirth] [date] NULL,
        [S_Email] [nvarchar](255) NULL,
        [S_Phone] [nvarchar](30) NULL,
        [S_Address] [nvarchar](max) NULL,
        [S_Status] [nvarchar](20) NOT NULL CONSTRAINT [DF_Students_S_Status] DEFAULT ('active'),
        [S_CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_Students_S_CreatedAt] DEFAULT (sysutcdatetime()),
        [S_UpdatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_Students_S_UpdatedAt] DEFAULT (sysutcdatetime()),
        [S_DeletedAt] [datetime2](7) NULL,
        CONSTRAINT [PK_Students_S] PRIMARY KEY CLUSTERED ([S_Id] ASC),
        CONSTRAINT [UQ_Students_S_Tenant_StudentCode] UNIQUE NONCLUSTERED ([S_TenantId] ASC, [S_StudentCode] ASC)
    );
END
GO

-- 11. Enrollments_E
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Enrollments_E]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Enrollments_E](
        [E_Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_Enrollments_E_Id] DEFAULT (newid()),
        [E_TenantId] [uniqueidentifier] NOT NULL,
        [E_StudentId] [uniqueidentifier] NOT NULL,
        [E_AcademicYearId] [uniqueidentifier] NOT NULL,
        [E_CourseId] [uniqueidentifier] NOT NULL,
        [E_BatchId] [uniqueidentifier] NOT NULL,
        [E_EnrollmentDate] [date] NOT NULL,
        [E_Status] [nvarchar](20) NOT NULL CONSTRAINT [DF_Enrollments_E_Status] DEFAULT ('enrolled'),
        [E_CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_Enrollments_E_CreatedAt] DEFAULT (sysutcdatetime()),
        [E_UpdatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_Enrollments_E_UpdatedAt] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_Enrollments_E] PRIMARY KEY CLUSTERED ([E_Id] ASC)
    );
END
GO

-- 12. BatchStudents_BS
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BatchStudents_BS]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[BatchStudents_BS](
        [BS_Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_BatchStudents_BS_Id] DEFAULT (newid()),
        [BS_BatchId] [uniqueidentifier] NOT NULL,
        [BS_StudentId] [uniqueidentifier] NOT NULL,
        [BS_JoinedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_BatchStudents_BS_JoinedAt] DEFAULT (sysutcdatetime()),
        [BS_LeftAt] [datetime2](7) NULL,
        [BS_Status] [nvarchar](20) NOT NULL CONSTRAINT [DF_BatchStudents_BS_Status] DEFAULT ('active'),
        [BS_CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_BatchStudents_BS_CreatedAt] DEFAULT (sysutcdatetime()),
        [BS_UpdatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_BatchStudents_BS_UpdatedAt] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_BatchStudents_BS] PRIMARY KEY CLUSTERED ([BS_Id] ASC)
    );
END
GO

-- 13. Guardians_G
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Guardians_G]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Guardians_G](
        [G_Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_Guardians_G_Id] DEFAULT (newid()),
        [G_TenantId] [uniqueidentifier] NOT NULL,
        [G_FirstName] [nvarchar](100) NOT NULL,
        [G_LastName] [nvarchar](100) NOT NULL,
        [G_Phone] [nvarchar](30) NOT NULL,
        [G_Email] [nvarchar](255) NULL,
        [G_Address] [nvarchar](max) NULL,
        [G_Occupation] [nvarchar](100) NULL,
        [G_CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_Guardians_G_CreatedAt] DEFAULT (sysutcdatetime()),
        [G_UpdatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_Guardians_G_UpdatedAt] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_Guardians_G] PRIMARY KEY CLUSTERED ([G_Id] ASC)
    );
END
GO

-- 14. StudentGuardians_SG
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StudentGuardians_SG]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[StudentGuardians_SG](
        [SG_StudentId] [uniqueidentifier] NOT NULL,
        [SG_GuardianId] [uniqueidentifier] NOT NULL,
        [SG_Relationship] [nvarchar](50) NOT NULL,
        [SG_IsPrimary] [bit] NOT NULL CONSTRAINT [DF_StudentGuardians_SG_IsPrimary] DEFAULT ((0)),
        [SG_IsEmergency] [bit] NOT NULL CONSTRAINT [DF_StudentGuardians_SG_IsEmergency] DEFAULT ((0)),
        CONSTRAINT [PK_StudentGuardians_SG] PRIMARY KEY CLUSTERED ([SG_StudentId] ASC, [SG_GuardianId] ASC)
    );
END
GO

-- 15. Departments_D
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Departments_D]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Departments_D](
        [D_Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_Departments_D_Id] DEFAULT (newid()),
        [D_TenantId] [uniqueidentifier] NOT NULL,
        [D_BranchId] [uniqueidentifier] NOT NULL,
        [D_Name] [nvarchar](200) NOT NULL,
        [D_Code] [nvarchar](50) NOT NULL,
        [D_IsActive] [bit] NOT NULL CONSTRAINT [DF_Departments_D_IsActive] DEFAULT ((1)),
        [D_CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_Departments_D_CreatedAt] DEFAULT (sysutcdatetime()),
        [D_UpdatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_Departments_D_UpdatedAt] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_Departments_D] PRIMARY KEY CLUSTERED ([D_Id] ASC),
        CONSTRAINT [UQ_Departments_D_Branch_Code] UNIQUE NONCLUSTERED ([D_BranchId] ASC, [D_Code] ASC)
    );
END
GO

-- 16. Designations_DS
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Designations_DS]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Designations_DS](
        [DS_Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_Designations_DS_Id] DEFAULT (newid()),
        [DS_TenantId] [uniqueidentifier] NOT NULL,
        [DS_Name] [nvarchar](100) NOT NULL,
        [DS_Code] [nvarchar](50) NOT NULL,
        [DS_IsActive] [bit] NOT NULL CONSTRAINT [DF_Designations_DS_IsActive] DEFAULT ((1)),
        [DS_CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_Designations_DS_CreatedAt] DEFAULT (sysutcdatetime()),
        [DS_UpdatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_Designations_DS_UpdatedAt] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_Designations_DS] PRIMARY KEY CLUSTERED ([DS_Id] ASC),
        CONSTRAINT [UQ_Designations_DS_Tenant_Code] UNIQUE NONCLUSTERED ([DS_TenantId] ASC, [DS_Code] ASC)
    );
END
GO

-- 17. Staff_ST (Critical Fixes per database_doc.md: PK, UQ, FKs added)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Staff_ST]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Staff_ST](
        [ST_Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_Staff_ST_Id] DEFAULT (newid()),
        [ST_TenantId] [uniqueidentifier] NOT NULL,
        [ST_BranchId] [uniqueidentifier] NOT NULL,
        [ST_UserId] [uniqueidentifier] NULL,
        [ST_DepartmentId] [uniqueidentifier] NULL,
        [ST_DesignationId] [uniqueidentifier] NULL,
        [ST_EmployeeCode] [nvarchar](50) NOT NULL,
        [ST_FirstName] [nvarchar](100) NOT NULL,
        [ST_LastName] [nvarchar](100) NOT NULL,
        [ST_Email] [nvarchar](255) NULL,
        [ST_Phone] [nvarchar](30) NULL,
        [ST_JoiningDate] [date] NULL,
        [ST_Status] [nvarchar](20) NOT NULL CONSTRAINT [DF_Staff_ST_Status] DEFAULT ('active'),
        [ST_CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_Staff_ST_CreatedAt] DEFAULT (sysutcdatetime()),
        [ST_UpdatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_Staff_ST_UpdatedAt] DEFAULT (sysutcdatetime()),
        [ST_DeletedAt] [datetime2](7) NULL,
        CONSTRAINT [PK_Staff_ST_Id] PRIMARY KEY CLUSTERED ([ST_Id] ASC),
        CONSTRAINT [UQ_Staff_ST_Org_EmployeeCode] UNIQUE NONCLUSTERED ([ST_TenantId] ASC, [ST_EmployeeCode] ASC)
    );
END
GO

-- 18. Classrooms_CR
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Classrooms_CR]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Classrooms_CR](
        [CR_Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_Classrooms_CR_Id] DEFAULT (newid()),
        [CR_TenantId] [uniqueidentifier] NOT NULL,
        [CR_BranchId] [uniqueidentifier] NOT NULL,
        [CR_RoomNumber] [nvarchar](50) NOT NULL,
        [CR_BuildingName] [nvarchar](150) NULL,
        [CR_Capacity] [int] NULL,
        [CR_IsActive] [bit] NOT NULL CONSTRAINT [DF_Classrooms_CR_IsActive] DEFAULT ((1)),
        [CR_CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_Classrooms_CR_CreatedAt] DEFAULT (sysutcdatetime()),
        [CR_UpdatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_Classrooms_CR_UpdatedAt] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_Classrooms_CR] PRIMARY KEY CLUSTERED ([CR_Id] ASC),
        CONSTRAINT [UQ_Classrooms_CR_Branch_Room] UNIQUE NONCLUSTERED ([CR_BranchId] ASC, [CR_RoomNumber] ASC)
    );
END
GO

-- 19. Timetables_TT
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Timetables_TT]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Timetables_TT](
        [TT_Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_Timetables_TT_Id] DEFAULT (newid()),
        [TT_TenantId] [uniqueidentifier] NOT NULL,
        [TT_BranchId] [uniqueidentifier] NOT NULL,
        [TT_BatchId] [uniqueidentifier] NOT NULL,
        [TT_SubjectId] [uniqueidentifier] NOT NULL,
        [TT_StaffId] [uniqueidentifier] NULL,
        [TT_ClassroomId] [uniqueidentifier] NULL,
        [TT_DayOfWeek] [nvarchar](20) NOT NULL,
        [TT_StartTime] [time](7) NOT NULL,
        [TT_EndTime] [time](7) NOT NULL,
        [TT_EffectiveFrom] [date] NOT NULL,
        [TT_EffectiveTo] [date] NULL,
        [TT_CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_Timetables_TT_CreatedAt] DEFAULT (sysutcdatetime()),
        [TT_UpdatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_Timetables_TT_UpdatedAt] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_Timetables_TT] PRIMARY KEY CLUSTERED ([TT_Id] ASC)
    );
END
GO

-- 20. AttendanceSessions_AS
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AttendanceSessions_AS]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AttendanceSessions_AS](
        [AS_Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_AttendanceSessions_AS_Id] DEFAULT (newid()),
        [AS_TenantId] [uniqueidentifier] NOT NULL,
        [AS_BranchId] [uniqueidentifier] NOT NULL,
        [AS_BatchId] [uniqueidentifier] NOT NULL,
        [AS_SubjectId] [uniqueidentifier] NULL,
        [AS_StaffId] [uniqueidentifier] NULL,
        [AS_SessionDate] [date] NOT NULL,
        [AS_StartTime] [time](7) NULL,
        [AS_EndTime] [time](7) NULL,
        [AS_TakenBy] [uniqueidentifier] NULL,
        [AS_CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_AttendanceSessions_AS_CreatedAt] DEFAULT (sysutcdatetime()),
        [AS_UpdatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_AttendanceSessions_AS_UpdatedAt] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_AttendanceSessions_AS] PRIMARY KEY CLUSTERED ([AS_Id] ASC)
    );
END
GO

-- 21. AttendanceRecords_AR
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AttendanceRecords_AR]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AttendanceRecords_AR](
        [AR_Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_AttendanceRecords_AR_Id] DEFAULT (newid()),
        [AR_SessionId] [uniqueidentifier] NOT NULL,
        [AR_StudentId] [uniqueidentifier] NOT NULL,
        [AR_Status] [nvarchar](20) NOT NULL,
        [AR_Remarks] [nvarchar](255) NULL,
        [AR_CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_AttendanceRecords_AR_CreatedAt] DEFAULT (sysutcdatetime()),
        [AR_UpdatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_AttendanceRecords_AR_UpdatedAt] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_AttendanceRecords_AR] PRIMARY KEY CLUSTERED ([AR_Id] ASC),
        CONSTRAINT [UQ_AttendanceRecords_AR_Session_Student] UNIQUE NONCLUSTERED ([AR_SessionId] ASC, [AR_StudentId] ASC)
    );
END
GO

-- 22. ExamTypes_ET
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExamTypes_ET]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ExamTypes_ET](
        [ET_Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_ExamTypes_ET_Id] DEFAULT (newid()),
        [ET_TenantId] [uniqueidentifier] NOT NULL,
        [ET_Name] [nvarchar](150) NOT NULL,
        [ET_Code] [nvarchar](50) NOT NULL,
        [ET_WeightagePercentage] [decimal](5, 2) NULL,
        [ET_IsActive] [bit] NOT NULL CONSTRAINT [DF_ExamTypes_ET_IsActive] DEFAULT ((1)),
        [ET_CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_ExamTypes_ET_CreatedAt] DEFAULT (sysutcdatetime()),
        [ET_UpdatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_ExamTypes_ET_UpdatedAt] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_ExamTypes_ET] PRIMARY KEY CLUSTERED ([ET_Id] ASC),
        CONSTRAINT [UQ_ExamTypes_ET_Tenant_Code] UNIQUE NONCLUSTERED ([ET_TenantId] ASC, [ET_Code] ASC)
    );
END
GO

-- 23. Exams_EX
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Exams_EX]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Exams_EX](
        [EX_Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_Exams_EX_Id] DEFAULT (newid()),
        [EX_TenantId] [uniqueidentifier] NOT NULL,
        [EX_BranchId] [uniqueidentifier] NOT NULL,
        [EX_BatchId] [uniqueidentifier] NOT NULL,
        [EX_ExamTypeId] [uniqueidentifier] NOT NULL,
        [EX_Name] [nvarchar](200) NOT NULL,
        [EX_StartDate] [date] NOT NULL,
        [EX_EndDate] [date] NOT NULL,
        [EX_Status] [nvarchar](20) NOT NULL CONSTRAINT [DF_Exams_EX_Status] DEFAULT ('scheduled'),
        [EX_CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_Exams_EX_CreatedAt] DEFAULT (sysutcdatetime()),
        [EX_UpdatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_Exams_EX_UpdatedAt] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_Exams_EX] PRIMARY KEY CLUSTERED ([EX_Id] ASC)
    );
END
GO

-- 24. ExamSubjects_ES
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExamSubjects_ES]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ExamSubjects_ES](
        [ES_Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_ExamSubjects_ES_Id] DEFAULT (newid()),
        [ES_ExamId] [uniqueidentifier] NOT NULL,
        [ES_SubjectId] [uniqueidentifier] NOT NULL,
        [ES_MaxMarks] [decimal](18, 2) NOT NULL,
        [ES_PassMarks] [decimal](18, 2) NOT NULL,
        [ES_Weightage] [decimal](5, 2) NULL,
        [ES_CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_ExamSubjects_ES_CreatedAt] DEFAULT (sysutcdatetime()),
        [ES_UpdatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_ExamSubjects_ES_UpdatedAt] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_ExamSubjects_ES] PRIMARY KEY CLUSTERED ([ES_Id] ASC)
    );
END
GO

-- 25. ExamSchedules_ESC
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExamSchedules_ESC]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ExamSchedules_ESC](
        [ESC_Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_ExamSchedules_ESC_Id] DEFAULT (newid()),
        [ESC_ExamSubjectId] [uniqueidentifier] NOT NULL,
        [ESC_ClassroomId] [uniqueidentifier] NULL,
        [ESC_ExamDate] [date] NOT NULL,
        [ESC_StartTime] [time](7) NOT NULL,
        [ESC_EndTime] [time](7) NOT NULL,
        [ESC_CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_ExamSchedules_ESC_CreatedAt] DEFAULT (sysutcdatetime()),
        [ESC_UpdatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_ExamSchedules_ESC_UpdatedAt] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_ExamSchedules_ESC] PRIMARY KEY CLUSTERED ([ESC_Id] ASC)
    );
END
GO

-- 26. GradeScales_GS
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GradeScales_GS]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[GradeScales_GS](
        [GS_Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_GradeScales_GS_Id] DEFAULT (newid()),
        [GS_TenantId] [uniqueidentifier] NOT NULL,
        [GS_Name] [nvarchar](150) NOT NULL,
        [GS_Code] [nvarchar](50) NOT NULL,
        [GS_Description] [nvarchar](max) NULL,
        [GS_IsActive] [bit] NOT NULL CONSTRAINT [DF_GradeScales_GS_IsActive] DEFAULT ((1)),
        [GS_CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_GradeScales_GS_CreatedAt] DEFAULT (sysutcdatetime()),
        [GS_UpdatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_GradeScales_GS_UpdatedAt] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_GradeScales_GS] PRIMARY KEY CLUSTERED ([GS_Id] ASC),
        CONSTRAINT [UQ_GradeScales_GS_Tenant_Code] UNIQUE NONCLUSTERED ([GS_TenantId] ASC, [GS_Code] ASC)
    );
END
GO

-- 27. GradeScaleItems_GSI
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GradeScaleItems_GSI]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[GradeScaleItems_GSI](
        [GSI_Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_GradeScaleItems_GSI_Id] DEFAULT (newid()),
        [GSI_GradeScaleId] [uniqueidentifier] NOT NULL,
        [GSI_Grade] [nvarchar](20) NOT NULL,
        [GSI_MinPercentage] [decimal](5, 2) NOT NULL,
        [GSI_MaxPercentage] [decimal](5, 2) NOT NULL,
        [GSI_GradePoint] [decimal](4, 2) NULL,
        [GSI_Description] [nvarchar](255) NULL,
        [GSI_CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_GradeScaleItems_GSI_CreatedAt] DEFAULT (sysutcdatetime()),
        [GSI_UpdatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_GradeScaleItems_GSI_UpdatedAt] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_GradeScaleItems_GSI] PRIMARY KEY CLUSTERED ([GSI_Id] ASC)
    );
END
GO

-- 28. Marks_M
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Marks_M]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Marks_M](
        [M_Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_Marks_M_Id] DEFAULT (newid()),
        [M_ExamSubjectId] [uniqueidentifier] NOT NULL,
        [M_StudentId] [uniqueidentifier] NOT NULL,
        [M_MarksObtained] [decimal](18, 2) NULL,
        [M_GradeScaleItemId] [uniqueidentifier] NULL,
        [M_Remarks] [nvarchar](255) NULL,
        [M_IsAbsent] [bit] NOT NULL CONSTRAINT [DF_Marks_M_IsAbsent] DEFAULT ((0)),
        [M_CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_Marks_M_CreatedAt] DEFAULT (sysutcdatetime()),
        [M_UpdatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_Marks_M_UpdatedAt] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_Marks_M] PRIMARY KEY CLUSTERED ([M_Id] ASC),
        CONSTRAINT [UQ_Marks_M_Subject_Student] UNIQUE NONCLUSTERED ([M_ExamSubjectId] ASC, [M_StudentId] ASC)
    );
END
GO

-- 29. Results_R
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Results_R]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Results_R](
        [R_Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_Results_R_Id] DEFAULT (newid()),
        [R_ExamId] [uniqueidentifier] NOT NULL,
        [R_StudentId] [uniqueidentifier] NOT NULL,
        [R_TotalMarks] [decimal](18, 2) NOT NULL,
        [R_ObtainedMarks] [decimal](18, 2) NOT NULL,
        [R_Percentage] [decimal](5, 2) NOT NULL,
        [R_Grade] [nvarchar](20) NULL,
        [R_ResultStatus] [nvarchar](20) NOT NULL,
        [R_CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_Results_R_CreatedAt] DEFAULT (sysutcdatetime()),
        [R_UpdatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_Results_R_UpdatedAt] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_Results_R] PRIMARY KEY CLUSTERED ([R_Id] ASC),
        CONSTRAINT [UQ_Results_R_Exam_Student] UNIQUE NONCLUSTERED ([R_ExamId] ASC, [R_StudentId] ASC)
    );
END
GO

-- 30. FeeCategories_FC
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FeeCategories_FC]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[FeeCategories_FC](
        [FC_Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_FeeCategories_FC_Id] DEFAULT (newid()),
        [FC_TenantId] [uniqueidentifier] NOT NULL,
        [FC_Name] [nvarchar](150) NOT NULL,
        [FC_Code] [nvarchar](50) NOT NULL,
        [FC_Description] [nvarchar](max) NULL,
        [FC_IsActive] [bit] NOT NULL CONSTRAINT [DF_FeeCategories_FC_IsActive] DEFAULT ((1)),
        [FC_CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_FeeCategories_FC_CreatedAt] DEFAULT (sysutcdatetime()),
        [FC_UpdatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_FeeCategories_FC_UpdatedAt] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_FeeCategories_FC] PRIMARY KEY CLUSTERED ([FC_Id] ASC),
        CONSTRAINT [UQ_FeeCategories_FC_Tenant_Code] UNIQUE NONCLUSTERED ([FC_TenantId] ASC, [FC_Code] ASC)
    );
END
GO

-- 31. Discounts_DIS
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Discounts_DIS]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Discounts_DIS](
        [DIS_Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_Discounts_DIS_Id] DEFAULT (newid()),
        [DIS_TenantId] [uniqueidentifier] NOT NULL,
        [DIS_Name] [nvarchar](150) NOT NULL,
        [DIS_Code] [nvarchar](50) NOT NULL,
        [DIS_DiscountType] [nvarchar](30) NOT NULL,
        [DIS_Value] [decimal](18, 2) NOT NULL,
        [DIS_IsActive] [bit] NOT NULL CONSTRAINT [DF_Discounts_DIS_IsActive] DEFAULT ((1)),
        [DIS_CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_Discounts_DIS_CreatedAt] DEFAULT (sysutcdatetime()),
        [DIS_UpdatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_Discounts_DIS_UpdatedAt] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_Discounts_DIS] PRIMARY KEY CLUSTERED ([DIS_Id] ASC),
        CONSTRAINT [UQ_Discounts_DIS_Tenant_Code] UNIQUE NONCLUSTERED ([DIS_TenantId] ASC, [DIS_Code] ASC)
    );
END
GO

-- 32. FeeStructures_FS
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FeeStructures_FS]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[FeeStructures_FS](
        [FS_Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_FeeStructures_FS_Id] DEFAULT (newid()),
        [FS_TenantId] [uniqueidentifier] NOT NULL,
        [FS_AcademicYearId] [uniqueidentifier] NOT NULL,
        [FS_CourseId] [uniqueidentifier] NULL,
        [FS_BatchId] [uniqueidentifier] NULL,
        [FS_Name] [nvarchar](150) NOT NULL,
        [FS_Description] [nvarchar](max) NULL,
        [FS_IsActive] [bit] NOT NULL CONSTRAINT [DF_FeeStructures_FS_IsActive] DEFAULT ((1)),
        [FS_CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_FeeStructures_FS_CreatedAt] DEFAULT (sysutcdatetime()),
        [FS_UpdatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_FeeStructures_FS_UpdatedAt] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_FeeStructures_FS] PRIMARY KEY CLUSTERED ([FS_Id] ASC)
    );
END
GO

-- 33. FeeStructureItems_FSI
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FeeStructureItems_FSI]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[FeeStructureItems_FSI](
        [FSI_Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_FeeStructureItems_FSI_Id] DEFAULT (newid()),
        [FSI_FeeStructureId] [uniqueidentifier] NOT NULL,
        [FSI_FeeCategoryId] [uniqueidentifier] NOT NULL,
        [FSI_Amount] [decimal](18, 2) NOT NULL,
        [FSI_DueDate] [date] NULL,
        [FSI_CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_FeeStructureItems_FSI_CreatedAt] DEFAULT (sysutcdatetime()),
        [FSI_UpdatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_FeeStructureItems_FSI_UpdatedAt] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_FeeStructureItems_FSI] PRIMARY KEY CLUSTERED ([FSI_Id] ASC)
    );
END
GO

-- 34. StudentFeeAssignments_SFA (Added SFA_DiscountId per database_doc.md)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StudentFeeAssignments_SFA]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[StudentFeeAssignments_SFA](
        [SFA_Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_StudentFeeAssignments_SFA_Id] DEFAULT (newid()),
        [SFA_StudentId] [uniqueidentifier] NOT NULL,
        [SFA_FeeStructureId] [uniqueidentifier] NOT NULL,
        [SFA_DiscountId] [uniqueidentifier] NULL,
        [SFA_DiscountAmount] [decimal](18, 2) NOT NULL CONSTRAINT [DF_StudentFeeAssignments_SFA_DiscountAmount] DEFAULT ((0)),
        [SFA_TotalAmount] [decimal](18, 2) NOT NULL,
        [SFA_DueDate] [date] NULL,
        [SFA_Status] [nvarchar](20) NOT NULL CONSTRAINT [DF_StudentFeeAssignments_SFA_Status] DEFAULT ('assigned'),
        [SFA_CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_StudentFeeAssignments_SFA_CreatedAt] DEFAULT (sysutcdatetime()),
        [SFA_UpdatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_StudentFeeAssignments_SFA_UpdatedAt] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_StudentFeeAssignments_SFA] PRIMARY KEY CLUSTERED ([SFA_Id] ASC)
    );
END
GO

-- 35. FeeInvoices_FI (Added FI_StudentFeeAssignmentId per database_doc.md)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FeeInvoices_FI]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[FeeInvoices_FI](
        [FI_Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_FeeInvoices_FI_Id] DEFAULT (newid()),
        [FI_TenantId] [uniqueidentifier] NOT NULL,
        [FI_StudentId] [uniqueidentifier] NOT NULL,
        [FI_StudentFeeAssignmentId] [uniqueidentifier] NULL,
        [FI_InvoiceNumber] [nvarchar](50) NOT NULL,
        [FI_InvoiceDate] [date] NOT NULL,
        [FI_DueDate] [date] NOT NULL,
        [FI_TotalAmount] [decimal](18, 2) NOT NULL,
        [FI_PaidAmount] [decimal](18, 2) NOT NULL CONSTRAINT [DF_FeeInvoices_FI_PaidAmount] DEFAULT ((0)),
        [FI_Status] [nvarchar](20) NOT NULL CONSTRAINT [DF_FeeInvoices_FI_Status] DEFAULT ('unpaid'),
        [FI_CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_FeeInvoices_FI_CreatedAt] DEFAULT (sysutcdatetime()),
        [FI_UpdatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_FeeInvoices_FI_UpdatedAt] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_FeeInvoices_FI] PRIMARY KEY CLUSTERED ([FI_Id] ASC),
        CONSTRAINT [UQ_FeeInvoices_FI_Tenant_InvoiceNumber] UNIQUE NONCLUSTERED ([FI_TenantId] ASC, [FI_InvoiceNumber] ASC)
    );
END
GO

-- 36. FeeInvoiceItems_FII (Added FII_DiscountId per database_doc.md)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FeeInvoiceItems_FII]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[FeeInvoiceItems_FII](
        [FII_Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_FeeInvoiceItems_FII_Id] DEFAULT (newid()),
        [FII_InvoiceId] [uniqueidentifier] NOT NULL,
        [FII_FeeCategoryId] [uniqueidentifier] NOT NULL,
        [FII_DiscountId] [uniqueidentifier] NULL,
        [FII_Amount] [decimal](18, 2) NOT NULL,
        [FII_DiscountAmount] [decimal](18, 2) NOT NULL CONSTRAINT [DF_FeeInvoiceItems_FII_DiscountAmount] DEFAULT ((0)),
        [FII_NetAmount] [decimal](18, 2) NOT NULL,
        [FII_CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_FeeInvoiceItems_FII_CreatedAt] DEFAULT (sysutcdatetime()),
        [FII_UpdatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_FeeInvoiceItems_FII_UpdatedAt] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_FeeInvoiceItems_FII] PRIMARY KEY CLUSTERED ([FII_Id] ASC)
    );
END
GO

-- 37. PaymentMethods_PM
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PaymentMethods_PM]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[PaymentMethods_PM](
        [PM_Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_PaymentMethods_PM_Id] DEFAULT (newid()),
        [PM_TenantId] [uniqueidentifier] NOT NULL,
        [PM_Name] [nvarchar](100) NOT NULL,
        [PM_Type] [nvarchar](30) NOT NULL,
        [PM_IsActive] [bit] NOT NULL CONSTRAINT [DF_PaymentMethods_PM_IsActive] DEFAULT ((1)),
        [PM_CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_PaymentMethods_PM_CreatedAt] DEFAULT (sysutcdatetime()),
        [PM_UpdatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_PaymentMethods_PM_UpdatedAt] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_PaymentMethods_PM] PRIMARY KEY CLUSTERED ([PM_Id] ASC),
        CONSTRAINT [UQ_PaymentMethods_PM_Tenant_Name] UNIQUE NONCLUSTERED ([PM_TenantId] ASC, [PM_Name] ASC)
    );
END
GO

-- 38. Payments_PAY
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Payments_PAY]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Payments_PAY](
        [PAY_Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_Payments_PAY_Id] DEFAULT (newid()),
        [PAY_TenantId] [uniqueidentifier] NOT NULL,
        [PAY_StudentId] [uniqueidentifier] NOT NULL,
        [PAY_PaymentMethodId] [uniqueidentifier] NOT NULL,
        [PAY_PaymentNumber] [nvarchar](50) NOT NULL,
        [PAY_Amount] [decimal](18, 2) NOT NULL,
        [PAY_PaymentDate] [date] NOT NULL,
        [PAY_ReferenceNumber] [nvarchar](100) NULL,
        [PAY_Remarks] [nvarchar](max) NULL,
        [PAY_CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_Payments_PAY_CreatedAt] DEFAULT (sysutcdatetime()),
        [PAY_UpdatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_Payments_PAY_UpdatedAt] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_Payments_PAY] PRIMARY KEY CLUSTERED ([PAY_Id] ASC),
        CONSTRAINT [UQ_Payments_PAY_Tenant_PaymentNumber] UNIQUE NONCLUSTERED ([PAY_TenantId] ASC, [PAY_PaymentNumber] ASC)
    );
END
GO

-- 39. PaymentAllocations_PA
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PaymentAllocations_PA]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[PaymentAllocations_PA](
        [PA_Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_PaymentAllocations_PA_Id] DEFAULT (newid()),
        [PA_PaymentId] [uniqueidentifier] NOT NULL,
        [PA_InvoiceId] [uniqueidentifier] NOT NULL,
        [PA_Amount] [decimal](18, 2) NOT NULL,
        [PA_CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_PaymentAllocations_PA_CreatedAt] DEFAULT (sysutcdatetime()),
        [PA_UpdatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_PaymentAllocations_PA_UpdatedAt] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_PaymentAllocations_PA] PRIMARY KEY CLUSTERED ([PA_Id] ASC)
    );
END
GO

-- 40. Refunds_RF
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Refunds_RF]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Refunds_RF](
        [RF_Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_Refunds_RF_Id] DEFAULT (newid()),
        [RF_TenantId] [uniqueidentifier] NOT NULL,
        [RF_PaymentId] [uniqueidentifier] NOT NULL,
        [RF_Amount] [decimal](18, 2) NOT NULL,
        [RF_RefundDate] [date] NOT NULL,
        [RF_Reason] [nvarchar](max) NULL,
        [RF_Status] [nvarchar](20) NOT NULL CONSTRAINT [DF_Refunds_RF_Status] DEFAULT ('processed'),
        [RF_CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_Refunds_RF_CreatedAt] DEFAULT (sysutcdatetime()),
        [RF_UpdatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_Refunds_RF_UpdatedAt] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_Refunds_RF] PRIMARY KEY CLUSTERED ([RF_Id] ASC)
    );
END
GO

-- 41. DocumentTypes_DT
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DocumentTypes_DT]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[DocumentTypes_DT](
        [DT_Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_DocumentTypes_DT_Id] DEFAULT (newid()),
        [DT_TenantId] [uniqueidentifier] NOT NULL,
        [DT_Name] [nvarchar](150) NOT NULL,
        [DT_Code] [nvarchar](50) NOT NULL,
        [DT_AllowedExtensions] [nvarchar](255) NULL,
        [DT_MaxSizeBytes] [bigint] NULL,
        [DT_IsRequired] [bit] NOT NULL CONSTRAINT [DF_DocumentTypes_DT_IsRequired] DEFAULT ((0)),
        [DT_IsActive] [bit] NOT NULL CONSTRAINT [DF_DocumentTypes_DT_IsActive] DEFAULT ((1)),
        [DT_CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_DocumentTypes_DT_CreatedAt] DEFAULT (sysutcdatetime()),
        [DT_UpdatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_DocumentTypes_DT_UpdatedAt] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_DocumentTypes_DT] PRIMARY KEY CLUSTERED ([DT_Id] ASC),
        CONSTRAINT [UQ_DocumentTypes_DT_Tenant_Code] UNIQUE NONCLUSTERED ([DT_TenantId] ASC, [DT_Code] ASC)
    );
END
GO

-- 42. Documents_DOC
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Documents_DOC]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Documents_DOC](
        [DOC_Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_Documents_DOC_Id] DEFAULT (newid()),
        [DOC_TenantId] [uniqueidentifier] NOT NULL,
        [DOC_DocumentTypeId] [uniqueidentifier] NOT NULL,
        [DOC_FileName] [nvarchar](255) NOT NULL,
        [DOC_FilePath] [nvarchar](max) NOT NULL,
        [DOC_FileSize] [bigint] NOT NULL,
        [DOC_MimeType] [nvarchar](100) NULL,
        [DOC_CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_Documents_DOC_CreatedAt] DEFAULT (sysutcdatetime()),
        [DOC_UpdatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_Documents_DOC_UpdatedAt] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_Documents_DOC] PRIMARY KEY CLUSTERED ([DOC_Id] ASC)
    );
END
GO

-- 43. EntityDocuments_ED
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EntityDocuments_ED]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[EntityDocuments_ED](
        [ED_Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_EntityDocuments_ED_Id] DEFAULT (newid()),
        [ED_DocumentId] [uniqueidentifier] NOT NULL,
        [ED_EntityType] [nvarchar](50) NOT NULL,
        [ED_EntityId] [uniqueidentifier] NOT NULL,
        [ED_CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_EntityDocuments_ED_CreatedAt] DEFAULT (sysutcdatetime()),
        [ED_UpdatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_EntityDocuments_ED_UpdatedAt] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_EntityDocuments_ED] PRIMARY KEY CLUSTERED ([ED_Id] ASC)
    );
END
GO

-- 44. AdmissionApplications_AA
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AdmissionApplications_AA]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AdmissionApplications_AA](
        [AA_Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_AdmissionApplications_AA_Id] DEFAULT (newid()),
        [AA_TenantId] [uniqueidentifier] NOT NULL,
        [AA_BranchId] [uniqueidentifier] NOT NULL,
        [AA_CourseId] [uniqueidentifier] NULL,
        [AA_ApplicationNumber] [nvarchar](50) NOT NULL,
        [AA_FirstName] [nvarchar](100) NOT NULL,
        [AA_LastName] [nvarchar](100) NOT NULL,
        [AA_Email] [nvarchar](255) NULL,
        [AA_Phone] [nvarchar](30) NULL,
        [AA_Status] [nvarchar](20) NOT NULL CONSTRAINT [DF_AdmissionApplications_AA_Status] DEFAULT ('submitted'),
        [AA_SubmittedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_AdmissionApplications_AA_SubmittedAt] DEFAULT (sysutcdatetime()),
        [AA_CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_AdmissionApplications_AA_CreatedAt] DEFAULT (sysutcdatetime()),
        [AA_UpdatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_AdmissionApplications_AA_UpdatedAt] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_AdmissionApplications_AA] PRIMARY KEY CLUSTERED ([AA_Id] ASC),
        CONSTRAINT [UQ_AdmissionApplications_AA_Tenant_AppNo] UNIQUE NONCLUSTERED ([AA_TenantId] ASC, [AA_ApplicationNumber] ASC)
    );
END
GO

-- 45. AdmissionApplicationDocuments_AAD
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AdmissionApplicationDocuments_AAD]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AdmissionApplicationDocuments_AAD](
        [AAD_Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_AdmissionApplicationDocuments_AAD_Id] DEFAULT (newid()),
        [AAD_ApplicationId] [uniqueidentifier] NOT NULL,
        [AAD_DocumentTypeId] [uniqueidentifier] NOT NULL,
        [AAD_DocumentId] [uniqueidentifier] NULL,
        [AAD_IsVerified] [bit] NOT NULL CONSTRAINT [DF_AdmissionApplicationDocuments_AAD_IsVerified] DEFAULT ((0)),
        [AAD_VerifiedBy] [uniqueidentifier] NULL,
        [AAD_VerifiedAt] [datetime2](7) NULL,
        [AAD_CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_AdmissionApplicationDocuments_AAD_CreatedAt] DEFAULT (sysutcdatetime()),
        [AAD_UpdatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_AdmissionApplicationDocuments_AAD_UpdatedAt] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_AdmissionApplicationDocuments_AAD] PRIMARY KEY CLUSTERED ([AAD_Id] ASC)
    );
END
GO

-- 46. ExpenseCategories_EC
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExpenseCategories_EC]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ExpenseCategories_EC](
        [EC_Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_ExpenseCategories_EC_Id] DEFAULT (newid()),
        [EC_TenantId] [uniqueidentifier] NOT NULL,
        [EC_Name] [nvarchar](150) NOT NULL,
        [EC_Code] [nvarchar](50) NOT NULL,
        [EC_Description] [nvarchar](max) NULL,
        [EC_IsActive] [bit] NOT NULL CONSTRAINT [DF_ExpenseCategories_EC_IsActive] DEFAULT ((1)),
        [EC_CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_ExpenseCategories_EC_CreatedAt] DEFAULT (sysutcdatetime()),
        [EC_UpdatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_ExpenseCategories_EC_UpdatedAt] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_ExpenseCategories_EC] PRIMARY KEY CLUSTERED ([EC_Id] ASC),
        CONSTRAINT [UQ_ExpenseCategories_EC_Tenant_Code] UNIQUE NONCLUSTERED ([EC_TenantId] ASC, [EC_Code] ASC)
    );
END
GO

-- 47. Vendors_V
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Vendors_V]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Vendors_V](
        [V_Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_Vendors_V_Id] DEFAULT (newid()),
        [V_TenantId] [uniqueidentifier] NOT NULL,
        [V_Name] [nvarchar](200) NOT NULL,
        [V_Code] [nvarchar](50) NOT NULL,
        [V_Email] [nvarchar](255) NULL,
        [V_Phone] [nvarchar](30) NULL,
        [V_TaxNumber] [nvarchar](100) NULL,
        [V_Address] [nvarchar](max) NULL,
        [V_IsActive] [bit] NOT NULL CONSTRAINT [DF_Vendors_V_IsActive] DEFAULT ((1)),
        [V_CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_Vendors_V_CreatedAt] DEFAULT (sysutcdatetime()),
        [V_UpdatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_Vendors_V_UpdatedAt] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_Vendors_V] PRIMARY KEY CLUSTERED ([V_Id] ASC),
        CONSTRAINT [UQ_Vendors_V_Tenant_Code] UNIQUE NONCLUSTERED ([V_TenantId] ASC, [V_Code] ASC)
    );
END
GO

-- 48. Expenses_EXP
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Expenses_EXP]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Expenses_EXP](
        [EXP_Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_Expenses_EXP_Id] DEFAULT (newid()),
        [EXP_TenantId] [uniqueidentifier] NOT NULL,
        [EXP_BranchId] [uniqueidentifier] NOT NULL,
        [EXP_ExpenseCategoryId] [uniqueidentifier] NOT NULL,
        [EXP_VendorId] [uniqueidentifier] NULL,
        [EXP_PaymentMethodId] [uniqueidentifier] NULL,
        [EXP_ExpenseNumber] [nvarchar](50) NOT NULL,
        [EXP_Amount] [decimal](18, 2) NOT NULL,
        [EXP_ExpenseDate] [date] NOT NULL,
        [EXP_Description] [nvarchar](max) NULL,
        [EXP_CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_Expenses_EXP_CreatedAt] DEFAULT (sysutcdatetime()),
        [EXP_UpdatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_Expenses_EXP_UpdatedAt] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_Expenses_EXP] PRIMARY KEY CLUSTERED ([EXP_Id] ASC)
    );
END
GO

-- 49. Announcements_ANN
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Announcements_ANN]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Announcements_ANN](
        [ANN_Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_Announcements_ANN_Id] DEFAULT (newid()),
        [ANN_TenantId] [uniqueidentifier] NOT NULL,
        [ANN_BranchId] [uniqueidentifier] NULL,
        [ANN_Title] [nvarchar](200) NOT NULL,
        [ANN_Content] [nvarchar](max) NOT NULL,
        [ANN_TargetAudience] [nvarchar](50) NOT NULL CONSTRAINT [DF_Announcements_ANN_TargetAudience] DEFAULT ('all'),
        [ANN_PublishDate] [datetime2](7) NOT NULL CONSTRAINT [DF_Announcements_ANN_PublishDate] DEFAULT (sysutcdatetime()),
        [ANN_ExpiryDate] [datetime2](7) NULL,
        [ANN_IsActive] [bit] NOT NULL CONSTRAINT [DF_Announcements_ANN_IsActive] DEFAULT ((1)),
        [ANN_CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_Announcements_ANN_CreatedAt] DEFAULT (sysutcdatetime()),
        [ANN_UpdatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_Announcements_ANN_UpdatedAt] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_Announcements_ANN] PRIMARY KEY CLUSTERED ([ANN_Id] ASC)
    );
END
GO

-- 50. NotificationTemplates_NT
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NotificationTemplates_NT]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[NotificationTemplates_NT](
        [NT_Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_NotificationTemplates_NT_Id] DEFAULT (newid()),
        [NT_TenantId] [uniqueidentifier] NOT NULL,
        [NT_Name] [nvarchar](150) NOT NULL,
        [NT_EventKey] [nvarchar](100) NOT NULL,
        [NT_Channel] [nvarchar](20) NOT NULL,
        [NT_Subject] [nvarchar](255) NULL,
        [NT_BodyTemplate] [nvarchar](max) NOT NULL,
        [NT_IsActive] [bit] NOT NULL CONSTRAINT [DF_NotificationTemplates_NT_IsActive] DEFAULT ((1)),
        [NT_CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_NotificationTemplates_NT_CreatedAt] DEFAULT (sysutcdatetime()),
        [NT_UpdatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_NotificationTemplates_NT_UpdatedAt] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_NotificationTemplates_NT] PRIMARY KEY CLUSTERED ([NT_Id] ASC),
        CONSTRAINT [UQ_NotificationTemplates_NT_Tenant_Event_Channel] UNIQUE NONCLUSTERED ([NT_TenantId] ASC, [NT_EventKey] ASC, [NT_Channel] ASC)
    );
END
GO

-- 51. Notifications_N
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Notifications_N]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Notifications_N](
        [N_Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_Notifications_N_Id] DEFAULT (newid()),
        [N_TenantId] [uniqueidentifier] NOT NULL,
        [N_UserId] [uniqueidentifier] NOT NULL,
        [N_Title] [nvarchar](200) NOT NULL,
        [N_Message] [nvarchar](max) NOT NULL,
        [N_Channel] [nvarchar](20) NOT NULL,
        [N_IsRead] [bit] NOT NULL CONSTRAINT [DF_Notifications_N_IsRead] DEFAULT ((0)),
        [N_ReadAt] [datetime2](7) NULL,
        [N_CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_Notifications_N_CreatedAt] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_Notifications_N] PRIMARY KEY CLUSTERED ([N_Id] ASC)
    );
END
GO

-- 52. CustomFields_CF
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CustomFields_CF]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[CustomFields_CF](
        [CF_Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_CustomFields_CF_Id] DEFAULT (newid()),
        [CF_TenantId] [uniqueidentifier] NOT NULL,
        [CF_EntityType] [nvarchar](50) NOT NULL,
        [CF_FieldName] [nvarchar](100) NOT NULL,
        [CF_FieldLabel] [nvarchar](150) NOT NULL,
        [CF_FieldType] [nvarchar](30) NOT NULL,
        [CF_IsRequired] [bit] NOT NULL CONSTRAINT [DF_CustomFields_CF_IsRequired] DEFAULT ((0)),
        [CF_DefaultValue] [nvarchar](max) NULL,
        [CF_IsActive] [bit] NOT NULL CONSTRAINT [DF_CustomFields_CF_IsActive] DEFAULT ((1)),
        [CF_CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_CustomFields_CF_CreatedAt] DEFAULT (sysutcdatetime()),
        [CF_UpdatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_CustomFields_CF_UpdatedAt] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_CustomFields_CF] PRIMARY KEY CLUSTERED ([CF_Id] ASC),
        CONSTRAINT [UQ_CustomFields_CF_Tenant_Entity_Field] UNIQUE NONCLUSTERED ([CF_TenantId] ASC, [CF_EntityType] ASC, [CF_FieldName] ASC)
    );
END
GO

-- 53. CustomFieldValues_CFV
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CustomFieldValues_CFV]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[CustomFieldValues_CFV](
        [CFV_Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_CustomFieldValues_CFV_Id] DEFAULT (newid()),
        [CFV_CustomFieldId] [uniqueidentifier] NOT NULL,
        [CFV_EntityId] [uniqueidentifier] NOT NULL,
        [CFV_FieldValue] [nvarchar](max) NULL,
        [CFV_CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_CustomFieldValues_CFV_CreatedAt] DEFAULT (sysutcdatetime()),
        [CFV_UpdatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_CustomFieldValues_CFV_UpdatedAt] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_CustomFieldValues_CFV] PRIMARY KEY CLUSTERED ([CFV_Id] ASC),
        CONSTRAINT [UQ_CustomFieldValues_CFV_Field_Entity] UNIQUE NONCLUSTERED ([CFV_CustomFieldId] ASC, [CFV_EntityId] ASC)
    );
END
GO

-- 54. ActivityLogs_ACL
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ActivityLogs_ACL]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ActivityLogs_ACL](
        [ACL_Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_ActivityLogs_ACL_Id] DEFAULT (newid()),
        [ACL_TenantId] [uniqueidentifier] NOT NULL,
        [ACL_UserId] [uniqueidentifier] NULL,
        [ACL_ActivityType] [nvarchar](100) NOT NULL,
        [ACL_Description] [nvarchar](max) NOT NULL,
        [ACL_Metadata] [nvarchar](max) NULL,
        [ACL_CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_ActivityLogs_ACL_CreatedAt] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_ActivityLogs_ACL] PRIMARY KEY CLUSTERED ([ACL_Id] ASC)
    );
END
GO

-- 55. AuditLogs_AL
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AuditLogs_AL]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AuditLogs_AL](
        [AL_Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_AuditLogs_AL_Id] DEFAULT (newid()),
        [AL_TenantId] [uniqueidentifier] NOT NULL,
        [AL_UserId] [uniqueidentifier] NULL,
        [AL_TableName] [nvarchar](100) NOT NULL,
        [AL_EntityId] [uniqueidentifier] NOT NULL,
        [AL_ActionType] [nvarchar](20) NOT NULL,
        [AL_OldValues] [nvarchar](max) NULL,
        [AL_NewValues] [nvarchar](max) NULL,
        [AL_CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_AuditLogs_AL_CreatedAt] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_AuditLogs_AL] PRIMARY KEY CLUSTERED ([AL_Id] ASC)
    );
END
GO

-- 56. BankMaster_BM
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BankMaster_BM]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[BankMaster_BM](
        [BM_Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_BankMaster_BM_Id] DEFAULT (newid()),
        [BM_TenantId] [uniqueidentifier] NULL,
        [BM_BankName] [nvarchar](200) NOT NULL,
        [BM_AccountNo] [nvarchar](50) NULL,
        [BM_IFSCCode] [nvarchar](20) NULL,
        [BM_BranchName] [nvarchar](200) NULL,
        [BM_IsActive] [bit] NOT NULL CONSTRAINT [DF_BankMaster_BM_IsActive] DEFAULT ((1)),
        [BM_CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_BankMaster_BM_CreatedAt] DEFAULT (sysutcdatetime()),
        [BM_CreatedBy] [uniqueidentifier] NULL,
        [BM_UpdatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_BankMaster_BM_UpdatedAt] DEFAULT (sysutcdatetime()),
        [BM_UpdatedBy] [uniqueidentifier] NULL,
        CONSTRAINT [PK_BankMaster_BM] PRIMARY KEY CLUSTERED ([BM_Id] ASC)
    );
END
GO

-- =========================================================================================
-- FOREIGN KEY CONSTRAINTS
-- =========================================================================================

ALTER TABLE [dbo].[Branches_B] WITH CHECK ADD CONSTRAINT [FK_Branches_B_TenantId] FOREIGN KEY([B_TenantId]) REFERENCES [dbo].[Organizations_O] ([O_Id]) ON DELETE CASCADE;
ALTER TABLE [dbo].[AcademicYears_AY] WITH CHECK ADD CONSTRAINT [FK_AcademicYears_AY_TenantId] FOREIGN KEY([AY_TenantId]) REFERENCES [dbo].[Organizations_O] ([O_Id]) ON DELETE CASCADE;
ALTER TABLE [dbo].[Programs_P] WITH CHECK ADD CONSTRAINT [FK_Programs_P_TenantId] FOREIGN KEY([P_TenantId]) REFERENCES [dbo].[Organizations_O] ([O_Id]) ON DELETE CASCADE;
ALTER TABLE [dbo].[Courses_C] WITH CHECK ADD CONSTRAINT [FK_Courses_C_TenantId] FOREIGN KEY([C_TenantId]) REFERENCES [dbo].[Organizations_O] ([O_Id]) ON DELETE CASCADE;
ALTER TABLE [dbo].[Courses_C] WITH CHECK ADD CONSTRAINT [FK_Courses_C_ProgramId] FOREIGN KEY([C_ProgramId]) REFERENCES [dbo].[Programs_P] ([P_Id]);
ALTER TABLE [dbo].[Subjects_SB] WITH CHECK ADD CONSTRAINT [FK_Subjects_SB_TenantId] FOREIGN KEY([SB_TenantId]) REFERENCES [dbo].[Organizations_O] ([O_Id]) ON DELETE CASCADE;
ALTER TABLE [dbo].[CourseSubjects_CS] WITH CHECK ADD CONSTRAINT [FK_CourseSubjects_CS_CourseId] FOREIGN KEY([CS_CourseId]) REFERENCES [dbo].[Courses_C] ([C_Id]) ON DELETE CASCADE;
ALTER TABLE [dbo].[CourseSubjects_CS] WITH CHECK ADD CONSTRAINT [FK_CourseSubjects_CS_SubjectId] FOREIGN KEY([CS_SubjectId]) REFERENCES [dbo].[Subjects_SB] ([SB_Id]);
ALTER TABLE [dbo].[CourseSubjects_CS] WITH CHECK ADD CONSTRAINT [FK_CourseSubjects_CS_AcademicYearId] FOREIGN KEY([CS_AcademicYearId]) REFERENCES [dbo].[AcademicYears_AY] ([AY_Id]);

ALTER TABLE [dbo].[Batches_BT] WITH CHECK ADD CONSTRAINT [FK_Batches_BT_TenantId] FOREIGN KEY([BT_TenantId]) REFERENCES [dbo].[Organizations_O] ([O_Id]);
ALTER TABLE [dbo].[Batches_BT] WITH CHECK ADD CONSTRAINT [FK_Batches_BT_BranchId] FOREIGN KEY([BT_BranchId]) REFERENCES [dbo].[Branches_B] ([B_Id]);
ALTER TABLE [dbo].[Batches_BT] WITH CHECK ADD CONSTRAINT [FK_Batches_BT_CourseId] FOREIGN KEY([BT_CourseId]) REFERENCES [dbo].[Courses_C] ([C_Id]);
ALTER TABLE [dbo].[Batches_BT] WITH CHECK ADD CONSTRAINT [FK_Batches_BT_AcademicYearId] FOREIGN KEY([BT_AcademicYearId]) REFERENCES [dbo].[AcademicYears_AY] ([AY_Id]);

ALTER TABLE [dbo].[Students_S] WITH CHECK ADD CONSTRAINT [FK_Students_S_TenantId] FOREIGN KEY([S_TenantId]) REFERENCES [dbo].[Organizations_O] ([O_Id]);
ALTER TABLE [dbo].[Students_S] WITH CHECK ADD CONSTRAINT [FK_Students_S_BranchId] FOREIGN KEY([S_BranchId]) REFERENCES [dbo].[Branches_B] ([B_Id]);

ALTER TABLE [dbo].[Enrollments_E] WITH CHECK ADD CONSTRAINT [FK_Enrollments_E_StudentId] FOREIGN KEY([E_StudentId]) REFERENCES [dbo].[Students_S] ([S_Id]) ON DELETE CASCADE;
ALTER TABLE [dbo].[Enrollments_E] WITH CHECK ADD CONSTRAINT [FK_Enrollments_E_AcademicYearId] FOREIGN KEY([E_AcademicYearId]) REFERENCES [dbo].[AcademicYears_AY] ([AY_Id]);
ALTER TABLE [dbo].[Enrollments_E] WITH CHECK ADD CONSTRAINT [FK_Enrollments_E_CourseId] FOREIGN KEY([E_CourseId]) REFERENCES [dbo].[Courses_C] ([C_Id]);
ALTER TABLE [dbo].[Enrollments_E] WITH CHECK ADD CONSTRAINT [FK_Enrollments_E_BatchId] FOREIGN KEY([E_BatchId]) REFERENCES [dbo].[Batches_BT] ([BT_Id]);

ALTER TABLE [dbo].[BatchStudents_BS] WITH CHECK ADD CONSTRAINT [FK_BatchStudents_BS_BatchId] FOREIGN KEY([BS_BatchId]) REFERENCES [dbo].[Batches_BT] ([BT_Id]) ON DELETE CASCADE;
ALTER TABLE [dbo].[BatchStudents_BS] WITH CHECK ADD CONSTRAINT [FK_BatchStudents_BS_StudentId] FOREIGN KEY([BS_StudentId]) REFERENCES [dbo].[Students_S] ([S_Id]);

ALTER TABLE [dbo].[Guardians_G] WITH CHECK ADD CONSTRAINT [FK_Guardians_G_TenantId] FOREIGN KEY([G_TenantId]) REFERENCES [dbo].[Organizations_O] ([O_Id]);
ALTER TABLE [dbo].[StudentGuardians_SG] WITH CHECK ADD CONSTRAINT [FK_StudentGuardians_SG_StudentId] FOREIGN KEY([SG_StudentId]) REFERENCES [dbo].[Students_S] ([S_Id]) ON DELETE CASCADE;
ALTER TABLE [dbo].[StudentGuardians_SG] WITH CHECK ADD CONSTRAINT [FK_StudentGuardians_SG_GuardianId] FOREIGN KEY([SG_GuardianId]) REFERENCES [dbo].[Guardians_G] ([G_Id]) ON DELETE CASCADE;

ALTER TABLE [dbo].[Departments_D] WITH CHECK ADD CONSTRAINT [FK_Departments_D_TenantId] FOREIGN KEY([D_TenantId]) REFERENCES [dbo].[Organizations_O] ([O_Id]);
ALTER TABLE [dbo].[Departments_D] WITH CHECK ADD CONSTRAINT [FK_Departments_D_BranchId] FOREIGN KEY([D_BranchId]) REFERENCES [dbo].[Branches_B] ([B_Id]);
ALTER TABLE [dbo].[Designations_DS] WITH CHECK ADD CONSTRAINT [FK_Designations_DS_TenantId] FOREIGN KEY([DS_TenantId]) REFERENCES [dbo].[Organizations_O] ([O_Id]);

-- Staff_ST Foreign Keys (per database_doc.md)
ALTER TABLE [dbo].[Staff_ST] WITH CHECK ADD CONSTRAINT [FK_Staff_ST_TenantId] FOREIGN KEY([ST_TenantId]) REFERENCES [dbo].[Organizations_O] ([O_Id]) ON DELETE CASCADE;
ALTER TABLE [dbo].[Staff_ST] WITH CHECK ADD CONSTRAINT [FK_Staff_ST_BranchId] FOREIGN KEY([ST_BranchId]) REFERENCES [dbo].[Branches_B] ([B_Id]);
ALTER TABLE [dbo].[Staff_ST] WITH CHECK ADD CONSTRAINT [FK_Staff_ST_DepartmentId] FOREIGN KEY([ST_DepartmentId]) REFERENCES [dbo].[Departments_D] ([D_Id]);
ALTER TABLE [dbo].[Staff_ST] WITH CHECK ADD CONSTRAINT [FK_Staff_ST_DesignationId] FOREIGN KEY([ST_DesignationId]) REFERENCES [dbo].[Designations_DS] ([DS_Id]);

ALTER TABLE [dbo].[Classrooms_CR] WITH CHECK ADD CONSTRAINT [FK_Classrooms_CR_BranchId] FOREIGN KEY([CR_BranchId]) REFERENCES [dbo].[Branches_B] ([B_Id]);
ALTER TABLE [dbo].[Timetables_TT] WITH CHECK ADD CONSTRAINT [FK_Timetables_TT_BatchId] FOREIGN KEY([TT_BatchId]) REFERENCES [dbo].[Batches_BT] ([BT_Id]);
ALTER TABLE [dbo].[Timetables_TT] WITH CHECK ADD CONSTRAINT [FK_Timetables_TT_SubjectId] FOREIGN KEY([TT_SubjectId]) REFERENCES [dbo].[Subjects_SB] ([SB_Id]);
ALTER TABLE [dbo].[Timetables_TT] WITH CHECK ADD CONSTRAINT [FK_Timetables_TT_StaffId] FOREIGN KEY([TT_StaffId]) REFERENCES [dbo].[Staff_ST] ([ST_Id]);
ALTER TABLE [dbo].[Timetables_TT] WITH CHECK ADD CONSTRAINT [FK_Timetables_TT_ClassroomId] FOREIGN KEY([TT_ClassroomId]) REFERENCES [dbo].[Classrooms_CR] ([CR_Id]);

ALTER TABLE [dbo].[AttendanceSessions_AS] WITH CHECK ADD CONSTRAINT [FK_AttendanceSessions_AS_BatchId] FOREIGN KEY([AS_BatchId]) REFERENCES [dbo].[Batches_BT] ([BT_Id]);
ALTER TABLE [dbo].[AttendanceRecords_AR] WITH CHECK ADD CONSTRAINT [FK_AttendanceRecords_AR_SessionId] FOREIGN KEY([AR_SessionId]) REFERENCES [dbo].[AttendanceSessions_AS] ([AS_Id]) ON DELETE CASCADE;
ALTER TABLE [dbo].[AttendanceRecords_AR] WITH CHECK ADD CONSTRAINT [FK_AttendanceRecords_AR_StudentId] FOREIGN KEY([AR_StudentId]) REFERENCES [dbo].[Students_S] ([S_Id]);

ALTER TABLE [dbo].[Exams_EX] WITH CHECK ADD CONSTRAINT [FK_Exams_EX_BatchId] FOREIGN KEY([EX_BatchId]) REFERENCES [dbo].[Batches_BT] ([BT_Id]);
ALTER TABLE [dbo].[Exams_EX] WITH CHECK ADD CONSTRAINT [FK_Exams_EX_ExamTypeId] FOREIGN KEY([EX_ExamTypeId]) REFERENCES [dbo].[ExamTypes_ET] ([ET_Id]);
ALTER TABLE [dbo].[ExamSubjects_ES] WITH CHECK ADD CONSTRAINT [FK_ExamSubjects_ES_ExamId] FOREIGN KEY([ES_ExamId]) REFERENCES [dbo].[Exams_EX] ([EX_Id]) ON DELETE CASCADE;
ALTER TABLE [dbo].[ExamSubjects_ES] WITH CHECK ADD CONSTRAINT [FK_ExamSubjects_ES_SubjectId] FOREIGN KEY([ES_SubjectId]) REFERENCES [dbo].[Subjects_SB] ([SB_Id]);
ALTER TABLE [dbo].[ExamSchedules_ESC] WITH CHECK ADD CONSTRAINT [FK_ExamSchedules_ESC_ExamSubjectId] FOREIGN KEY([ESC_ExamSubjectId]) REFERENCES [dbo].[ExamSubjects_ES] ([ES_Id]) ON DELETE CASCADE;
ALTER TABLE [dbo].[ExamSchedules_ESC] WITH CHECK ADD CONSTRAINT [FK_ExamSchedules_ESC_ClassroomId] FOREIGN KEY([ESC_ClassroomId]) REFERENCES [dbo].[Classrooms_CR] ([CR_Id]);

ALTER TABLE [dbo].[GradeScaleItems_GSI] WITH CHECK ADD CONSTRAINT [FK_GradeScaleItems_GSI_GradeScaleId] FOREIGN KEY([GSI_GradeScaleId]) REFERENCES [dbo].[GradeScales_GS] ([GS_Id]) ON DELETE CASCADE;
ALTER TABLE [dbo].[Marks_M] WITH CHECK ADD CONSTRAINT [FK_Marks_M_ExamSubjectId] FOREIGN KEY([M_ExamSubjectId]) REFERENCES [dbo].[ExamSubjects_ES] ([ES_Id]) ON DELETE CASCADE;
ALTER TABLE [dbo].[Marks_M] WITH CHECK ADD CONSTRAINT [FK_Marks_M_StudentId] FOREIGN KEY([M_StudentId]) REFERENCES [dbo].[Students_S] ([S_Id]);
ALTER TABLE [dbo].[Results_R] WITH CHECK ADD CONSTRAINT [FK_Results_R_ExamId] FOREIGN KEY([R_ExamId]) REFERENCES [dbo].[Exams_EX] ([EX_Id]) ON DELETE CASCADE;
ALTER TABLE [dbo].[Results_R] WITH CHECK ADD CONSTRAINT [FK_Results_R_StudentId] FOREIGN KEY([R_StudentId]) REFERENCES [dbo].[Students_S] ([S_Id]);

ALTER TABLE [dbo].[FeeStructures_FS] WITH CHECK ADD CONSTRAINT [FK_FeeStructures_FS_AcademicYearId] FOREIGN KEY([FS_AcademicYearId]) REFERENCES [dbo].[AcademicYears_AY] ([AY_Id]);
ALTER TABLE [dbo].[FeeStructureItems_FSI] WITH CHECK ADD CONSTRAINT [FK_FeeStructureItems_FSI_FeeStructureId] FOREIGN KEY([FSI_FeeStructureId]) REFERENCES [dbo].[FeeStructures_FS] ([FS_Id]) ON DELETE CASCADE;
ALTER TABLE [dbo].[FeeStructureItems_FSI] WITH CHECK ADD CONSTRAINT [FK_FeeStructureItems_FSI_FeeCategoryId] FOREIGN KEY([FSI_FeeCategoryId]) REFERENCES [dbo].[FeeCategories_FC] ([FC_Id]);

ALTER TABLE [dbo].[StudentFeeAssignments_SFA] WITH CHECK ADD CONSTRAINT [FK_StudentFeeAssignments_SFA_StudentId] FOREIGN KEY([SFA_StudentId]) REFERENCES [dbo].[Students_S] ([S_Id]);
ALTER TABLE [dbo].[StudentFeeAssignments_SFA] WITH CHECK ADD CONSTRAINT [FK_StudentFeeAssignments_SFA_FeeStructureId] FOREIGN KEY([SFA_FeeStructureId]) REFERENCES [dbo].[FeeStructures_FS] ([FS_Id]);
ALTER TABLE [dbo].[StudentFeeAssignments_SFA] WITH CHECK ADD CONSTRAINT [FK_StudentFeeAssignments_SFA_DiscountId] FOREIGN KEY([SFA_DiscountId]) REFERENCES [dbo].[Discounts_DIS] ([DIS_Id]);

ALTER TABLE [dbo].[FeeInvoices_FI] WITH CHECK ADD CONSTRAINT [FK_FeeInvoices_FI_StudentId] FOREIGN KEY([FI_StudentId]) REFERENCES [dbo].[Students_S] ([S_Id]);
ALTER TABLE [dbo].[FeeInvoices_FI] WITH CHECK ADD CONSTRAINT [FK_FeeInvoices_FI_StudentFeeAssignmentId] FOREIGN KEY([FI_StudentFeeAssignmentId]) REFERENCES [dbo].[StudentFeeAssignments_SFA] ([SFA_Id]);

ALTER TABLE [dbo].[FeeInvoiceItems_FII] WITH CHECK ADD CONSTRAINT [FK_FeeInvoiceItems_FII_InvoiceId] FOREIGN KEY([FII_InvoiceId]) REFERENCES [dbo].[FeeInvoices_FI] ([FI_Id]) ON DELETE CASCADE;
ALTER TABLE [dbo].[FeeInvoiceItems_FII] WITH CHECK ADD CONSTRAINT [FK_FeeInvoiceItems_FII_FeeCategoryId] FOREIGN KEY([FII_FeeCategoryId]) REFERENCES [dbo].[FeeCategories_FC] ([FC_Id]);
ALTER TABLE [dbo].[FeeInvoiceItems_FII] WITH CHECK ADD CONSTRAINT [FK_FeeInvoiceItems_FII_DiscountId] FOREIGN KEY([FII_DiscountId]) REFERENCES [dbo].[Discounts_DIS] ([DIS_Id]);

ALTER TABLE [dbo].[Payments_PAY] WITH CHECK ADD CONSTRAINT [FK_Payments_PAY_StudentId] FOREIGN KEY([PAY_StudentId]) REFERENCES [dbo].[Students_S] ([S_Id]);
ALTER TABLE [dbo].[Payments_PAY] WITH CHECK ADD CONSTRAINT [FK_Payments_PAY_PaymentMethodId] FOREIGN KEY([PAY_PaymentMethodId]) REFERENCES [dbo].[PaymentMethods_PM] ([PM_Id]);
ALTER TABLE [dbo].[PaymentAllocations_PA] WITH CHECK ADD CONSTRAINT [FK_PaymentAllocations_PA_PaymentId] FOREIGN KEY([PA_PaymentId]) REFERENCES [dbo].[Payments_PAY] ([PAY_Id]) ON DELETE CASCADE;
ALTER TABLE [dbo].[PaymentAllocations_PA] WITH CHECK ADD CONSTRAINT [FK_PaymentAllocations_PA_InvoiceId] FOREIGN KEY([PA_InvoiceId]) REFERENCES [dbo].[FeeInvoices_FI] ([FI_Id]);
ALTER TABLE [dbo].[Refunds_RF] WITH CHECK ADD CONSTRAINT [FK_Refunds_RF_PaymentId] FOREIGN KEY([RF_PaymentId]) REFERENCES [dbo].[Payments_PAY] ([PAY_Id]);

ALTER TABLE [dbo].[Documents_DOC] WITH CHECK ADD CONSTRAINT [FK_Documents_DOC_DocumentTypeId] FOREIGN KEY([DOC_DocumentTypeId]) REFERENCES [dbo].[DocumentTypes_DT] ([DT_Id]);
ALTER TABLE [dbo].[EntityDocuments_ED] WITH CHECK ADD CONSTRAINT [FK_EntityDocuments_ED_DocumentId] FOREIGN KEY([ED_DocumentId]) REFERENCES [dbo].[Documents_DOC] ([DOC_Id]) ON DELETE CASCADE;
ALTER TABLE [dbo].[AdmissionApplicationDocuments_AAD] WITH CHECK ADD CONSTRAINT [FK_AdmissionApplicationDocuments_AAD_ApplicationId] FOREIGN KEY([AAD_ApplicationId]) REFERENCES [dbo].[AdmissionApplications_AA] ([AA_Id]) ON DELETE CASCADE;
ALTER TABLE [dbo].[AdmissionApplicationDocuments_AAD] WITH CHECK ADD CONSTRAINT [FK_AdmissionApplicationDocuments_AAD_DocumentTypeId] FOREIGN KEY([AAD_DocumentTypeId]) REFERENCES [dbo].[DocumentTypes_DT] ([DT_Id]);

ALTER TABLE [dbo].[Expenses_EXP] WITH CHECK ADD CONSTRAINT [FK_Expenses_EXP_ExpenseCategoryId] FOREIGN KEY([EXP_ExpenseCategoryId]) REFERENCES [dbo].[ExpenseCategories_EC] ([EC_Id]);
ALTER TABLE [dbo].[Expenses_EXP] WITH CHECK ADD CONSTRAINT [FK_Expenses_EXP_VendorId] FOREIGN KEY([EXP_VendorId]) REFERENCES [dbo].[Vendors_V] ([V_Id]);
ALTER TABLE [dbo].[Expenses_EXP] WITH CHECK ADD CONSTRAINT [FK_Expenses_EXP_PaymentMethodId] FOREIGN KEY([EXP_PaymentMethodId]) REFERENCES [dbo].[PaymentMethods_PM] ([PM_Id]);

ALTER TABLE [dbo].[CustomFieldValues_CFV] WITH CHECK ADD CONSTRAINT [FK_CustomFieldValues_CFV_CustomFieldId] FOREIGN KEY([CFV_CustomFieldId]) REFERENCES [dbo].[CustomFields_CF] ([CF_Id]) ON DELETE CASCADE;
GO