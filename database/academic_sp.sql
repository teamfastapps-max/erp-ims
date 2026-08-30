-- Academic Module Stored Procedures
-- Batches, CourseSubjects, Enrollments, Timetables, AdmissionApplications

-- ============================================================================
-- 1. USP_Batches_BT
-- ============================================================================
CREATE PROCEDURE [dbo].[USP_Batches_BT]
    @Action NVARCHAR(20),
    @BT_Id UNIQUEIDENTIFIER = NULL,
    @BT_TenantId UNIQUEIDENTIFIER = NULL,
    @BT_BranchId UNIQUEIDENTIFIER = NULL,
    @BT_CourseId UNIQUEIDENTIFIER = NULL,
    @BT_AcademicYearId UNIQUEIDENTIFIER = NULL,
    @BT_Name NVARCHAR(150) = NULL,
    @BT_Code NVARCHAR(50) = NULL,
    @BT_StartDate DATE = NULL,
    @BT_EndDate DATE = NULL,
    @BT_Capacity INT = NULL,
    @BT_Status NVARCHAR(20) = NULL,
    @SearchTerm NVARCHAR(255) = NULL,
    @BranchId UNIQUEIDENTIFIER = NULL,
    @CourseId UNIQUEIDENTIFIER = NULL,
    @AcademicYearId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(20) = NULL,
    @ExcludeId UNIQUEIDENTIFIER = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        IF @Action = 'GetAll'
        BEGIN
            SELECT bt.*, c.C_Name AS CourseName, ay.AY_Name AS AcademicYearName,
                   (SELECT COUNT(*) FROM BatchStudents_BS bs WHERE bs.BS_BatchId = bt.BT_Id AND bs.BS_LeftAt IS NULL) AS EnrolledCount
            FROM Batches_BT bt
            LEFT JOIN Courses_C c ON bt.BT_CourseId = c.C_Id
            LEFT JOIN AcademicYears_AY ay ON bt.BT_AcademicYearId = ay.AY_Id
            ORDER BY bt.BT_Name;
        END

        ELSE IF @Action = 'GetById'
        BEGIN
            SELECT bt.*, c.C_Name AS CourseName, ay.AY_Name AS AcademicYearName,
                   (SELECT COUNT(*) FROM BatchStudents_BS bs WHERE bs.BS_BatchId = bt.BT_Id AND bs.BS_LeftAt IS NULL) AS EnrolledCount
            FROM Batches_BT bt
            LEFT JOIN Courses_C c ON bt.BT_CourseId = c.C_Id
            LEFT JOIN AcademicYears_AY ay ON bt.BT_AcademicYearId = ay.AY_Id
            WHERE bt.BT_Id = @BT_Id AND bt.BT_TenantId = @BT_TenantId;
        END

        ELSE IF @Action = 'GetPaged'
        BEGIN
            SELECT COUNT(*) AS TotalCount
            FROM Batches_BT bt
            WHERE bt.BT_TenantId = @BT_TenantId
              AND (@SearchTerm IS NULL OR bt.BT_Name LIKE '%' + @SearchTerm + '%' OR bt.BT_Code LIKE '%' + @SearchTerm + '%')
              AND (@BranchId IS NULL OR bt.BT_BranchId = @BranchId)
              AND (@CourseId IS NULL OR bt.BT_CourseId = @CourseId)
              AND (@AcademicYearId IS NULL OR bt.BT_AcademicYearId = @AcademicYearId)
              AND (@Status IS NULL OR bt.BT_Status = @Status);

            SELECT bt.*, c.C_Name AS CourseName, ay.AY_Name AS AcademicYearName,
                   (SELECT COUNT(*) FROM BatchStudents_BS bs WHERE bs.BS_BatchId = bt.BT_Id AND bs.BS_LeftAt IS NULL) AS EnrolledCount
            FROM Batches_BT bt
            LEFT JOIN Courses_C c ON bt.BT_CourseId = c.C_Id
            LEFT JOIN AcademicYears_AY ay ON bt.BT_AcademicYearId = ay.AY_Id
            WHERE bt.BT_TenantId = @BT_TenantId
              AND (@SearchTerm IS NULL OR bt.BT_Name LIKE '%' + @SearchTerm + '%' OR bt.BT_Code LIKE '%' + @SearchTerm + '%')
              AND (@BranchId IS NULL OR bt.BT_BranchId = @BranchId)
              AND (@CourseId IS NULL OR bt.BT_CourseId = @CourseId)
              AND (@AcademicYearId IS NULL OR bt.BT_AcademicYearId = @AcademicYearId)
              AND (@Status IS NULL OR bt.BT_Status = @Status)
            ORDER BY bt.BT_Name
            OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
        END

        ELSE IF @Action = 'Insert'
        BEGIN
            INSERT INTO Batches_BT (BT_Id, BT_TenantId, BT_BranchId, BT_CourseId, BT_AcademicYearId, BT_Name, BT_Code, BT_StartDate, BT_EndDate, BT_Capacity, BT_Status, BT_CreatedAt, BT_UpdatedAt)
            VALUES (@BT_Id, @BT_TenantId, @BT_BranchId, @BT_CourseId, @BT_AcademicYearId, @BT_Name, @BT_Code, @BT_StartDate, @BT_EndDate, @BT_Capacity, @BT_Status, GETUTCDATE(), GETUTCDATE());
            SELECT @BT_Id;
        END

        ELSE IF @Action = 'Update'
        BEGIN
            UPDATE Batches_BT SET
                BT_BranchId = @BT_BranchId, BT_CourseId = @BT_CourseId, BT_AcademicYearId = @BT_AcademicYearId,
                BT_Name = @BT_Name, BT_Code = @BT_Code, BT_StartDate = @BT_StartDate, BT_EndDate = @BT_EndDate,
                BT_Capacity = @BT_Capacity, BT_Status = @BT_Status, BT_UpdatedAt = GETUTCDATE()
            WHERE BT_Id = @BT_Id AND BT_TenantId = @BT_TenantId;
            SELECT @@ROWCOUNT;
        END

        ELSE IF @Action = 'Delete'
        BEGIN
            DELETE FROM Batches_BT WHERE BT_Id = @BT_Id AND BT_TenantId = @BT_TenantId;
            SELECT @@ROWCOUNT;
        END

        ELSE IF @Action = 'ExistsByCode'
        BEGIN
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM Batches_BT WHERE BT_TenantId = @BT_TenantId AND BT_Code = @BT_Code
                AND (@ExcludeId IS NULL OR BT_Id <> @ExcludeId)
            ) THEN 1 ELSE 0 END;
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

