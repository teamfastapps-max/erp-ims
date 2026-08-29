USE [IMS]
GO

-- =========================================================================================
-- Master Module Stored Procedures (USP) for MasterConfigRegistry
-- Each SP is a multi-action dispatcher: GetAll | GetById | Insert | Update | Deactivate | ExistsByField
-- Common params (@TenantId, @CreatedBy, @UpdatedBy) match MasterDAL convention (no table prefix)
-- =========================================================================================

-- 1. USP_Bank (BankMaster_BM)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[USP_Bank]') AND type in (N'P'))
    DROP PROCEDURE [dbo].[USP_Bank];
GO
CREATE PROCEDURE [dbo].[USP_Bank]
    @Action NVARCHAR(20),
    @BM_Id INT = NULL,
    @BM_BankName NVARCHAR(200) = NULL,
    @BM_AccountNo NVARCHAR(50) = NULL,
    @BM_IFSCCode NVARCHAR(20) = NULL,
    @BM_BranchName NVARCHAR(200) = NULL,
    @BM_IsActive BIT = NULL,
    @TenantId UNIQUEIDENTIFIER = NULL,
    @CreatedBy UNIQUEIDENTIFIER = NULL,
    @UpdatedBy UNIQUEIDENTIFIER = NULL,
    @ColumnName NVARCHAR(100) = NULL,
    @Value NVARCHAR(MAX) = NULL,
    @ExcludeId INT = NULL,
    @NewId INT = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF @Action = 'GetAll'
    BEGIN
        SELECT BM_Id, BM_TenantId, BM_BankName, BM_AccountNo, BM_IFSCCode, BM_BranchName,
               BM_IsActive, BM_CreatedAt, BM_CreatedBy, BM_UpdatedAt, BM_UpdatedBy
        FROM dbo.BankMaster_BM
        WHERE BM_IsActive = 1
        ORDER BY BM_BankName;
    END

    ELSE IF @Action = 'GetById'
    BEGIN
        SELECT BM_Id, BM_TenantId, BM_BankName, BM_AccountNo, BM_IFSCCode, BM_BranchName,
               BM_IsActive, BM_CreatedAt, BM_CreatedBy, BM_UpdatedAt, BM_UpdatedBy
        FROM dbo.BankMaster_BM
        WHERE BM_Id = @BM_Id;
    END

    ELSE IF @Action = 'Insert'
    BEGIN
        INSERT INTO dbo.BankMaster_BM (BM_TenantId, BM_BankName, BM_AccountNo, BM_IFSCCode, BM_BranchName, BM_IsActive, BM_CreatedBy, BM_UpdatedBy)
        VALUES (@TenantId, @BM_BankName, @BM_AccountNo, @BM_IFSCCode, @BM_BranchName, ISNULL(@BM_IsActive, 1), @CreatedBy, @UpdatedBy);

        SET @NewId = SCOPE_IDENTITY();
    END

    ELSE IF @Action = 'Update'
    BEGIN
        UPDATE dbo.BankMaster_BM
        SET BM_BankName = @BM_BankName,
            BM_AccountNo = @BM_AccountNo,
            BM_IFSCCode = @BM_IFSCCode,
            BM_BranchName = @BM_BranchName,
            BM_IsActive = @BM_IsActive,
            BM_UpdatedAt = GETUTCDATE(),
            BM_UpdatedBy = @UpdatedBy
        WHERE BM_Id = @BM_Id;
    END

    ELSE IF @Action = 'Deactivate'
    BEGIN
        UPDATE dbo.BankMaster_BM
        SET BM_IsActive = 0, BM_UpdatedAt = GETUTCDATE()
        WHERE BM_Id = @BM_Id;
    END

    ELSE IF @Action = 'ExistsByField'
    BEGIN
        DECLARE @Sql NVARCHAR(MAX);
        SET @Sql = N'SELECT COUNT(1) FROM dbo.BankMaster_BM WHERE ' + QUOTENAME(@ColumnName) + N' = @P_Val AND BM_IsActive = 1';
        IF @ExcludeId IS NOT NULL
            SET @Sql = @Sql + N' AND BM_Id <> @P_ExId';
        EXEC sp_executesql @Sql, N'@P_Val NVARCHAR(MAX), @P_ExId INT', @Value, @ExcludeId;
    END
END
GO

-- 2. USP_Branches_B (Branches_B)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[USP_Branches_B]') AND type in (N'P'))
    DROP PROCEDURE [dbo].[USP_Branches_B];
GO
CREATE PROCEDURE [dbo].[USP_Branches_B]
    @Action NVARCHAR(20),
    @B_Id UNIQUEIDENTIFIER = NULL,
    @B_Name NVARCHAR(200) = NULL,
    @B_Code NVARCHAR(50) = NULL,
    @B_Email NVARCHAR(255) = NULL,
    @B_Phone NVARCHAR(30) = NULL,
    @B_AddressLine1 NVARCHAR(255) = NULL,
    @B_AddressLine2 NVARCHAR(255) = NULL,
    @B_City NVARCHAR(100) = NULL,
    @B_State NVARCHAR(100) = NULL,
    @B_PostalCode NVARCHAR(20) = NULL,
    @B_CountryCode CHAR(2) = NULL,
    @TenantId UNIQUEIDENTIFIER = NULL,
    @CreatedBy UNIQUEIDENTIFIER = NULL,
    @UpdatedBy UNIQUEIDENTIFIER = NULL,
    @ColumnName NVARCHAR(100) = NULL,
    @Value NVARCHAR(MAX) = NULL,
    @ExcludeId UNIQUEIDENTIFIER = NULL,
    @NewId UNIQUEIDENTIFIER = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF @Action = 'GetAll'
    BEGIN
        SELECT B_Id, B_TenantId, B_Name, B_Code, B_Email, B_Phone,
               B_AddressLine1, B_AddressLine2, B_City, B_State, B_PostalCode, B_CountryCode,
               B_Status, B_CreatedAt, B_UpdatedAt
        FROM dbo.Branches_B
        WHERE B_Status = 'active' AND B_DeletedAt IS NULL
        ORDER BY B_Name;
    END

    ELSE IF @Action = 'GetById'
    BEGIN
        SELECT B_Id, B_TenantId, B_Name, B_Code, B_Email, B_Phone,
               B_AddressLine1, B_AddressLine2, B_City, B_State, B_PostalCode, B_CountryCode,
               B_Status, B_CreatedAt, B_UpdatedAt
        FROM dbo.Branches_B
        WHERE B_Id = @B_Id;
    END

    ELSE IF @Action = 'Insert'
    BEGIN
        SET @NewId = NEWID();
        INSERT INTO dbo.Branches_B (B_Id, B_TenantId, B_Name, B_Code, B_Email, B_Phone,
            B_AddressLine1, B_AddressLine2, B_City, B_State, B_PostalCode, B_CountryCode, B_Status)
        VALUES (@NewId, @TenantId, @B_Name, @B_Code, @B_Email, @B_Phone,
            @B_AddressLine1, @B_AddressLine2, @B_City, @B_State, @B_PostalCode, @B_CountryCode, 'active');
    END

    ELSE IF @Action = 'Update'
    BEGIN
        UPDATE dbo.Branches_B
        SET B_Name = @B_Name,
            B_Code = @B_Code,
            B_Email = @B_Email,
            B_Phone = @B_Phone,
            B_AddressLine1 = @B_AddressLine1,
            B_AddressLine2 = @B_AddressLine2,
            B_City = @B_City,
            B_State = @B_State,
            B_PostalCode = @B_PostalCode,
            B_CountryCode = @B_CountryCode,
            B_UpdatedAt = SYSUTCDATETIME()
        WHERE B_Id = @B_Id;
    END

    ELSE IF @Action = 'Deactivate'
    BEGIN
        UPDATE dbo.Branches_B
        SET B_Status = 'inactive', B_DeletedAt = SYSUTCDATETIME(), B_UpdatedAt = SYSUTCDATETIME()
        WHERE B_Id = @B_Id;
    END

    ELSE IF @Action = 'ExistsByField'
    BEGIN
        DECLARE @Sql NVARCHAR(MAX);
        SET @Sql = N'SELECT COUNT(1) FROM dbo.Branches_B WHERE ' + QUOTENAME(@ColumnName) + N' = @P_Val AND B_Status = ''active'' AND B_DeletedAt IS NULL';
        IF @ExcludeId IS NOT NULL
            SET @Sql = @Sql + N' AND B_Id <> @P_ExId';
        EXEC sp_executesql @Sql, N'@P_Val NVARCHAR(MAX), @P_ExId UNIQUEIDENTIFIER', @Value, @ExcludeId;
    END
