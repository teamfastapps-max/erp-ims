-- ============================================================
-- USP_Students_S
-- Lookup SP for Student entity used by MasterConfigRegistry
-- ============================================================
CREATE PROCEDURE [dbo].[USP_Students_S]
    @Action NVARCHAR(20),
    @S_Id UNIQUEIDENTIFIER = NULL,
    @S_TenantId UNIQUEIDENTIFIER = NULL,
    @S_FirstName NVARCHAR(100) = NULL,
    @S_LastName NVARCHAR(100) = NULL,
    @S_StudentCode NVARCHAR(50) = NULL,
    @TenantId UNIQUEIDENTIFIER = NULL,
    @UpdatedBy UNIQUEIDENTIFIER = NULL,
    @ColumnName NVARCHAR(100) = NULL,
    @Value NVARCHAR(MAX) = NULL,
    @ExcludeId UNIQUEIDENTIFIER = NULL,
    @NewId UNIQUEIDENTIFIER = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        IF @Action = 'GetAll'
        BEGIN
            SELECT S_Id, S_TenantId, S_FirstName, S_LastName, S_StudentCode, S_Email, S_Phone, S_Status
            FROM Students_S
            WHERE S_DeletedAt IS NULL
            ORDER BY S_FirstName, S_LastName;
        END

        IF @Action = 'GetById'
        BEGIN
            SELECT * FROM Students_S WHERE S_Id = @S_Id;
        END

        IF @Action = 'ExistsByField'
        BEGIN
            DECLARE @Sql NVARCHAR(MAX);
            SET @Sql = 'SELECT COUNT(*) FROM Students_S WHERE ' + QUOTENAME(@ColumnName) + ' = @Value'
                      + CASE WHEN @ExcludeId IS NOT NULL THEN ' AND S_Id <> @ExcludeId' ELSE '' END;
            EXEC sp_executesql @Sql, N'@Value NVARCHAR(MAX), @ExcludeId UNIQUEIDENTIFIER', @Value, @ExcludeId;
        END

        IF @Action = 'Insert'
        BEGIN
            SET @NewId = ISNULL(@NewId, NEWID());
            INSERT INTO Students_S (S_Id, S_TenantId, S_FirstName, S_LastName, S_StudentCode, S_AdmissionNumber, S_Status, S_CreatedAt, S_UpdatedAt)
            VALUES (@NewId, @TenantId, @S_FirstName, @S_LastName, @S_StudentCode, @S_StudentCode, 'Active', SYSUTCDATETIME(), SYSUTCDATETIME());
        END

        IF @Action = 'Update'
        BEGIN
            UPDATE Students_S
            SET S_FirstName = @S_FirstName, S_LastName = @S_LastName, S_StudentCode = @S_StudentCode,
                S_UpdatedAt = SYSUTCDATETIME()
            WHERE S_Id = @S_Id;
        END

        IF @Action = 'Delete'
        BEGIN
            DELETE FROM Students_S WHERE S_Id = @S_Id;
        END

        IF @Action = 'Deactivate'
        BEGIN
            UPDATE Students_S SET S_Status = 'inactive', S_UpdatedAt = SYSUTCDATETIME() WHERE S_Id = @S_Id;
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