-- ============================================================================
-- 2. USP_CourseSubjects_CS
-- ============================================================================
CREATE PROCEDURE [dbo].[USP_CourseSubjects_CS]
    @Action NVARCHAR(20),
    @CS_CourseId UNIQUEIDENTIFIER = NULL,
    @CS_SubjectId UNIQUEIDENTIFIER = NULL,
    @CS_SequenceNo INT = NULL,
    @CS_IsMandatory BIT = NULL,
    @CS_MaxMarks DECIMAL(8,2) = NULL,
    @CS_PassMarks DECIMAL(8,2) = NULL,
    @TenantId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        IF @Action = 'GetAll'
        BEGIN
            SELECT cs.*, c.C_Name AS CourseName, s.SB_Name AS SubjectName
            FROM CourseSubjects_CS cs
            LEFT JOIN Courses_C c ON cs.CS_CourseId = c.C_Id
            LEFT JOIN Subjects_SB s ON cs.CS_SubjectId = s.SB_Id
            WHERE c.C_TenantId = @TenantId
            ORDER BY cs.CS_SequenceNo;
        END

        ELSE IF @Action = 'GetByCourseId'
        BEGIN
            SELECT cs.*, c.C_Name AS CourseName, s.SB_Name AS SubjectName
            FROM CourseSubjects_CS cs
            LEFT JOIN Courses_C c ON cs.CS_CourseId = c.C_Id
            LEFT JOIN Subjects_SB s ON cs.CS_SubjectId = s.SB_Id
            WHERE cs.CS_CourseId = @CS_CourseId AND c.C_TenantId = @TenantId
            ORDER BY cs.CS_SequenceNo;
        END

        ELSE IF @Action = 'GetById'
        BEGIN
            SELECT cs.*, c.C_Name AS CourseName, s.SB_Name AS SubjectName
            FROM CourseSubjects_CS cs
            LEFT JOIN Courses_C c ON cs.CS_CourseId = c.C_Id
            LEFT JOIN Subjects_SB s ON cs.CS_SubjectId = s.SB_Id
            WHERE cs.CS_CourseId = @CS_CourseId AND cs.CS_SubjectId = @CS_SubjectId AND c.C_TenantId = @TenantId;
        END

        ELSE IF @Action = 'Exists'
        BEGIN
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM CourseSubjects_CS WHERE CS_CourseId = @CS_CourseId AND CS_SubjectId = @CS_SubjectId
            ) THEN 1 ELSE 0 END;
        END

        ELSE IF @Action = 'Insert'
        BEGIN
            INSERT INTO CourseSubjects_CS (CS_CourseId, CS_SubjectId, CS_SequenceNo, CS_IsMandatory, CS_MaxMarks, CS_PassMarks)
            VALUES (@CS_CourseId, @CS_SubjectId, ISNULL(@CS_SequenceNo, 0), ISNULL(@CS_IsMandatory, 1), @CS_MaxMarks, @CS_PassMarks);
        END

        ELSE IF @Action = 'Update'
        BEGIN
            UPDATE CourseSubjects_CS SET
                CS_SequenceNo = ISNULL(@CS_SequenceNo, CS_SequenceNo),
                CS_IsMandatory = ISNULL(@CS_IsMandatory, CS_IsMandatory),
                CS_MaxMarks = @CS_MaxMarks,
                CS_PassMarks = @CS_PassMarks
            WHERE CS_CourseId = @CS_CourseId AND CS_SubjectId = @CS_SubjectId;
        END

        ELSE IF @Action = 'Delete'
        BEGIN
            DELETE FROM CourseSubjects_CS WHERE CS_CourseId = @CS_CourseId AND CS_SubjectId = @CS_SubjectId;
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