END
GO

-- 3. USP_Courses_C (Courses_C)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[USP_Courses_C]') AND type in (N'P'))
    DROP PROCEDURE [dbo].[USP_Courses_C];
GO
CREATE PROCEDURE [dbo].[USP_Courses_C]
    @Action NVARCHAR(20),
    @C_Id UNIQUEIDENTIFIER = NULL,
    @C_Name NVARCHAR(200) = NULL,
    @C_Code NVARCHAR(50) = NULL,
    @C_ProgramId UNIQUEIDENTIFIER = NULL,
    @C_Description NVARCHAR(MAX) = NULL,
    @TenantId UNIQUEIDENTIFIER = NULL,
    @CreatedBy UNIQUEIDENTIFIER = NULL,
    @UpdatedBy UNIQUEIDENTIFIER = NULL,
    @ColumnName NVARCHAR(100) = NULL,
    @Value NVARCHAR(MAX) = NULL,
    @ExcludeId UNIQUEIDENTIFIER = NULL,
    @NewId UNIQUEIDENTIFIER = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF @Action = 'GetAll'
    BEGIN
        SELECT C_Id, C_TenantId, C_ProgramId, C_Name, C_Code,
               C_Description, C_Status, C_CreatedAt, C_UpdatedAt
        FROM dbo.Courses_C
        WHERE C_Status = 'active' AND C_DeletedAt IS NULL
        ORDER BY C_Name;
    END

    ELSE IF @Action = 'GetById'
    BEGIN
        SELECT C_Id, C_TenantId, C_ProgramId, C_Name, C_Code,
               C_Description, C_Status, C_CreatedAt, C_UpdatedAt
        FROM dbo.Courses_C
        WHERE C_Id = @C_Id;
    END

    ELSE IF @Action = 'Insert'
    BEGIN
        SET @NewId = NEWID();
        INSERT INTO dbo.Courses_C (C_Id, C_TenantId, C_ProgramId, C_Name, C_Code, C_Description, C_Status)
        VALUES (@NewId, @TenantId, @C_ProgramId, @C_Name, @C_Code, @C_Description, 'active');
    END

    ELSE IF @Action = 'Update'
    BEGIN
        UPDATE dbo.Courses_C
        SET C_ProgramId = @C_ProgramId,
            C_Name = @C_Name,
            C_Code = @C_Code,
            C_Description = @C_Description,
            C_UpdatedAt = SYSUTCDATETIME()
        WHERE C_Id = @C_Id;
    END

    ELSE IF @Action = 'Deactivate'
    BEGIN
        UPDATE dbo.Courses_C
        SET C_Status = 'inactive', C_DeletedAt = SYSUTCDATETIME(), C_UpdatedAt = SYSUTCDATETIME()
        WHERE C_Id = @C_Id;
    END

    ELSE IF @Action = 'ExistsByField'
    BEGIN
        DECLARE @Sql NVARCHAR(MAX);
        SET @Sql = N'SELECT COUNT(1) FROM dbo.Courses_C WHERE ' + QUOTENAME(@ColumnName) + N' = @P_Val AND C_Status = ''active'' AND C_DeletedAt IS NULL';
        IF @ExcludeId IS NOT NULL
            SET @Sql = @Sql + N' AND C_Id <> @P_ExId';
        EXEC sp_executesql @Sql, N'@P_Val NVARCHAR(MAX), @P_ExId UNIQUEIDENTIFIER', @Value, @ExcludeId;
    END
END
GO

-- 4. USP_AcademicYears_AY (AcademicYears_AY)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[USP_AcademicYears_AY]') AND type in (N'P'))
    DROP PROCEDURE [dbo].[USP_AcademicYears_AY];
GO
CREATE PROCEDURE [dbo].[USP_AcademicYears_AY]
    @Action NVARCHAR(20),
    @AY_Id UNIQUEIDENTIFIER = NULL,
    @AY_Name NVARCHAR(100) = NULL,
    @AY_Code NVARCHAR(50) = NULL,
    @AY_StartDate DATE = NULL,
    @AY_EndDate DATE = NULL,
    @AY_IsCurrent BIT = NULL,
    @TenantId UNIQUEIDENTIFIER = NULL,
    @CreatedBy UNIQUEIDENTIFIER = NULL,
    @UpdatedBy UNIQUEIDENTIFIER = NULL,
    @ColumnName NVARCHAR(100) = NULL,
    @Value NVARCHAR(MAX) = NULL,
    @ExcludeId UNIQUEIDENTIFIER = NULL,
    @NewId UNIQUEIDENTIFIER = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF @Action = 'GetAll'
    BEGIN
        SELECT AY_Id, AY_TenantId, AY_Name, AY_Code, AY_StartDate, AY_EndDate,
               AY_IsCurrent, AY_CreatedAt, AY_UpdatedAt
        FROM dbo.AcademicYears_AY
        ORDER BY AY_StartDate DESC;
    END

    ELSE IF @Action = 'GetById'
    BEGIN
        SELECT AY_Id, AY_TenantId, AY_Name, AY_Code, AY_StartDate, AY_EndDate,
               AY_IsCurrent, AY_CreatedAt, AY_UpdatedAt
        FROM dbo.AcademicYears_AY
        WHERE AY_Id = @AY_Id;
    END

    ELSE IF @Action = 'Insert'
    BEGIN
        SET @NewId = NEWID();
        INSERT INTO dbo.AcademicYears_AY (AY_Id, AY_TenantId, AY_Name, AY_Code, AY_StartDate, AY_EndDate, AY_IsCurrent)
        VALUES (@NewId, @TenantId, @AY_Name, @AY_Code, @AY_StartDate, @AY_EndDate, ISNULL(@AY_IsCurrent, 0));
    END

    ELSE IF @Action = 'Update'
    BEGIN
        UPDATE dbo.AcademicYears_AY
        SET AY_Name = @AY_Name,
            AY_Code = @AY_Code,
            AY_StartDate = @AY_StartDate,
            AY_EndDate = @AY_EndDate,
            AY_IsCurrent = @AY_IsCurrent,
            AY_UpdatedAt = SYSUTCDATETIME()
        WHERE AY_Id = @AY_Id;
    END

    ELSE IF @Action = 'Deactivate'
    BEGIN
        RAISERROR('AcademicYears_AY does not support soft delete.', 16, 1);
    END

    ELSE IF @Action = 'ExistsByField'
    BEGIN
        DECLARE @Sql NVARCHAR(MAX);
        SET @Sql = N'SELECT COUNT(1) FROM dbo.AcademicYears_AY WHERE ' + QUOTENAME(@ColumnName) + N' = @P_Val';
        IF @ExcludeId IS NOT NULL
            SET @Sql = @Sql + N' AND AY_Id <> @P_ExId';
        EXEC sp_executesql @Sql, N'@P_Val NVARCHAR(MAX), @P_ExId UNIQUEIDENTIFIER', @Value, @ExcludeId;
    END
END
GO

-- 5. USP_Departments_D (Departments_D)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[USP_Departments_D]') AND type in (N'P'))
    DROP PROCEDURE [dbo].[USP_Departments_D];
