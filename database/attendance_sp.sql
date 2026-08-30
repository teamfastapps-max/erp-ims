-- ============================================================
-- USP_AttendanceSessions_AS
-- ============================================================
ALTER PROCEDURE [dbo].[USP_AttendanceSessions_AS]
    @Action NVARCHAR(20),
    @AS_Id UNIQUEIDENTIFIER = NULL,
    @AS_TenantId UNIQUEIDENTIFIER = NULL,
    @AS_BranchId UNIQUEIDENTIFIER = NULL,
    @AS_BatchId UNIQUEIDENTIFIER = NULL,
    @AS_SubjectId UNIQUEIDENTIFIER = NULL,
    @AS_StaffId UNIQUEIDENTIFIER = NULL,
    @AS_AttendanceDate DATE = NULL,
    @AS_StartTime TIME(7) = NULL,
    @AS_EndTime TIME(7) = NULL,
    @AS_Remarks NVARCHAR(MAX) = NULL,
    @SearchTerm NVARCHAR(255) = NULL,
    @BranchId UNIQUEIDENTIFIER = NULL,
    @BatchId UNIQUEIDENTIFIER = NULL,
    @Date DATE = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        IF @Action = 'GetById'
        BEGIN
            SELECT s.*,
                   b.B_Name AS BranchName,
                   bt.BT_Name AS BatchName,
                   sb.SB_Name AS SubjectName,
                   st.ST_FirstName + ' ' + st.ST_LastName AS StaffName
            FROM AttendanceSessions_AS s
            LEFT JOIN Branches_B b ON s.AS_BranchId = b.B_Id
            LEFT JOIN Batches_BT bt ON s.AS_BatchId = bt.BT_Id
            LEFT JOIN Subjects_SB sb ON s.AS_SubjectId = sb.SB_Id
            LEFT JOIN Staff_ST st ON s.AS_StaffId = st.ST_Id
            WHERE s.AS_Id = @AS_Id AND s.AS_TenantId = @AS_TenantId;
        END

        IF @Action = 'GetPaged'
        BEGIN
            SELECT COUNT(*) AS TotalCount
            FROM AttendanceSessions_AS s
            LEFT JOIN Batches_BT bt ON s.AS_BatchId = bt.BT_Id
            LEFT JOIN Subjects_SB sb ON s.AS_SubjectId = sb.SB_Id
            WHERE s.AS_TenantId = @AS_TenantId
              AND (@SearchTerm IS NULL OR bt.BT_Name LIKE '%' + @SearchTerm + '%' OR sb.SB_Name LIKE '%' + @SearchTerm + '%')
              AND (@BranchId IS NULL OR s.AS_BranchId = @BranchId)
              AND (@BatchId IS NULL OR s.AS_BatchId = @BatchId)
              AND (@Date IS NULL OR s.AS_AttendanceDate = @Date);

            SELECT s.*,
                   b.B_Name AS BranchName,
                   bt.BT_Name AS BatchName,
                   sb.SB_Name AS SubjectName,
                   st.ST_FirstName + ' ' + st.ST_LastName AS StaffName
            FROM AttendanceSessions_AS s
            LEFT JOIN Branches_B b ON s.AS_BranchId = b.B_Id
            LEFT JOIN Batches_BT bt ON s.AS_BatchId = bt.BT_Id
            LEFT JOIN Subjects_SB sb ON s.AS_SubjectId = sb.SB_Id
            LEFT JOIN Staff_ST st ON s.AS_StaffId = st.ST_Id
            WHERE s.AS_TenantId = @AS_TenantId
              AND (@SearchTerm IS NULL OR bt.BT_Name LIKE '%' + @SearchTerm + '%' OR sb.SB_Name LIKE '%' + @SearchTerm + '%')
              AND (@BranchId IS NULL OR s.AS_BranchId = @BranchId)
              AND (@BatchId IS NULL OR s.AS_BatchId = @BatchId)
              AND (@Date IS NULL OR s.AS_AttendanceDate = @Date)
            ORDER BY s.AS_AttendanceDate DESC, s.AS_CreatedAt DESC
            OFFSET (@PageNumber - 1) * @PageSize ROWS
            FETCH NEXT @PageSize ROWS ONLY;
        END

        IF @Action = 'Insert'
        BEGIN
            INSERT INTO AttendanceSessions_AS (AS_Id, AS_TenantId, AS_BranchId, AS_BatchId, AS_SubjectId, AS_StaffId, AS_AttendanceDate, AS_StartTime, AS_EndTime, AS_Remarks, AS_CreatedAt, AS_UpdatedAt)
            VALUES (@AS_Id, @AS_TenantId, @AS_BranchId, @AS_BatchId, @AS_SubjectId, @AS_StaffId, @AS_AttendanceDate, @AS_StartTime, @AS_EndTime, @AS_Remarks, SYSUTCDATETIME(), SYSUTCDATETIME());

            SELECT @AS_Id;
        END

        IF @Action = 'Update'
        BEGIN
            UPDATE AttendanceSessions_AS
            SET AS_BranchId = @AS_BranchId,
                AS_BatchId = @AS_BatchId,
                AS_SubjectId = @AS_SubjectId,
                AS_StaffId = @AS_StaffId,
                AS_AttendanceDate = @AS_AttendanceDate,
                AS_StartTime = @AS_StartTime,
                AS_EndTime = @AS_EndTime,
                AS_Remarks = @AS_Remarks,
                AS_UpdatedAt = SYSUTCDATETIME()
            WHERE AS_Id = @AS_Id AND AS_TenantId = @AS_TenantId;

            SELECT @@ROWCOUNT;
        END

        IF @Action = 'Delete'
        BEGIN
            DELETE FROM AttendanceSessions_AS WHERE AS_Id = @AS_Id AND AS_TenantId = @AS_TenantId;
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

-- ============================================================
-- USP_AttendanceRecords_AR
-- ============================================================
ALTER PROCEDURE [dbo].[USP_AttendanceRecords_AR]
    @Action NVARCHAR(20),
    @AR_Id UNIQUEIDENTIFIER = NULL,
    @AR_AttendanceSessionId UNIQUEIDENTIFIER = NULL,
    @AR_StudentId UNIQUEIDENTIFIER = NULL,
    @AR_Status NVARCHAR(20) = NULL,
    @AR_Remarks NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        IF @Action = 'GetBySession'
        BEGIN
            SELECT ar.*,
                   s.S_FirstName + ' ' + s.S_LastName AS StudentName,
                   s.S_StudentCode AS StudentCode
            FROM AttendanceRecords_AR ar
            LEFT JOIN Students_S s ON ar.AR_StudentId = s.S_Id
            WHERE ar.AR_AttendanceSessionId = @AR_AttendanceSessionId
            ORDER BY s.S_FirstName, s.S_LastName;
        END

        IF @Action = 'Insert'
        BEGIN
            INSERT INTO AttendanceRecords_AR (AR_Id, AR_AttendanceSessionId, AR_StudentId, AR_Status, AR_Remarks, AR_CreatedAt, AR_UpdatedAt)
            VALUES (@AR_Id, @AR_AttendanceSessionId, @AR_StudentId, @AR_Status, @AR_Remarks, SYSUTCDATETIME(), SYSUTCDATETIME());
        END

        IF @Action = 'DeleteBySession'
        BEGIN
            DELETE FROM AttendanceRecords_AR WHERE AR_AttendanceSessionId = @AR_AttendanceSessionId;
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