-- ============================================================================
-- 3. USP_Enrollments_E
-- ============================================================================
CREATE PROCEDURE [dbo].[USP_Enrollments_E]
    @Action NVARCHAR(20),
    @E_Id UNIQUEIDENTIFIER = NULL,
    @E_TenantId UNIQUEIDENTIFIER = NULL,
    @E_StudentId UNIQUEIDENTIFIER = NULL,
    @E_AcademicYearId UNIQUEIDENTIFIER = NULL,
    @E_CourseId UNIQUEIDENTIFIER = NULL,
    @E_BatchId UNIQUEIDENTIFIER = NULL,
    @E_EnrollmentNumber NVARCHAR(50) = NULL,
    @E_EnrollmentDate DATE = NULL,
    @E_Status NVARCHAR(20) = NULL,
    @E_CompletionDate DATE = NULL,
    @SearchTerm NVARCHAR(255) = NULL,
    @AcademicYearId UNIQUEIDENTIFIER = NULL,
    @CourseId UNIQUEIDENTIFIER = NULL,
    @BatchId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(20) = NULL,
    @ExcludeId UNIQUEIDENTIFIER = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        IF @Action = 'GetById'
        BEGIN
            SELECT e.*, s.S_FirstName + ' ' + ISNULL(s.S_MiddleName + ' ', '') + s.S_LastName AS StudentName,
                   c.C_Name AS CourseName, bt.BT_Name AS BatchName, ay.AY_Name AS AcademicYearName
            FROM Enrollments_E e
            LEFT JOIN Students_S s ON e.E_StudentId = s.S_Id
            LEFT JOIN Courses_C c ON e.E_CourseId = c.C_Id
            LEFT JOIN Batches_BT bt ON e.E_BatchId = bt.BT_Id
            LEFT JOIN AcademicYears_AY ay ON e.E_AcademicYearId = ay.AY_Id
            WHERE e.E_Id = @E_Id AND e.E_TenantId = @E_TenantId;
        END

        ELSE IF @Action = 'GetPaged'
        BEGIN
            SELECT COUNT(*) AS TotalCount
            FROM Enrollments_E e
            WHERE e.E_TenantId = @E_TenantId
              AND (@SearchTerm IS NULL OR e.E_EnrollmentNumber LIKE '%' + @SearchTerm + '%')
              AND (@AcademicYearId IS NULL OR e.E_AcademicYearId = @AcademicYearId)
              AND (@CourseId IS NULL OR e.E_CourseId = @CourseId)
              AND (@BatchId IS NULL OR e.E_BatchId = @BatchId)
              AND (@Status IS NULL OR e.E_Status = @Status);

            SELECT e.*, s.S_FirstName + ' ' + ISNULL(s.S_MiddleName + ' ', '') + s.S_LastName AS StudentName,
                   c.C_Name AS CourseName, bt.BT_Name AS BatchName, ay.AY_Name AS AcademicYearName
            FROM Enrollments_E e
            LEFT JOIN Students_S s ON e.E_StudentId = s.S_Id
            LEFT JOIN Courses_C c ON e.E_CourseId = c.C_Id
            LEFT JOIN Batches_BT bt ON e.E_BatchId = bt.BT_Id
            LEFT JOIN AcademicYears_AY ay ON e.E_AcademicYearId = ay.AY_Id
            WHERE e.E_TenantId = @E_TenantId
              AND (@SearchTerm IS NULL OR e.E_EnrollmentNumber LIKE '%' + @SearchTerm + '%')
              AND (@AcademicYearId IS NULL OR e.E_AcademicYearId = @AcademicYearId)
              AND (@CourseId IS NULL OR e.E_CourseId = @CourseId)
              AND (@BatchId IS NULL OR e.E_BatchId = @BatchId)
              AND (@Status IS NULL OR e.E_Status = @Status)
            ORDER BY e.E_EnrollmentDate DESC
            OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
        END

        ELSE IF @Action = 'IsDuplicate'
        BEGIN
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM Enrollments_E WHERE E_TenantId = @E_TenantId AND E_StudentId = @E_StudentId AND E_BatchId = @E_BatchId
                AND (@ExcludeId IS NULL OR E_Id <> @ExcludeId)
            ) THEN 1 ELSE 0 END;
        END

        ELSE IF @Action = 'Insert'
        BEGIN
            IF @E_EnrollmentNumber IS NULL OR @E_EnrollmentNumber = ''
            BEGIN
                DECLARE @YearPart VARCHAR(4) = FORMAT(GETUTCDATE(), 'yy');
                DECLARE @Seq INT = ISNULL((SELECT MAX(CAST(RIGHT(E_EnrollmentNumber, 4) AS INT)) FROM Enrollments_E WHERE E_TenantId = @E_TenantId AND E_EnrollmentNumber LIKE 'ENR-' + @YearPart + '-%'), 0) + 1;
                SET @E_EnrollmentNumber = 'ENR-' + @YearPart + '-' + FORMAT(@Seq, '0000');
            END

            INSERT INTO Enrollments_E (E_Id, E_TenantId, E_StudentId, E_AcademicYearId, E_CourseId, E_BatchId, E_EnrollmentNumber, E_EnrollmentDate, E_Status, E_CompletionDate, E_CreatedAt, E_UpdatedAt)
            VALUES (@E_Id, @E_TenantId, @E_StudentId, @E_AcademicYearId, @E_CourseId, @E_BatchId, @E_EnrollmentNumber, @E_EnrollmentDate, @E_Status, @E_CompletionDate, GETUTCDATE(), GETUTCDATE());
            SELECT @E_Id;
        END

        ELSE IF @Action = 'Update'
        BEGIN
            UPDATE Enrollments_E SET
                E_StudentId = @E_StudentId, E_AcademicYearId = @E_AcademicYearId, E_CourseId = @E_CourseId,
                E_BatchId = @E_BatchId, E_EnrollmentNumber = ISNULL(@E_EnrollmentNumber, E_EnrollmentNumber),
                E_EnrollmentDate = @E_EnrollmentDate, E_Status = @E_Status, E_CompletionDate = @E_CompletionDate,
                E_UpdatedAt = GETUTCDATE()
            WHERE E_Id = @E_Id AND E_TenantId = @E_TenantId;
            SELECT @@ROWCOUNT;
        END

        ELSE IF @Action = 'Delete'
        BEGIN
            DELETE FROM Enrollments_E WHERE E_Id = @E_Id AND E_TenantId = @E_TenantId;
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