GO
CREATE PROCEDURE [dbo].[USP_Departments_D]
    @Action NVARCHAR(20),
    @D_Id UNIQUEIDENTIFIER = NULL,
    @D_Name NVARCHAR(150) = NULL,
    @D_Code NVARCHAR(50) = NULL,
    @D_BranchId UNIQUEIDENTIFIER = NULL,
    @D_Description NVARCHAR(MAX) = NULL,
    @TenantId UNIQUEIDENTIFIER = NULL,
    @CreatedBy UNIQUEIDENTIFIER = NULL,
    @UpdatedBy UNIQUEIDENTIFIER = NULL,
    @ColumnName NVARCHAR(100) = NULL,
    @Value NVARCHAR(MAX) = NULL,
    @ExcludeId UNIQUEIDENTIFIER = NULL,
    @NewId UNIQUEIDENTIFIER = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF @Action = 'GetAll'
    BEGIN
        SELECT D_Id, D_TenantId, D_BranchId, D_Name, D_Code, D_Description,
               D_CreatedAt, D_UpdatedAt
        FROM dbo.Departments_D
        ORDER BY D_Name;
    END

    ELSE IF @Action = 'GetById'
    BEGIN
        SELECT D_Id, D_TenantId, D_BranchId, D_Name, D_Code, D_Description,
               D_CreatedAt, D_UpdatedAt
        FROM dbo.Departments_D
        WHERE D_Id = @D_Id;
    END

    ELSE IF @Action = 'Insert'
    BEGIN
        SET @NewId = NEWID();
        INSERT INTO dbo.Departments_D (D_Id, D_TenantId, D_BranchId, D_Name, D_Code, D_Description)
        VALUES (@NewId, @TenantId, @D_BranchId, @D_Name, @D_Code, @D_Description);
    END

    ELSE IF @Action = 'Update'
    BEGIN
        UPDATE dbo.Departments_D
        SET D_BranchId = @D_BranchId,
            D_Name = @D_Name,
            D_Code = @D_Code,
            D_Description = @D_Description,
            D_UpdatedAt = SYSUTCDATETIME()
        WHERE D_Id = @D_Id;
    END

    ELSE IF @Action = 'Deactivate'
    BEGIN
        RAISERROR('Departments_D does not support soft delete.', 16, 1);
    END

    ELSE IF @Action = 'ExistsByField'
    BEGIN
        DECLARE @Sql NVARCHAR(MAX);
        SET @Sql = N'SELECT COUNT(1) FROM dbo.Departments_D WHERE ' + QUOTENAME(@ColumnName) + N' = @P_Val';
        IF @ExcludeId IS NOT NULL
            SET @Sql = @Sql + N' AND D_Id <> @P_ExId';
        EXEC sp_executesql @Sql, N'@P_Val NVARCHAR(MAX), @P_ExId UNIQUEIDENTIFIER', @Value, @ExcludeId;
    END
END
GO

-- 6. USP_Designations_DS (Designations_DS)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[USP_Designations_DS]') AND type in (N'P'))
    DROP PROCEDURE [dbo].[USP_Designations_DS];
GO
CREATE PROCEDURE [dbo].[USP_Designations_DS]
    @Action NVARCHAR(20),
    @DS_Id UNIQUEIDENTIFIER = NULL,
    @DS_Name NVARCHAR(100) = NULL,
    @DS_Code NVARCHAR(50) = NULL,
    @TenantId UNIQUEIDENTIFIER = NULL,
    @CreatedBy UNIQUEIDENTIFIER = NULL,
    @UpdatedBy UNIQUEIDENTIFIER = NULL,
    @ColumnName NVARCHAR(100) = NULL,
    @Value NVARCHAR(MAX) = NULL,
    @ExcludeId UNIQUEIDENTIFIER = NULL,
    @NewId UNIQUEIDENTIFIER = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF @Action = 'GetAll'
    BEGIN
        SELECT DS_Id, DS_TenantId, DS_Name, DS_Code, DS_CreatedAt, DS_UpdatedAt
        FROM dbo.Designations_DS
        ORDER BY DS_Name;
    END

    ELSE IF @Action = 'GetById'
    BEGIN
        SELECT DS_Id, DS_TenantId, DS_Name, DS_Code, DS_CreatedAt, DS_UpdatedAt
        FROM dbo.Designations_DS
        WHERE DS_Id = @DS_Id;
    END

    ELSE IF @Action = 'Insert'
    BEGIN
        SET @NewId = NEWID();
        INSERT INTO dbo.Designations_DS (DS_Id, DS_TenantId, DS_Name, DS_Code)
        VALUES (@NewId, @TenantId, @DS_Name, @DS_Code);
    END

    ELSE IF @Action = 'Update'
    BEGIN
        UPDATE dbo.Designations_DS
        SET DS_Name = @DS_Name,
            DS_Code = @DS_Code,
            DS_UpdatedAt = SYSUTCDATETIME()
        WHERE DS_Id = @DS_Id;
    END

    ELSE IF @Action = 'Deactivate'
    BEGIN
        RAISERROR('Designations_DS does not support soft delete.', 16, 1);
    END

    ELSE IF @Action = 'ExistsByField'
    BEGIN
        DECLARE @Sql NVARCHAR(MAX);
        SET @Sql = N'SELECT COUNT(1) FROM dbo.Designations_DS WHERE ' + QUOTENAME(@ColumnName) + N' = @P_Val';
        IF @ExcludeId IS NOT NULL
            SET @Sql = @Sql + N' AND DS_Id <> @P_ExId';
        EXEC sp_executesql @Sql, N'@P_Val NVARCHAR(MAX), @P_ExId UNIQUEIDENTIFIER', @Value, @ExcludeId;
    END
END
GO

-- 7. USP_DocumentTypes_DT (DocumentTypes_DT)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[USP_DocumentTypes_DT]') AND type in (N'P'))
    DROP PROCEDURE [dbo].[USP_DocumentTypes_DT];
GO
CREATE PROCEDURE [dbo].[USP_DocumentTypes_DT]
    @Action NVARCHAR(20),
    @DT_Id UNIQUEIDENTIFIER = NULL,
    @DT_Name NVARCHAR(100) = NULL,
    @DT_Code NVARCHAR(50) = NULL,
    @DT_EntityType NVARCHAR(30) = NULL,
    @DT_IsRequired BIT = NULL,
    @TenantId UNIQUEIDENTIFIER = NULL,
    @CreatedBy UNIQUEIDENTIFIER = NULL,
    @UpdatedBy UNIQUEIDENTIFIER = NULL,
    @ColumnName NVARCHAR(100) = NULL,
    @Value NVARCHAR(MAX) = NULL,
    @ExcludeId UNIQUEIDENTIFIER = NULL,
    @NewId UNIQUEIDENTIFIER = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF @Action = 'GetAll'
    BEGIN
        SELECT DT_Id, DT_TenantId, DT_Name, DT_Code, DT_EntityType, DT_IsRequired,
               DT_CreatedAt, DT_UpdatedAt
        FROM dbo.DocumentTypes_DT
        ORDER BY DT_Name;
    END

    ELSE IF @Action = 'GetById'
    BEGIN
        SELECT DT_Id, DT_TenantId, DT_Name, DT_Code, DT_EntityType, DT_IsRequired,
               DT_CreatedAt, DT_UpdatedAt
        FROM dbo.DocumentTypes_DT
        WHERE DT_Id = @DT_Id;
    END

    ELSE IF @Action = 'Insert'
    BEGIN
        SET @NewId = NEWID();
        INSERT INTO dbo.DocumentTypes_DT (DT_Id, DT_TenantId, DT_Name, DT_Code, DT_EntityType, DT_IsRequired)
        VALUES (@NewId, @TenantId, @DT_Name, @DT_Code, @DT_EntityType, ISNULL(@DT_IsRequired, 0));
    END

    ELSE IF @Action = 'Update'
    BEGIN
        UPDATE dbo.DocumentTypes_DT
        SET DT_Name = @DT_Name,
            DT_Code = @DT_Code,
            DT_EntityType = @DT_EntityType,
            DT_IsRequired = @DT_IsRequired,
            DT_UpdatedAt = SYSUTCDATETIME()
        WHERE DT_Id = @DT_Id;
    END

    ELSE IF @Action = 'Deactivate'
    BEGIN
        RAISERROR('DocumentTypes_DT does not support soft delete.', 16, 1);
    END

    ELSE IF @Action = 'ExistsByField'
    BEGIN
        DECLARE @Sql NVARCHAR(MAX);
        SET @Sql = N'SELECT COUNT(1) FROM dbo.DocumentTypes_DT WHERE ' + QUOTENAME(@ColumnName) + N' = @P_Val';
        IF @ExcludeId IS NOT NULL
            SET @Sql = @Sql + N' AND DT_Id <> @P_ExId';
        EXEC sp_executesql @Sql, N'@P_Val NVARCHAR(MAX), @P_ExId UNIQUEIDENTIFIER', @Value, @ExcludeId;
    END
