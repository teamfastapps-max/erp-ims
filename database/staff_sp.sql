-- ============================================================
-- USP_Staff_ST
-- ============================================================
ALTER PROCEDURE [dbo].[USP_Staff_ST]
    @Action NVARCHAR(20),
    @ST_Id UNIQUEIDENTIFIER = NULL,
    @ST_TenantId UNIQUEIDENTIFIER = NULL,
    @ST_BranchId UNIQUEIDENTIFIER = NULL,
    @ST_DepartmentId UNIQUEIDENTIFIER = NULL,
    @ST_DesignationId UNIQUEIDENTIFIER = NULL,
    @ST_EmployeeCode NVARCHAR(50) = NULL,
    @ST_FirstName NVARCHAR(100) = NULL,
    @ST_LastName NVARCHAR(100) = NULL,
    @ST_Email NVARCHAR(255) = NULL,
    @ST_Phone NVARCHAR(30) = NULL,
    @ST_JoiningDate DATE = NULL,
    @ST_Status NVARCHAR(20) = NULL,
    @ExcludeId UNIQUEIDENTIFIER = NULL,
    @SearchTerm NVARCHAR(255) = NULL,
    @BranchId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(20) = NULL,
    @TenantId UNIQUEIDENTIFIER = NULL,
    @CreatedBy UNIQUEIDENTIFIER = NULL,
    @UpdatedBy UNIQUEIDENTIFIER = NULL,
    @ColumnName NVARCHAR(100) = NULL,
    @Value NVARCHAR(MAX) = NULL,
    @NewId UNIQUEIDENTIFIER = NULL OUTPUT,
    @PageNumber INT = 1,
    @PageSize INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        IF @Action = 'GetAll'
        BEGIN
            SELECT ST_Id, ST_TenantId, ST_FirstName, ST_LastName, ST_EmployeeCode, ST_Email, ST_Status
            FROM Staff_ST
            WHERE ST_DeletedAt IS NULL
            ORDER BY ST_FirstName, ST_LastName;
        END

        IF @Action = 'GetById'
        BEGIN
            SELECT s.*,
                   b.B_Name AS BranchName,
                   d.D_Name AS DepartmentName,
                   ds.DS_Name AS DesignationName
            FROM Staff_ST s
            LEFT JOIN Branches_B b ON s.ST_BranchId = b.B_Id
            LEFT JOIN Departments_D d ON s.ST_DepartmentId = d.D_Id
            LEFT JOIN Designations_DS ds ON s.ST_DesignationId = ds.DS_Id
            WHERE s.ST_Id = @ST_Id AND s.ST_TenantId = @ST_TenantId AND s.ST_DeletedAt IS NULL;
        END

        IF @Action = 'GetPaged'
        BEGIN
            SELECT COUNT(*) AS TotalCount
            FROM Staff_ST s
            WHERE s.ST_TenantId = @ST_TenantId AND s.ST_DeletedAt IS NULL
              AND (@SearchTerm IS NULL OR s.ST_FirstName LIKE '%' + @SearchTerm + '%' OR s.ST_LastName LIKE '%' + @SearchTerm + '%' OR s.ST_EmployeeCode LIKE '%' + @SearchTerm + '%' OR s.ST_Email LIKE '%' + @SearchTerm + '%')
              AND (@BranchId IS NULL OR s.ST_BranchId = @BranchId)
              AND (@Status IS NULL OR s.ST_Status = @Status);

            SELECT s.*,
                   b.B_Name AS BranchName,
                   d.D_Name AS DepartmentName,
                   ds.DS_Name AS DesignationName
            FROM Staff_ST s
            LEFT JOIN Branches_B b ON s.ST_BranchId = b.B_Id
            LEFT JOIN Departments_D d ON s.ST_DepartmentId = d.D_Id
            LEFT JOIN Designations_DS ds ON s.ST_DesignationId = ds.DS_Id
            WHERE s.ST_TenantId = @ST_TenantId AND s.ST_DeletedAt IS NULL
              AND (@SearchTerm IS NULL OR s.ST_FirstName LIKE '%' + @SearchTerm + '%' OR s.ST_LastName LIKE '%' + @SearchTerm + '%' OR s.ST_EmployeeCode LIKE '%' + @SearchTerm + '%' OR s.ST_Email LIKE '%' + @SearchTerm + '%')
              AND (@BranchId IS NULL OR s.ST_BranchId = @BranchId)
              AND (@Status IS NULL OR s.ST_Status = @Status)
            ORDER BY s.ST_FirstName, s.ST_LastName
            OFFSET (@PageNumber - 1) * @PageSize ROWS
            FETCH NEXT @PageSize ROWS ONLY;
        END

        IF @Action = 'ExistsByCode'
        BEGIN
            SELECT COUNT(*) FROM Staff_ST
            WHERE ST_TenantId = @ST_TenantId AND ST_EmployeeCode = @ST_EmployeeCode
              AND (@ExcludeId IS NULL OR ST_Id <> @ExcludeId);
        END

        IF @Action = 'Insert'
        BEGIN
            INSERT INTO Staff_ST (ST_Id, ST_TenantId, ST_BranchId, ST_DepartmentId, ST_DesignationId, ST_EmployeeCode, ST_FirstName, ST_LastName, ST_Email, ST_Phone, ST_JoiningDate, ST_Status, ST_CreatedAt, ST_UpdatedAt)
            VALUES (@ST_Id, @ST_TenantId, @ST_BranchId, @ST_DepartmentId, @ST_DesignationId, @ST_EmployeeCode, @ST_FirstName, @ST_LastName, @ST_Email, @ST_Phone, @ST_JoiningDate, @ST_Status, SYSUTCDATETIME(), SYSUTCDATETIME());

            SELECT @ST_Id;
        END

        IF @Action = 'Update'
        BEGIN
            UPDATE Staff_ST
            SET ST_BranchId = @ST_BranchId,
                ST_DepartmentId = @ST_DepartmentId,
                ST_DesignationId = @ST_DesignationId,
                ST_EmployeeCode = @ST_EmployeeCode,
                ST_FirstName = @ST_FirstName,
                ST_LastName = @ST_LastName,
                ST_Email = @ST_Email,
                ST_Phone = @ST_Phone,
                ST_JoiningDate = @ST_JoiningDate,
                ST_Status = @ST_Status,
                ST_UpdatedAt = SYSUTCDATETIME()
            WHERE ST_Id = @ST_Id AND ST_TenantId = @ST_TenantId;

            SELECT @@ROWCOUNT;
        END

        IF @Action = 'Delete'
        BEGIN
            UPDATE Staff_ST SET ST_DeletedAt = SYSUTCDATETIME(), ST_UpdatedAt = SYSUTCDATETIME()
            WHERE ST_Id = @ST_Id AND ST_TenantId = @ST_TenantId;

            SELECT @@ROWCOUNT;
        END
    END TRY
    BEGIN CATCH
        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrSev INT = ERROR_SEVERITY();
        DECLARE @ErrState INT = ERROR_STATE();
        RAISERROR(@ErrMsg, @ErrSev, @ErrState);
    END CATCH
END
GO