-- ============================================================================
-- 4. USP_Timetables_TT
-- ============================================================================
CREATE PROCEDURE [dbo].[USP_Timetables_TT]
    @Action NVARCHAR(20),
    @TT_Id UNIQUEIDENTIFIER = NULL,
    @TT_TenantId UNIQUEIDENTIFIER = NULL,
    @TT_BranchId UNIQUEIDENTIFIER = NULL,
    @TT_BatchId UNIQUEIDENTIFIER = NULL,
    @TT_SubjectId UNIQUEIDENTIFIER = NULL,
    @TT_StaffId UNIQUEIDENTIFIER = NULL,
    @TT_ClassroomId UNIQUEIDENTIFIER = NULL,
    @TT_DayOfWeek SMALLINT = NULL,
    @TT_StartTime TIME = NULL,
    @TT_EndTime TIME = NULL,
    @TT_EffectiveFrom DATE = NULL,
    @TT_EffectiveTo DATE = NULL,
    @BatchId UNIQUEIDENTIFIER = NULL,
    @BranchId UNIQUEIDENTIFIER = NULL,
    @ExcludeId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        IF @Action = 'GetAll'
        BEGIN
            SELECT tt.*, s.SB_Name AS SubjectName,
                   st.ST_FirstName + ' ' + st.ST_LastName AS StaffName,
                   cr.CR_Name AS ClassroomName,
                   bt.BT_Name AS BatchName
            FROM Timetables_TT tt
            LEFT JOIN Subjects_SB s ON tt.TT_SubjectId = s.SB_Id
            LEFT JOIN Staff_ST st ON tt.TT_StaffId = st.ST_Id
            LEFT JOIN Classrooms_CR cr ON tt.TT_ClassroomId = cr.CR_Id
            LEFT JOIN Batches_BT bt ON tt.TT_BatchId = bt.BT_Id
            WHERE tt.TT_TenantId = @TT_TenantId
              AND (@BatchId IS NULL OR tt.TT_BatchId = @BatchId)
              AND (@BranchId IS NULL OR tt.TT_BranchId = @BranchId)
            ORDER BY tt.TT_DayOfWeek, tt.TT_StartTime;
        END

        ELSE IF @Action = 'GetById'
        BEGIN
            SELECT tt.*, s.SB_Name AS SubjectName,
                   st.ST_FirstName + ' ' + st.ST_LastName AS StaffName,
                   cr.CR_Name AS ClassroomName,
                   bt.BT_Name AS BatchName
            FROM Timetables_TT tt
            LEFT JOIN Subjects_SB s ON tt.TT_SubjectId = s.SB_Id
            LEFT JOIN Staff_ST st ON tt.TT_StaffId = st.ST_Id
            LEFT JOIN Classrooms_CR cr ON tt.TT_ClassroomId = cr.CR_Id
            LEFT JOIN Batches_BT bt ON tt.TT_BatchId = bt.BT_Id
            WHERE tt.TT_Id = @TT_Id AND tt.TT_TenantId = @TT_TenantId;
        END

        ELSE IF @Action = 'CheckConflict'
        BEGIN
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM Timetables_TT
                WHERE TT_TenantId = @TT_TenantId AND TT_BatchId = @TT_BatchId AND TT_DayOfWeek = @TT_DayOfWeek
                AND TT_StartTime < @TT_EndTime AND TT_EndTime > @TT_StartTime
                AND (@ExcludeId IS NULL OR TT_Id <> @ExcludeId)
            ) THEN 1 ELSE 0 END;
        END

        ELSE IF @Action = 'Insert'
        BEGIN
            INSERT INTO Timetables_TT (TT_Id, TT_TenantId, TT_BranchId, TT_BatchId, TT_SubjectId, TT_StaffId, TT_ClassroomId, TT_DayOfWeek, TT_StartTime, TT_EndTime, TT_EffectiveFrom, TT_EffectiveTo, TT_CreatedAt, TT_UpdatedAt)
            VALUES (@TT_Id, @TT_TenantId, @TT_BranchId, @TT_BatchId, @TT_SubjectId, @TT_StaffId, @TT_ClassroomId, @TT_DayOfWeek, @TT_StartTime, @TT_EndTime, @TT_EffectiveFrom, @TT_EffectiveTo, GETUTCDATE(), GETUTCDATE());
            SELECT @TT_Id;
        END

        ELSE IF @Action = 'Update'
        BEGIN
            UPDATE Timetables_TT SET
                TT_BranchId = @TT_BranchId, TT_BatchId = @TT_BatchId, TT_SubjectId = @TT_SubjectId,
                TT_StaffId = @TT_StaffId, TT_ClassroomId = @TT_ClassroomId, TT_DayOfWeek = @TT_DayOfWeek,
                TT_StartTime = @TT_StartTime, TT_EndTime = @TT_EndTime,
                TT_EffectiveFrom = @TT_EffectiveFrom, TT_EffectiveTo = @TT_EffectiveTo, TT_UpdatedAt = GETUTCDATE()
            WHERE TT_Id = @TT_Id AND TT_TenantId = @TT_TenantId;
            SELECT @@ROWCOUNT;
        END

        ELSE IF @Action = 'Delete'
        BEGIN
            DELETE FROM Timetables_TT WHERE TT_Id = @TT_Id AND TT_TenantId = @TT_TenantId;
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