END
GO

-- 8. USP_ExamTypes_ET (ExamTypes_ET)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[USP_ExamTypes_ET]') AND type in (N'P'))
    DROP PROCEDURE [dbo].[USP_ExamTypes_ET];
GO
CREATE PROCEDURE [dbo].[USP_ExamTypes_ET]
    @Action NVARCHAR(20),
    @ET_Id UNIQUEIDENTIFIER = NULL,
    @ET_Name NVARCHAR(100) = NULL,
    @ET_Code NVARCHAR(50) = NULL,
    @ET_WeightagePercentage DECIMAL(5,2) = NULL,
    @TenantId UNIQUEIDENTIFIER = NULL,
    @CreatedBy UNIQUEIDENTIFIER = NULL,
    @UpdatedBy UNIQUEIDENTIFIER = NULL,
    @ColumnName NVARCHAR(100) = NULL,
    @Value NVARCHAR(MAX) = NULL,
    @ExcludeId UNIQUEIDENTIFIER = NULL,
    @NewId UNIQUEIDENTIFIER = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF @Action = 'GetAll'
    BEGIN
        SELECT ET_Id, ET_TenantId, ET_Name, ET_Code, ET_WeightagePercentage, ET_IsActive
        FROM dbo.ExamTypes_ET
        WHERE ET_IsActive = 1
        ORDER BY ET_Name;
    END

    ELSE IF @Action = 'GetById'
    BEGIN
        SELECT ET_Id, ET_TenantId, ET_Name, ET_Code, ET_WeightagePercentage, ET_IsActive
        FROM dbo.ExamTypes_ET
        WHERE ET_Id = @ET_Id;
    END

    ELSE IF @Action = 'Insert'
    BEGIN
        SET @NewId = NEWID();
        INSERT INTO dbo.ExamTypes_ET (ET_Id, ET_TenantId, ET_Name, ET_Code, ET_WeightagePercentage, ET_IsActive)
        VALUES (@NewId, @TenantId, @ET_Name, @ET_Code, @ET_WeightagePercentage, 1);
    END

    ELSE IF @Action = 'Update'
    BEGIN
        UPDATE dbo.ExamTypes_ET
        SET ET_Name = @ET_Name,
            ET_Code = @ET_Code,
            ET_WeightagePercentage = @ET_WeightagePercentage,
            ET_UpdatedAt = SYSUTCDATETIME()
        WHERE ET_Id = @ET_Id;
    END

    ELSE IF @Action = 'Deactivate'
    BEGIN
        UPDATE dbo.ExamTypes_ET
        SET ET_IsActive = 0,
            ET_UpdatedAt = SYSUTCDATETIME()
        WHERE ET_Id = @ET_Id;
    END

    ELSE IF @Action = 'ExistsByField'
    BEGIN
        DECLARE @Sql NVARCHAR(MAX);
        SET @Sql = N'SELECT COUNT(1) FROM dbo.ExamTypes_ET WHERE ' + QUOTENAME(@ColumnName) + N' = @P_Val AND ET_IsActive = 1';
        IF @ExcludeId IS NOT NULL
            SET @Sql = @Sql + N' AND ET_Id <> @P_ExId';
        EXEC sp_executesql @Sql, N'@P_Val NVARCHAR(MAX), @P_ExId UNIQUEIDENTIFIER', @Value, @ExcludeId;
    END
END
GO

-- 9. USP_ExpenseCategories_EC (ExpenseCategories_EC)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[USP_ExpenseCategories_EC]') AND type in (N'P'))
    DROP PROCEDURE [dbo].[USP_ExpenseCategories_EC];
GO
CREATE PROCEDURE [dbo].[USP_ExpenseCategories_EC]
    @Action NVARCHAR(20),
    @EC_Id UNIQUEIDENTIFIER = NULL,
    @EC_Name NVARCHAR(100) = NULL,
    @EC_Code NVARCHAR(50) = NULL,
    @EC_Description NVARCHAR(MAX) = NULL,
    @TenantId UNIQUEIDENTIFIER = NULL,
    @CreatedBy UNIQUEIDENTIFIER = NULL,
    @UpdatedBy UNIQUEIDENTIFIER = NULL,
    @ColumnName NVARCHAR(100) = NULL,
    @Value NVARCHAR(MAX) = NULL,
    @ExcludeId UNIQUEIDENTIFIER = NULL,
    @NewId UNIQUEIDENTIFIER = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF @Action = 'GetAll'
    BEGIN
        SELECT EC_Id, EC_TenantId, EC_Name, EC_Code, EC_Description, EC_CreatedAt, EC_UpdatedAt
        FROM dbo.ExpenseCategories_EC
        ORDER BY EC_Name;
    END

    ELSE IF @Action = 'GetById'
    BEGIN
        SELECT EC_Id, EC_TenantId, EC_Name, EC_Code, EC_Description, EC_CreatedAt, EC_UpdatedAt
        FROM dbo.ExpenseCategories_EC
        WHERE EC_Id = @EC_Id;
    END

    ELSE IF @Action = 'Insert'
    BEGIN
        SET @NewId = NEWID();
        INSERT INTO dbo.ExpenseCategories_EC (EC_Id, EC_TenantId, EC_Name, EC_Code, EC_Description)
        VALUES (@NewId, @TenantId, @EC_Name, @EC_Code, @EC_Description);
    END

    ELSE IF @Action = 'Update'
    BEGIN
        UPDATE dbo.ExpenseCategories_EC
        SET EC_Name = @EC_Name,
            EC_Code = @EC_Code,
            EC_Description = @EC_Description,
            EC_UpdatedAt = SYSUTCDATETIME()
        WHERE EC_Id = @EC_Id;
    END

    ELSE IF @Action = 'Deactivate'
    BEGIN
        RAISERROR('ExpenseCategories_EC does not support soft delete.', 16, 1);
    END

    ELSE IF @Action = 'ExistsByField'
    BEGIN
        DECLARE @Sql NVARCHAR(MAX);
        SET @Sql = N'SELECT COUNT(1) FROM dbo.ExpenseCategories_EC WHERE ' + QUOTENAME(@ColumnName) + N' = @P_Val';
        IF @ExcludeId IS NOT NULL
            SET @Sql = @Sql + N' AND EC_Id <> @P_ExId';
        EXEC sp_executesql @Sql, N'@P_Val NVARCHAR(MAX), @P_ExId UNIQUEIDENTIFIER', @Value, @ExcludeId;
    END
END
GO

-- 10. USP_FeeCategories_FC (FeeCategories_FC)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[USP_FeeCategories_FC]') AND type in (N'P'))
    DROP PROCEDURE [dbo].[USP_FeeCategories_FC];
