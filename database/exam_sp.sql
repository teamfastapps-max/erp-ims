-- ============================================================
-- USP_Exams_EX
-- ============================================================
ALTER PROCEDURE [dbo].[USP_Exams_EX]
    @Action NVARCHAR(20),
    @EX_Id UNIQUEIDENTIFIER = NULL,
    @EX_TenantId UNIQUEIDENTIFIER = NULL,
    @EX_AcademicYearId UNIQUEIDENTIFIER = NULL,
    @EX_CourseId UNIQUEIDENTIFIER = NULL,
    @EX_BatchId UNIQUEIDENTIFIER = NULL,
    @EX_ExamTypeId UNIQUEIDENTIFIER = NULL,
    @EX_Name NVARCHAR(150) = NULL,
    @EX_Code NVARCHAR(50) = NULL,
    @EX_StartDate DATE = NULL,
    @EX_EndDate DATE = NULL,
    @EX_Status NVARCHAR(20) = NULL,
    @ExcludeId UNIQUEIDENTIFIER = NULL,
    @SearchTerm NVARCHAR(255) = NULL,
    @CourseId UNIQUEIDENTIFIER = NULL,
    @BatchId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(20) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        IF @Action = 'GetById'
        BEGIN
            SELECT e.*,
                   ay.AY_Name AS AcademicYearName,
                   c.C_Name AS CourseName,
                   bt.BT_Name AS BatchName,
                   et.ET_Name AS ExamTypeName
            FROM Exams_EX e
            LEFT JOIN AcademicYears_AY ay ON e.EX_AcademicYearId = ay.AY_Id
            LEFT JOIN Courses_C c ON e.EX_CourseId = c.C_Id
            LEFT JOIN Batches_BT bt ON e.EX_BatchId = bt.BT_Id
            LEFT JOIN ExamTypes_ET et ON e.EX_ExamTypeId = et.ET_Id
            WHERE e.EX_Id = @EX_Id AND e.EX_TenantId = @EX_TenantId;
        END

        IF @Action = 'GetPaged'
        BEGIN
            SELECT COUNT(*) AS TotalCount
            FROM Exams_EX e
            WHERE e.EX_TenantId = @EX_TenantId
              AND (@SearchTerm IS NULL OR e.EX_Name LIKE '%' + @SearchTerm + '%' OR e.EX_Code LIKE '%' + @SearchTerm + '%')
              AND (@CourseId IS NULL OR e.EX_CourseId = @CourseId)
              AND (@BatchId IS NULL OR e.EX_BatchId = @BatchId)
              AND (@Status IS NULL OR e.EX_Status = @Status);

            SELECT e.*,
                   ay.AY_Name AS AcademicYearName,
                   c.C_Name AS CourseName,
                   bt.BT_Name AS BatchName,
                   et.ET_Name AS ExamTypeName
            FROM Exams_EX e
            LEFT JOIN AcademicYears_AY ay ON e.EX_AcademicYearId = ay.AY_Id
            LEFT JOIN Courses_C c ON e.EX_CourseId = c.C_Id
            LEFT JOIN Batches_BT bt ON e.EX_BatchId = bt.BT_Id
            LEFT JOIN ExamTypes_ET et ON e.EX_ExamTypeId = et.ET_Id
            WHERE e.EX_TenantId = @EX_TenantId
              AND (@SearchTerm IS NULL OR e.EX_Name LIKE '%' + @SearchTerm + '%' OR e.EX_Code LIKE '%' + @SearchTerm + '%')
              AND (@CourseId IS NULL OR e.EX_CourseId = @CourseId)
              AND (@BatchId IS NULL OR e.EX_BatchId = @BatchId)
              AND (@Status IS NULL OR e.EX_Status = @Status)
            ORDER BY e.EX_StartDate DESC
            OFFSET (@PageNumber - 1) * @PageSize ROWS
            FETCH NEXT @PageSize ROWS ONLY;
        END

        IF @Action = 'ExistsByCode'
        BEGIN
            SELECT COUNT(*) FROM Exams_EX
            WHERE EX_TenantId = @EX_TenantId AND EX_Code = @EX_Code
              AND (@ExcludeId IS NULL OR EX_Id <> @ExcludeId);
        END

        IF @Action = 'Insert'
        BEGIN
            INSERT INTO Exams_EX (EX_Id, EX_TenantId, EX_AcademicYearId, EX_CourseId, EX_BatchId, EX_ExamTypeId, EX_Name, EX_Code, EX_StartDate, EX_EndDate, EX_Status, EX_CreatedAt, EX_UpdatedAt)
            VALUES (@EX_Id, @EX_TenantId, @EX_AcademicYearId, @EX_CourseId, @EX_BatchId, @EX_ExamTypeId, @EX_Name, @EX_Code, @EX_StartDate, @EX_EndDate, @EX_Status, SYSUTCDATETIME(), SYSUTCDATETIME());
            SELECT @EX_Id;
        END

        IF @Action = 'Update'
        BEGIN
            UPDATE Exams_EX
            SET EX_AcademicYearId = @EX_AcademicYearId, EX_CourseId = @EX_CourseId, EX_BatchId = @EX_BatchId,
                EX_ExamTypeId = @EX_ExamTypeId, EX_Name = @EX_Name, EX_Code = @EX_Code,
                EX_StartDate = @EX_StartDate, EX_EndDate = @EX_EndDate, EX_Status = @EX_Status,
                EX_UpdatedAt = SYSUTCDATETIME()
            WHERE EX_Id = @EX_Id AND EX_TenantId = @EX_TenantId;
            SELECT @@ROWCOUNT;
        END

        IF @Action = 'Delete'
        BEGIN
            DELETE FROM Exams_EX WHERE EX_Id = @EX_Id AND EX_TenantId = @EX_TenantId;
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
-- USP_ExamSubjects_ES
-- ============================================================
ALTER PROCEDURE [dbo].[USP_ExamSubjects_ES]
    @Action NVARCHAR(20),
    @ES_Id UNIQUEIDENTIFIER = NULL,
    @ES_ExamId UNIQUEIDENTIFIER = NULL,
    @ES_SubjectId UNIQUEIDENTIFIER = NULL,
    @ES_MaxMarks DECIMAL(8,2) = NULL,
    @ES_PassMarks DECIMAL(8,2) = NULL,
    @ES_Weightage DECIMAL(5,2) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        IF @Action = 'GetByExam'
        BEGIN
            SELECT es.*, sb.SB_Name AS SubjectName
            FROM ExamSubjects_ES es
            LEFT JOIN Subjects_SB sb ON es.ES_SubjectId = sb.SB_Id
            WHERE es.ES_ExamId = @ES_ExamId;
        END

        IF @Action = 'Insert'
        BEGIN
            INSERT INTO ExamSubjects_ES (ES_Id, ES_ExamId, ES_SubjectId, ES_MaxMarks, ES_PassMarks, ES_Weightage)
            VALUES (@ES_Id, @ES_ExamId, @ES_SubjectId, @ES_MaxMarks, @ES_PassMarks, @ES_Weightage);
        END

        IF @Action = 'DeleteByExam'
        BEGIN
            DELETE FROM ExamSubjects_ES WHERE ES_ExamId = @ES_ExamId;
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
-- USP_Marks_M
-- ============================================================
ALTER PROCEDURE [dbo].[USP_Marks_M]
    @Action NVARCHAR(20),
    @M_Id UNIQUEIDENTIFIER = NULL,
    @M_ExamSubjectId UNIQUEIDENTIFIER = NULL,
    @M_StudentId UNIQUEIDENTIFIER = NULL,
    @M_MarksObtained DECIMAL(8,2) = NULL,
    @M_Remarks NVARCHAR(MAX) = NULL,
    @ExamId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        IF @Action = 'GetByExam'
        BEGIN
            SELECT m.*, s.S_FirstName + ' ' + s.S_LastName AS StudentName, s.S_StudentCode AS StudentCode,
                   CASE WHEN m.M_MarksObtained >= es.ES_PassMarks THEN 'Pass' ELSE 'Fail' END AS Grade
            FROM Marks_M m
            INNER JOIN ExamSubjects_ES es ON m.M_ExamSubjectId = es.ES_Id
            LEFT JOIN Students_S s ON m.M_StudentId = s.S_Id
            WHERE es.ES_ExamId = @ExamId;
        END

        IF @Action = 'Upsert'
        BEGIN
            IF EXISTS (SELECT 1 FROM Marks_M WHERE M_ExamSubjectId = @M_ExamSubjectId AND M_StudentId = @M_StudentId)
            BEGIN
                UPDATE Marks_M SET M_MarksObtained = @M_MarksObtained, M_Remarks = @M_Remarks, M_UpdatedAt = SYSUTCDATETIME()
                WHERE M_ExamSubjectId = @M_ExamSubjectId AND M_StudentId = @M_StudentId;
            END
            ELSE
            BEGIN
                INSERT INTO Marks_M (M_Id, M_ExamSubjectId, M_StudentId, M_MarksObtained, M_Remarks, M_CreatedAt, M_UpdatedAt)
                VALUES (@M_Id, @M_ExamSubjectId, @M_StudentId, @M_MarksObtained, @M_Remarks, SYSUTCDATETIME(), SYSUTCDATETIME());
            END
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
-- USP_Results_R
-- ============================================================
ALTER PROCEDURE [dbo].[USP_Results_R]
    @Action NVARCHAR(20),
    @R_Id UNIQUEIDENTIFIER = NULL,
    @R_ExamId UNIQUEIDENTIFIER = NULL,
    @R_StudentId UNIQUEIDENTIFIER = NULL,
    @R_TotalMarks DECIMAL(10,2) = NULL,
    @R_MarksObtained DECIMAL(10,2) = NULL,
    @R_Percentage DECIMAL(6,2) = NULL,
    @R_Grade NVARCHAR(20) = NULL,
    @R_ResultStatus NVARCHAR(30) = NULL,
    @R_Remarks NVARCHAR(MAX) = NULL,
    @ExamId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        IF @Action = 'GetByExam'
        BEGIN
            SELECT r.*, s.S_FirstName + ' ' + s.S_LastName AS StudentName, s.S_StudentCode AS StudentCode
            FROM Results_R r
            LEFT JOIN Students_S s ON r.R_StudentId = s.S_Id
            WHERE r.R_ExamId = @ExamId;
        END

        IF @Action = 'Insert'
        BEGIN
            INSERT INTO Results_R (R_Id, R_ExamId, R_StudentId, R_TotalMarks, R_MarksObtained, R_Percentage, R_Grade, R_ResultStatus, R_Remarks, R_CreatedAt, R_UpdatedAt)
            VALUES (@R_Id, @R_ExamId, @R_StudentId, @R_TotalMarks, @R_MarksObtained, @R_Percentage, @R_Grade, @R_ResultStatus, @R_Remarks, SYSUTCDATETIME(), SYSUTCDATETIME());
        END

        IF @Action = 'DeleteByExam'
        BEGIN
            DELETE FROM Results_R WHERE R_ExamId = @R_ExamId;
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