-- ============================================================================
-- 5. USP_AdmissionApplications_AA
-- ============================================================================
CREATE PROCEDURE [dbo].[USP_AdmissionApplications_AA]
    @Action NVARCHAR(20),
    @AA_Id UNIQUEIDENTIFIER = NULL,
    @AA_TenantId UNIQUEIDENTIFIER = NULL,
    @AA_BranchId UNIQUEIDENTIFIER = NULL,
    @AA_ApplicationNumber NVARCHAR(50) = NULL,
    @AA_FirstName NVARCHAR(100) = NULL,
    @AA_LastName NVARCHAR(100) = NULL,
    @AA_DateOfBirth DATE = NULL,
    @AA_Gender NVARCHAR(20) = NULL,
    @AA_Email NVARCHAR(255) = NULL,
    @AA_Phone NVARCHAR(30) = NULL,
    @AA_CourseId UNIQUEIDENTIFIER = NULL,
    @AA_AcademicYearId UNIQUEIDENTIFIER = NULL,
    @AA_Status NVARCHAR(20) = NULL,
    @AA_SubmittedAt DATETIME2 = NULL,
    @AA_Notes NVARCHAR(MAX) = NULL,
    @AA_ReviewedBy UNIQUEIDENTIFIER = NULL,
    @SearchTerm NVARCHAR(255) = NULL,
    @BranchId UNIQUEIDENTIFIER = NULL,
    @CourseId UNIQUEIDENTIFIER = NULL,
    @AcademicYearId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(20) = NULL,
    @ExcludeId UNIQUEIDENTIFIER = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        IF @Action = 'GetById'
        BEGIN
            SELECT a.*, c.C_Name AS CourseName, ay.AY_Name AS AcademicYearName
            FROM AdmissionApplications_AA a
            LEFT JOIN Courses_C c ON a.AA_CourseId = c.C_Id
            LEFT JOIN AcademicYears_AY ay ON a.AA_AcademicYearId = ay.AY_Id
            WHERE a.AA_Id = @AA_Id AND a.AA_TenantId = @AA_TenantId;
        END

        ELSE IF @Action = 'GetPaged'
        BEGIN
            SELECT COUNT(*) AS TotalCount
            FROM AdmissionApplications_AA a
            WHERE a.AA_TenantId = @AA_TenantId
              AND (@SearchTerm IS NULL OR a.AA_ApplicationNumber LIKE '%' + @SearchTerm + '%'
                   OR a.AA_FirstName LIKE '%' + @SearchTerm + '%' OR a.AA_LastName LIKE '%' + @SearchTerm + '%')
              AND (@BranchId IS NULL OR a.AA_BranchId = @BranchId)
              AND (@CourseId IS NULL OR a.AA_CourseId = @CourseId)
              AND (@AcademicYearId IS NULL OR a.AA_AcademicYearId = @AcademicYearId)
              AND (@Status IS NULL OR a.AA_Status = @Status);

            SELECT a.*, c.C_Name AS CourseName, ay.AY_Name AS AcademicYearName
            FROM AdmissionApplications_AA a
            LEFT JOIN Courses_C c ON a.AA_CourseId = c.C_Id
            LEFT JOIN AcademicYears_AY ay ON a.AA_AcademicYearId = ay.AY_Id
            WHERE a.AA_TenantId = @AA_TenantId
              AND (@SearchTerm IS NULL OR a.AA_ApplicationNumber LIKE '%' + @SearchTerm + '%'
                   OR a.AA_FirstName LIKE '%' + @SearchTerm + '%' OR a.AA_LastName LIKE '%' + @SearchTerm + '%')
              AND (@BranchId IS NULL OR a.AA_BranchId = @BranchId)
              AND (@CourseId IS NULL OR a.AA_CourseId = @CourseId)
              AND (@AcademicYearId IS NULL OR a.AA_AcademicYearId = @AcademicYearId)
              AND (@Status IS NULL OR a.AA_Status = @Status)
            ORDER BY a.AA_CreatedAt DESC
            OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
        END

        ELSE IF @Action = 'ExistsByNumber'
        BEGIN
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM AdmissionApplications_AA WHERE AA_TenantId = @AA_TenantId AND AA_ApplicationNumber = @AA_ApplicationNumber
                AND (@ExcludeId IS NULL OR AA_Id <> @ExcludeId)
            ) THEN 1 ELSE 0 END;
        END

        ELSE IF @Action = 'Insert'
        BEGIN
            IF @AA_ApplicationNumber IS NULL OR @AA_ApplicationNumber = ''
            BEGIN
                DECLARE @YearPart VARCHAR(4) = FORMAT(GETUTCDATE(), 'yy');
                DECLARE @Seq INT = ISNULL((SELECT MAX(CAST(RIGHT(AA_ApplicationNumber, 4) AS INT)) FROM AdmissionApplications_AA WHERE AA_TenantId = @AA_TenantId AND AA_ApplicationNumber LIKE 'APP-' + @YearPart + '-%'), 0) + 1;
                SET @AA_ApplicationNumber = 'APP-' + @YearPart + '-' + FORMAT(@Seq, '0000');
            END

            INSERT INTO AdmissionApplications_AA (AA_Id, AA_TenantId, AA_BranchId, AA_ApplicationNumber, AA_FirstName, AA_LastName, AA_DateOfBirth, AA_Gender, AA_Email, AA_Phone, AA_CourseId, AA_AcademicYearId, AA_Status, AA_SubmittedAt, AA_Notes, AA_CreatedAt, AA_UpdatedAt)
            VALUES (@AA_Id, @AA_TenantId, @AA_BranchId, @AA_ApplicationNumber, @AA_FirstName, @AA_LastName, @AA_DateOfBirth, @AA_Gender, @AA_Email, @AA_Phone, @AA_CourseId, @AA_AcademicYearId, @AA_Status, @AA_SubmittedAt, @AA_Notes, GETUTCDATE(), GETUTCDATE());
            SELECT @AA_Id;
        END

        ELSE IF @Action = 'Update'
        BEGIN
            UPDATE AdmissionApplications_AA SET
                AA_BranchId = @AA_BranchId, AA_ApplicationNumber = ISNULL(@AA_ApplicationNumber, AA_ApplicationNumber),
                AA_FirstName = @AA_FirstName, AA_LastName = @AA_LastName, AA_DateOfBirth = @AA_DateOfBirth,
                AA_Gender = @AA_Gender, AA_Email = @AA_Email, AA_Phone = @AA_Phone,
                AA_CourseId = @AA_CourseId, AA_AcademicYearId = @AA_AcademicYearId,
                AA_Status = @AA_Status, AA_Notes = @AA_Notes, AA_UpdatedAt = GETUTCDATE()
            WHERE AA_Id = @AA_Id AND AA_TenantId = @AA_TenantId;
            SELECT @@ROWCOUNT;
        END

        ELSE IF @Action = 'Delete'
        BEGIN
            DELETE FROM AdmissionApplications_AA WHERE AA_Id = @AA_Id AND AA_TenantId = @AA_TenantId;
            SELECT @@ROWCOUNT;
        END

        ELSE IF @Action = 'Review'
        BEGIN
            UPDATE AdmissionApplications_AA SET
                AA_Status = @AA_Status,
                AA_Notes = ISNULL(@AA_Notes, AA_Notes),
                AA_ReviewedAt = GETUTCDATE(),
                AA_ReviewedBy = @AA_ReviewedBy,
                AA_UpdatedAt = GETUTCDATE()
            WHERE AA_Id = @AA_Id AND AA_TenantId = @AA_TenantId;
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