GO
CREATE PROCEDURE [dbo].[USP_FeeCategories_FC]
    @Action NVARCHAR(20),
    @FC_Id UNIQUEIDENTIFIER = NULL,
    @FC_Name NVARCHAR(100) = NULL,
    @FC_Code NVARCHAR(50) = NULL,
    @FC_Description NVARCHAR(MAX) = NULL,
    @FC_IsRefundable BIT = NULL,
    @TenantId UNIQUEIDENTIFIER = NULL,
    @CreatedBy UNIQUEIDENTIFIER = NULL,
    @UpdatedBy UNIQUEIDENTIFIER = NULL,
    @ColumnName NVARCHAR(100) = NULL,
    @Value NVARCHAR(MAX) = NULL,
    @ExcludeId UNIQUEIDENTIFIER = NULL,
    @NewId UNIQUEIDENTIFIER = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF @Action = 'GetAll'
    BEGIN
        SELECT FC_Id, FC_TenantId, FC_Name, FC_Code, FC_Description, FC_IsRefundable,
               FC_CreatedAt, FC_UpdatedAt
        FROM dbo.FeeCategories_FC
        ORDER BY FC_Name;
    END

    ELSE IF @Action = 'GetById'
    BEGIN
        SELECT FC_Id, FC_TenantId, FC_Name, FC_Code, FC_Description, FC_IsRefundable,
               FC_CreatedAt, FC_UpdatedAt
        FROM dbo.FeeCategories_FC
        WHERE FC_Id = @FC_Id;
    END

    ELSE IF @Action = 'Insert'
    BEGIN
        SET @NewId = NEWID();
        INSERT INTO dbo.FeeCategories_FC (FC_Id, FC_TenantId, FC_Name, FC_Code, FC_Description, FC_IsRefundable)
        VALUES (@NewId, @TenantId, @FC_Name, @FC_Code, @FC_Description, ISNULL(@FC_IsRefundable, 0));
    END

    ELSE IF @Action = 'Update'
    BEGIN
        UPDATE dbo.FeeCategories_FC
        SET FC_Name = @FC_Name,
            FC_Code = @FC_Code,
            FC_Description = @FC_Description,
            FC_IsRefundable = @FC_IsRefundable,
            FC_UpdatedAt = SYSUTCDATETIME()
        WHERE FC_Id = @FC_Id;
    END

    ELSE IF @Action = 'Deactivate'
    BEGIN
        RAISERROR('FeeCategories_FC does not support soft delete.', 16, 1);
    END

    ELSE IF @Action = 'ExistsByField'
    BEGIN
        DECLARE @Sql NVARCHAR(MAX);
        SET @Sql = N'SELECT COUNT(1) FROM dbo.FeeCategories_FC WHERE ' + QUOTENAME(@ColumnName) + N' = @P_Val';
        IF @ExcludeId IS NOT NULL
            SET @Sql = @Sql + N' AND FC_Id <> @P_ExId';
        EXEC sp_executesql @Sql, N'@P_Val NVARCHAR(MAX), @P_ExId UNIQUEIDENTIFIER', @Value, @ExcludeId;
    END
END
GO

-- 11. USP_GradeScales_GS (GradeScales_GS)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[USP_GradeScales_GS]') AND type in (N'P'))
    DROP PROCEDURE [dbo].[USP_GradeScales_GS];
GO
CREATE PROCEDURE [dbo].[USP_GradeScales_GS]
    @Action NVARCHAR(20),
    @GS_Id UNIQUEIDENTIFIER = NULL,
    @GS_Name NVARCHAR(100) = NULL,
    @GS_Code NVARCHAR(50) = NULL,
    @GS_Description NVARCHAR(MAX) = NULL,
    @GS_IsDefault BIT = NULL,
    @TenantId UNIQUEIDENTIFIER = NULL,
    @CreatedBy UNIQUEIDENTIFIER = NULL,
    @UpdatedBy UNIQUEIDENTIFIER = NULL,
    @ColumnName NVARCHAR(100) = NULL,
    @Value NVARCHAR(MAX) = NULL,
    @ExcludeId UNIQUEIDENTIFIER = NULL,
    @NewId UNIQUEIDENTIFIER = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF @Action = 'GetAll'
    BEGIN
        SELECT GS_Id, GS_TenantId, GS_Name, GS_Code, GS_Description, GS_IsDefault, GS_IsActive
        FROM dbo.GradeScales_GS
        WHERE GS_IsActive = 1
        ORDER BY GS_Name;
    END

    ELSE IF @Action = 'GetById'
    BEGIN
        SELECT GS_Id, GS_TenantId, GS_Name, GS_Code, GS_Description, GS_IsDefault, GS_IsActive
        FROM dbo.GradeScales_GS
        WHERE GS_Id = @GS_Id;
    END

    ELSE IF @Action = 'Insert'
    BEGIN
        SET @NewId = NEWID();
        INSERT INTO dbo.GradeScales_GS (GS_Id, GS_TenantId, GS_Name, GS_Code, GS_Description, GS_IsDefault, GS_IsActive)
        VALUES (@NewId, @TenantId, @GS_Name, @GS_Code, @GS_Description, ISNULL(@GS_IsDefault, 0), 1);
    END

    ELSE IF @Action = 'Update'
    BEGIN
        UPDATE dbo.GradeScales_GS
        SET GS_Name = @GS_Name,
            GS_Code = @GS_Code,
            GS_Description = @GS_Description,
            GS_IsDefault = @GS_IsDefault,
            GS_UpdatedAt = SYSUTCDATETIME()
        WHERE GS_Id = @GS_Id;
    END

    ELSE IF @Action = 'Deactivate'
    BEGIN
        UPDATE dbo.GradeScales_GS
        SET GS_IsActive = 0,
            GS_UpdatedAt = SYSUTCDATETIME()
        WHERE GS_Id = @GS_Id;
    END

    ELSE IF @Action = 'ExistsByField'
    BEGIN
        DECLARE @Sql NVARCHAR(MAX);
        SET @Sql = N'SELECT COUNT(1) FROM dbo.GradeScales_GS WHERE ' + QUOTENAME(@ColumnName) + N' = @P_Val AND GS_IsActive = 1';
        IF @ExcludeId IS NOT NULL
            SET @Sql = @Sql + N' AND GS_Id <> @P_ExId';
        EXEC sp_executesql @Sql, N'@P_Val NVARCHAR(MAX), @P_ExId UNIQUEIDENTIFIER', @Value, @ExcludeId;
    END
END
GO

-- 12. USP_Classrooms_CR (Classrooms_CR)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[USP_Classrooms_CR]') AND type in (N'P'))
    DROP PROCEDURE [dbo].[USP_Classrooms_CR];
GO
CREATE PROCEDURE [dbo].[USP_Classrooms_CR]
    @Action NVARCHAR(20),
    @CR_Id UNIQUEIDENTIFIER = NULL,
    @CR_Name NVARCHAR(100) = NULL,
    @CR_Code NVARCHAR(50) = NULL,
    @CR_BranchId UNIQUEIDENTIFIER = NULL,
    @CR_Capacity INT = NULL,
    @CR_Location NVARCHAR(255) = NULL,
    @TenantId UNIQUEIDENTIFIER = NULL,
    @CreatedBy UNIQUEIDENTIFIER = NULL,
    @UpdatedBy UNIQUEIDENTIFIER = NULL,
    @ColumnName NVARCHAR(100) = NULL,
    @Value NVARCHAR(MAX) = NULL,
    @ExcludeId UNIQUEIDENTIFIER = NULL,
    @NewId UNIQUEIDENTIFIER = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF @Action = 'GetAll'
    BEGIN
        SELECT CR_Id, CR_TenantId, CR_BranchId, CR_Name, CR_Code, CR_Capacity, CR_Location,
               CR_CreatedAt, CR_UpdatedAt
        FROM dbo.Classrooms_CR
        ORDER BY CR_Name;
    END

    ELSE IF @Action = 'GetById'
    BEGIN
        SELECT CR_Id, CR_TenantId, CR_BranchId, CR_Name, CR_Code, CR_Capacity, CR_Location,
               CR_CreatedAt, CR_UpdatedAt
        FROM dbo.Classrooms_CR
        WHERE CR_Id = @CR_Id;
    END

    ELSE IF @Action = 'Insert'
    BEGIN
        SET @NewId = NEWID();
        INSERT INTO dbo.Classrooms_CR (CR_Id, CR_TenantId, CR_BranchId, CR_Name, CR_Code, CR_Capacity, CR_Location)
        VALUES (@NewId, @TenantId, @CR_BranchId, @CR_Name, @CR_Code, @CR_Capacity, @CR_Location);
    END

    ELSE IF @Action = 'Update'
    BEGIN
        UPDATE dbo.Classrooms_CR
        SET CR_BranchId = @CR_BranchId,
            CR_Name = @CR_Name,
            CR_Code = @CR_Code,
            CR_Capacity = @CR_Capacity,
            CR_Location = @CR_Location,
            CR_UpdatedAt = SYSUTCDATETIME()
        WHERE CR_Id = @CR_Id;
    END

    ELSE IF @Action = 'Deactivate'
    BEGIN
        RAISERROR('Classrooms_CR does not support soft delete.', 16, 1);
    END

    ELSE IF @Action = 'ExistsByField'
    BEGIN
        DECLARE @Sql NVARCHAR(MAX);
        SET @Sql = N'SELECT COUNT(1) FROM dbo.Classrooms_CR WHERE ' + QUOTENAME(@ColumnName) + N' = @P_Val';
        IF @ExcludeId IS NOT NULL
            SET @Sql = @Sql + N' AND CR_Id <> @P_ExId';
        EXEC sp_executesql @Sql, N'@P_Val NVARCHAR(MAX), @P_ExId UNIQUEIDENTIFIER', @Value, @ExcludeId;
    END
END
GO

-- 13. USP_PaymentMethods_PM (PaymentMethods_PM)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[USP_PaymentMethods_PM]') AND type in (N'P'))
    DROP PROCEDURE [dbo].[USP_PaymentMethods_PM];
GO
CREATE PROCEDURE [dbo].[USP_PaymentMethods_PM]
    @Action NVARCHAR(20),
    @PM_Id UNIQUEIDENTIFIER = NULL,
    @PM_Name NVARCHAR(100) = NULL,
    @PM_Type NVARCHAR(30) = NULL,
    @PM_IsActive BIT = NULL,
    @TenantId UNIQUEIDENTIFIER = NULL,
    @CreatedBy UNIQUEIDENTIFIER = NULL,
    @UpdatedBy UNIQUEIDENTIFIER = NULL,
    @ColumnName NVARCHAR(100) = NULL,
    @Value NVARCHAR(MAX) = NULL,
    @ExcludeId UNIQUEIDENTIFIER = NULL,
    @NewId UNIQUEIDENTIFIER = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF @Action = 'GetAll'
    BEGIN
        SELECT PM_Id, PM_TenantId, PM_Name, PM_Type, PM_IsActive
        FROM dbo.PaymentMethods_PM
        WHERE PM_IsActive = 1
        ORDER BY PM_Name;
    END

    ELSE IF @Action = 'GetById'
    BEGIN
        SELECT PM_Id, PM_TenantId, PM_Name, PM_Type, PM_IsActive
        FROM dbo.PaymentMethods_PM
        WHERE PM_Id = @PM_Id;
    END

    ELSE IF @Action = 'Insert'
    BEGIN
        SET @NewId = NEWID();
        INSERT INTO dbo.PaymentMethods_PM (PM_Id, PM_TenantId, PM_Name, PM_Type, PM_IsActive)
        VALUES (@NewId, @TenantId, @PM_Name, @PM_Type, ISNULL(@PM_IsActive, 1));
    END

    ELSE IF @Action = 'Update'
    BEGIN
        UPDATE dbo.PaymentMethods_PM
        SET PM_Name = @PM_Name,
            PM_Type = @PM_Type,
            PM_IsActive = ISNULL(@PM_IsActive, 1),
            PM_UpdatedAt = SYSUTCDATETIME()
        WHERE PM_Id = @PM_Id;
    END

    ELSE IF @Action = 'Deactivate'
    BEGIN
        UPDATE dbo.PaymentMethods_PM
        SET PM_IsActive = 0
        WHERE PM_Id = @PM_Id;
    END

    ELSE IF @Action = 'ExistsByField'
    BEGIN
        DECLARE @Sql NVARCHAR(MAX);
        SET @Sql = N'SELECT COUNT(1) FROM dbo.PaymentMethods_PM WHERE ' + QUOTENAME(@ColumnName) + N' = @P_Val AND PM_IsActive = 1';
        IF @ExcludeId IS NOT NULL
            SET @Sql = @Sql + N' AND PM_Id <> @P_ExId';
        EXEC sp_executesql @Sql, N'@P_Val NVARCHAR(MAX), @P_ExId UNIQUEIDENTIFIER', @Value, @ExcludeId;
    END
END
GO

-- 14. USP_Discounts_DIS (Discounts_DIS)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[USP_Discounts_DIS]') AND type in (N'P'))
    DROP PROCEDURE [dbo].[USP_Discounts_DIS];
GO
CREATE PROCEDURE [dbo].[USP_Discounts_DIS]
    @Action NVARCHAR(20),
    @DIS_Id UNIQUEIDENTIFIER = NULL,
    @DIS_Name NVARCHAR(100) = NULL,
    @DIS_Code NVARCHAR(50) = NULL,
    @DIS_DiscountType NVARCHAR(20) = NULL,
    @DIS_Value NUMERIC = NULL,
    @DIS_Description NVARCHAR(MAX) = NULL,
    @DIS_IsActive BIT = NULL,
    @TenantId UNIQUEIDENTIFIER = NULL,
    @CreatedBy UNIQUEIDENTIFIER = NULL,
    @UpdatedBy UNIQUEIDENTIFIER = NULL,
    @ColumnName NVARCHAR(100) = NULL,
    @Value NVARCHAR(MAX) = NULL,
    @ExcludeId UNIQUEIDENTIFIER = NULL,
    @NewId UNIQUEIDENTIFIER = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF @Action = 'GetAll'
    BEGIN
        SELECT DIS_Id, DIS_TenantId, DIS_Name, DIS_Code, DIS_DiscountType, DIS_Value,
               DIS_Description, DIS_IsActive, DIS_CreatedAt, DIS_UpdatedAt
        FROM dbo.Discounts_DIS
        WHERE DIS_IsActive = 1
        ORDER BY DIS_Name;
    END

    ELSE IF @Action = 'GetById'
    BEGIN
        SELECT DIS_Id, DIS_TenantId, DIS_Name, DIS_Code, DIS_DiscountType, DIS_Value,
               DIS_Description, DIS_IsActive, DIS_CreatedAt, DIS_UpdatedAt
        FROM dbo.Discounts_DIS
        WHERE DIS_Id = @DIS_Id;
    END

    ELSE IF @Action = 'Insert'
    BEGIN
        SET @NewId = NEWID();
        INSERT INTO dbo.Discounts_DIS (DIS_Id, DIS_TenantId, DIS_Name, DIS_Code, DIS_DiscountType, DIS_Value, DIS_Description, DIS_IsActive)
        VALUES (@NewId, @TenantId, @DIS_Name, @DIS_Code, @DIS_DiscountType, @DIS_Value, @DIS_Description, ISNULL(@DIS_IsActive, 1));
    END

    ELSE IF @Action = 'Update'
    BEGIN
        UPDATE dbo.Discounts_DIS
        SET DIS_Name = @DIS_Name,
            DIS_Code = @DIS_Code,
            DIS_DiscountType = @DIS_DiscountType,
            DIS_Value = @DIS_Value,
            DIS_Description = @DIS_Description,
            DIS_IsActive = @DIS_IsActive,
            DIS_UpdatedAt = SYSUTCDATETIME()
        WHERE DIS_Id = @DIS_Id;
    END

    ELSE IF @Action = 'Deactivate'
    BEGIN
        UPDATE dbo.Discounts_DIS
        SET DIS_IsActive = 0, DIS_UpdatedAt = SYSUTCDATETIME()
        WHERE DIS_Id = @DIS_Id;
    END

    ELSE IF @Action = 'ExistsByField'
    BEGIN
        DECLARE @Sql NVARCHAR(MAX);
        SET @Sql = N'SELECT COUNT(1) FROM dbo.Discounts_DIS WHERE ' + QUOTENAME(@ColumnName) + N' = @P_Val AND DIS_IsActive = 1';
        IF @ExcludeId IS NOT NULL
            SET @Sql = @Sql + N' AND DIS_Id <> @P_ExId';
        EXEC sp_executesql @Sql, N'@P_Val NVARCHAR(MAX), @P_ExId UNIQUEIDENTIFIER', @Value, @ExcludeId;
    END
END
GO

-- 15. USP_Subjects_SB (Subjects_SB)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[USP_Subjects_SB]') AND type in (N'P'))
    DROP PROCEDURE [dbo].[USP_Subjects_SB];
GO
CREATE PROCEDURE [dbo].[USP_Subjects_SB]
    @Action NVARCHAR(20),
    @SB_Id UNIQUEIDENTIFIER = NULL,
    @SB_Name NVARCHAR(200) = NULL,
    @SB_Code NVARCHAR(50) = NULL,
    @SB_Description NVARCHAR(MAX) = NULL,
    @SB_Credits NUMERIC = NULL,
    @SB_MaxMarks NUMERIC = NULL,
    @SB_PassMarks NUMERIC = NULL,
    @TenantId UNIQUEIDENTIFIER = NULL,
    @CreatedBy UNIQUEIDENTIFIER = NULL,
    @UpdatedBy UNIQUEIDENTIFIER = NULL,
    @ColumnName NVARCHAR(100) = NULL,
    @Value NVARCHAR(MAX) = NULL,
    @ExcludeId UNIQUEIDENTIFIER = NULL,
    @NewId UNIQUEIDENTIFIER = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF @Action = 'GetAll'
    BEGIN
        SELECT SB_Id, SB_TenantId, SB_Name, SB_Code, SB_Description, SB_Credits, SB_MaxMarks, SB_PassMarks,
               SB_CreatedAt, SB_UpdatedAt
        FROM dbo.Subjects_SB
        ORDER BY SB_Name;
    END

    ELSE IF @Action = 'GetById'
    BEGIN
        SELECT SB_Id, SB_TenantId, SB_Name, SB_Code, SB_Description, SB_Credits, SB_MaxMarks, SB_PassMarks,
               SB_CreatedAt, SB_UpdatedAt
        FROM dbo.Subjects_SB
        WHERE SB_Id = @SB_Id;
    END

    ELSE IF @Action = 'Insert'
    BEGIN
        SET @NewId = NEWID();
        INSERT INTO dbo.Subjects_SB (SB_Id, SB_TenantId, SB_Name, SB_Code, SB_Description, SB_Credits, SB_MaxMarks, SB_PassMarks)
        VALUES (@NewId, @TenantId, @SB_Name, @SB_Code, @SB_Description, @SB_Credits, @SB_MaxMarks, @SB_PassMarks);
    END

    ELSE IF @Action = 'Update'
    BEGIN
        UPDATE dbo.Subjects_SB
        SET SB_Name = @SB_Name,
            SB_Code = @SB_Code,
            SB_Description = @SB_Description,
            SB_Credits = @SB_Credits,
            SB_MaxMarks = @SB_MaxMarks,
            SB_PassMarks = @SB_PassMarks,
            SB_UpdatedAt = SYSUTCDATETIME()
        WHERE SB_Id = @SB_Id;
    END

    ELSE IF @Action = 'Deactivate'
    BEGIN
        RAISERROR('Subjects_SB does not support soft delete.', 16, 1);
    END

    ELSE IF @Action = 'ExistsByField'
    BEGIN
        DECLARE @Sql NVARCHAR(MAX);
        SET @Sql = N'SELECT COUNT(1) FROM dbo.Subjects_SB WHERE ' + QUOTENAME(@ColumnName) + N' = @P_Val';
        IF @ExcludeId IS NOT NULL
            SET @Sql = @Sql + N' AND SB_Id <> @P_ExId';
        EXEC sp_executesql @Sql, N'@P_Val NVARCHAR(MAX), @P_ExId UNIQUEIDENTIFIER', @Value, @ExcludeId;
    END
END
GO

-- 16. USP_Programs_P (Programs_P)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[USP_Programs_P]') AND type in (N'P'))
    DROP PROCEDURE [dbo].[USP_Programs_P];
GO
CREATE PROCEDURE [dbo].[USP_Programs_P]
    @Action NVARCHAR(20),
    @P_Id UNIQUEIDENTIFIER = NULL,
    @P_Name NVARCHAR(200) = NULL,
    @P_Code NVARCHAR(50) = NULL,
    @P_DurationValue INT = NULL,
    @P_DurationUnit NVARCHAR(20) = NULL,
    @P_Description NVARCHAR(MAX) = NULL,
    @TenantId UNIQUEIDENTIFIER = NULL,
    @CreatedBy UNIQUEIDENTIFIER = NULL,
    @UpdatedBy UNIQUEIDENTIFIER = NULL,
    @ColumnName NVARCHAR(100) = NULL,
    @Value NVARCHAR(MAX) = NULL,
    @ExcludeId UNIQUEIDENTIFIER = NULL,
    @NewId UNIQUEIDENTIFIER = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF @Action = 'GetAll'
    BEGIN
        SELECT P_Id, P_TenantId, P_Name, P_Code, P_DurationValue, P_DurationUnit,
               P_Description, P_Status, P_CreatedAt, P_UpdatedAt
        FROM dbo.Programs_P
        WHERE P_Status = 'active'
        ORDER BY P_Name;
    END

    ELSE IF @Action = 'GetById'
    BEGIN
        SELECT P_Id, P_TenantId, P_Name, P_Code, P_DurationValue, P_DurationUnit,
               P_Description, P_Status, P_CreatedAt, P_UpdatedAt
        FROM dbo.Programs_P
        WHERE P_Id = @P_Id;
    END

    ELSE IF @Action = 'Insert'
    BEGIN
        SET @NewId = NEWID();
        INSERT INTO dbo.Programs_P (P_Id, P_TenantId, P_Name, P_Code, P_DurationValue, P_DurationUnit, P_Description, P_Status)
        VALUES (@NewId, @TenantId, @P_Name, @P_Code, @P_DurationValue, @P_DurationUnit, @P_Description, 'active');
    END

    ELSE IF @Action = 'Update'
    BEGIN
        UPDATE dbo.Programs_P
        SET P_Name = @P_Name,
            P_Code = @P_Code,
            P_DurationValue = @P_DurationValue,
            P_DurationUnit = @P_DurationUnit,
            P_Description = @P_Description,
            P_UpdatedAt = SYSUTCDATETIME()
        WHERE P_Id = @P_Id;
    END

    ELSE IF @Action = 'Deactivate'
    BEGIN
        UPDATE dbo.Programs_P
        SET P_Status = 'inactive', P_UpdatedAt = SYSUTCDATETIME()
        WHERE P_Id = @P_Id;
    END

    ELSE IF @Action = 'ExistsByField'
    BEGIN
        DECLARE @Sql NVARCHAR(MAX);
        SET @Sql = N'SELECT COUNT(1) FROM dbo.Programs_P WHERE ' + QUOTENAME(@ColumnName) + N' = @P_Val AND P_Status = ''active''';
        IF @ExcludeId IS NOT NULL
            SET @Sql = @Sql + N' AND P_Id <> @P_ExId';
        EXEC sp_executesql @Sql, N'@P_Val NVARCHAR(MAX), @P_ExId UNIQUEIDENTIFIER', @Value, @ExcludeId;
    END
END
GO

-- 17. USP_Vendors_V (Vendors_V)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[USP_Vendors_V]') AND type in (N'P'))
    DROP PROCEDURE [dbo].[USP_Vendors_V];
GO
CREATE PROCEDURE [dbo].[USP_Vendors_V]
    @Action NVARCHAR(20),
    @V_Id UNIQUEIDENTIFIER = NULL,
    @V_Name NVARCHAR(200) = NULL,
    @V_Code NVARCHAR(50) = NULL,
    @V_Email NVARCHAR(255) = NULL,
    @V_Phone NVARCHAR(30) = NULL,
    @V_TaxNumber NVARCHAR(100) = NULL,
    @V_Address NVARCHAR(MAX) = NULL,
    @TenantId UNIQUEIDENTIFIER = NULL,
    @CreatedBy UNIQUEIDENTIFIER = NULL,
    @UpdatedBy UNIQUEIDENTIFIER = NULL,
    @ColumnName NVARCHAR(100) = NULL,
    @Value NVARCHAR(MAX) = NULL,
    @ExcludeId UNIQUEIDENTIFIER = NULL,
    @NewId UNIQUEIDENTIFIER = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF @Action = 'GetAll'
    BEGIN
        SELECT V_Id, V_TenantId, V_Name, V_Code, V_Email, V_Phone, V_TaxNumber, V_Address,
               V_CreatedAt, V_UpdatedAt
        FROM dbo.Vendors_V
        ORDER BY V_Name;
    END

    ELSE IF @Action = 'GetById'
    BEGIN
        SELECT V_Id, V_TenantId, V_Name, V_Code, V_Email, V_Phone, V_TaxNumber, V_Address,
               V_CreatedAt, V_UpdatedAt
        FROM dbo.Vendors_V
        WHERE V_Id = @V_Id;
    END

    ELSE IF @Action = 'Insert'
    BEGIN
        SET @NewId = NEWID();
        INSERT INTO dbo.Vendors_V (V_Id, V_TenantId, V_Name, V_Code, V_Email, V_Phone, V_TaxNumber, V_Address)
        VALUES (@NewId, @TenantId, @V_Name, @V_Code, @V_Email, @V_Phone, @V_TaxNumber, @V_Address);
    END

    ELSE IF @Action = 'Update'
    BEGIN
        UPDATE dbo.Vendors_V
        SET V_Name = @V_Name,
            V_Code = @V_Code,
            V_Email = @V_Email,
            V_Phone = @V_Phone,
            V_TaxNumber = @V_TaxNumber,
            V_Address = @V_Address,
            V_UpdatedAt = SYSUTCDATETIME()
        WHERE V_Id = @V_Id;
    END

    ELSE IF @Action = 'Deactivate'
    BEGIN
        RAISERROR('Vendors_V does not support soft delete.', 16, 1);
    END

    ELSE IF @Action = 'ExistsByField'
    BEGIN
        DECLARE @Sql NVARCHAR(MAX);
        SET @Sql = N'SELECT COUNT(1) FROM dbo.Vendors_V WHERE ' + QUOTENAME(@ColumnName) + N' = @P_Val';
        IF @ExcludeId IS NOT NULL
            SET @Sql = @Sql + N' AND V_Id <> @P_ExId';
        EXEC sp_executesql @Sql, N'@P_Val NVARCHAR(MAX), @P_ExId UNIQUEIDENTIFIER', @Value, @ExcludeId;
    END
END
GO

-- 18. USP_NotificationTemplates_NT (NotificationTemplates_NT)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[USP_NotificationTemplates_NT]') AND type in (N'P'))
    DROP PROCEDURE [dbo].[USP_NotificationTemplates_NT];
GO
CREATE PROCEDURE [dbo].[USP_NotificationTemplates_NT]
    @Action NVARCHAR(20),
    @NT_Id UNIQUEIDENTIFIER = NULL,
    @NT_Name NVARCHAR(150) = NULL,
    @NT_EventKey NVARCHAR(100) = NULL,
    @NT_Channel NVARCHAR(20) = NULL,
    @NT_Subject NVARCHAR(255) = NULL,
    @NT_BodyTemplate NVARCHAR(MAX) = NULL,
    @NT_IsActive BIT = NULL,
    @TenantId UNIQUEIDENTIFIER = NULL,
    @CreatedBy UNIQUEIDENTIFIER = NULL,
    @UpdatedBy UNIQUEIDENTIFIER = NULL,
    @ColumnName NVARCHAR(100) = NULL,
    @Value NVARCHAR(MAX) = NULL,
    @ExcludeId UNIQUEIDENTIFIER = NULL,
    @NewId UNIQUEIDENTIFIER = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF @Action = 'GetAll'
    BEGIN
        SELECT NT_Id, NT_TenantId, NT_Name, NT_EventKey, NT_Channel, NT_Subject, NT_BodyTemplate,
               NT_IsActive, NT_CreatedAt, NT_UpdatedAt
        FROM dbo.NotificationTemplates_NT
        WHERE NT_IsActive = 1
        ORDER BY NT_Name;
    END

    ELSE IF @Action = 'GetById'
    BEGIN
        SELECT NT_Id, NT_TenantId, NT_Name, NT_EventKey, NT_Channel, NT_Subject, NT_BodyTemplate,
               NT_IsActive, NT_CreatedAt, NT_UpdatedAt
        FROM dbo.NotificationTemplates_NT
        WHERE NT_Id = @NT_Id;
    END

    ELSE IF @Action = 'Insert'
    BEGIN
        SET @NewId = NEWID();
        INSERT INTO dbo.NotificationTemplates_NT (NT_Id, NT_TenantId, NT_Name, NT_EventKey, NT_Channel, NT_Subject, NT_BodyTemplate, NT_IsActive)
        VALUES (@NewId, @TenantId, @NT_Name, @NT_EventKey, @NT_Channel, @NT_Subject, @NT_BodyTemplate, ISNULL(@NT_IsActive, 1));
    END

    ELSE IF @Action = 'Update'
    BEGIN
        UPDATE dbo.NotificationTemplates_NT
        SET NT_Name = @NT_Name,
            NT_EventKey = @NT_EventKey,
            NT_Channel = @NT_Channel,
            NT_Subject = @NT_Subject,
            NT_BodyTemplate = @NT_BodyTemplate,
            NT_IsActive = @NT_IsActive,
            NT_UpdatedAt = SYSUTCDATETIME()
        WHERE NT_Id = @NT_Id;
    END

    ELSE IF @Action = 'Deactivate'
    BEGIN
        UPDATE dbo.NotificationTemplates_NT
        SET NT_IsActive = 0, NT_UpdatedAt = SYSUTCDATETIME()
        WHERE NT_Id = @NT_Id;
    END

    ELSE IF @Action = 'ExistsByField'
    BEGIN
        DECLARE @Sql NVARCHAR(MAX);
        SET @Sql = N'SELECT COUNT(1) FROM dbo.NotificationTemplates_NT WHERE ' + QUOTENAME(@ColumnName) + N' = @P_Val AND NT_IsActive = 1';
        IF @ExcludeId IS NOT NULL
            SET @Sql = @Sql + N' AND NT_Id <> @P_ExId';
        EXEC sp_executesql @Sql, N'@P_Val NVARCHAR(MAX), @P_ExId UNIQUEIDENTIFIER', @Value, @ExcludeId;
    END
END
GO

PRINT 'All 18 Master Module Stored Procedures created successfully.';
GO
