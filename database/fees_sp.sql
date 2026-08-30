-- ============================================================
-- USP_FeeStructures_FS
-- ============================================================
ALTER PROCEDURE [dbo].[USP_FeeStructures_FS]
    @Action NVARCHAR(20),
    @FS_Id UNIQUEIDENTIFIER = NULL,
    @FS_TenantId UNIQUEIDENTIFIER = NULL,
    @FS_Name NVARCHAR(150) = NULL,
    @FS_Code NVARCHAR(50) = NULL,
    @FS_CourseId UNIQUEIDENTIFIER = NULL,
    @FS_BatchId UNIQUEIDENTIFIER = NULL,
    @FS_AcademicYearId UNIQUEIDENTIFIER = NULL,
    @FS_Description NVARCHAR(MAX) = NULL,
    @FS_IsActive BIT = NULL,
    @ExcludeId UNIQUEIDENTIFIER = NULL,
    @SearchTerm NVARCHAR(255) = NULL,
    @AcademicYearId UNIQUEIDENTIFIER = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        IF @Action = 'GetById'
        BEGIN
            SELECT fs.*, c.C_Name AS CourseName, bt.BT_Name AS BatchName, ay.AY_Name AS AcademicYearName
            FROM FeeStructures_FS fs
            LEFT JOIN Courses_C c ON fs.FS_CourseId = c.C_Id
            LEFT JOIN Batches_BT bt ON fs.FS_BatchId = bt.BT_Id
            LEFT JOIN AcademicYears_AY ay ON fs.FS_AcademicYearId = ay.AY_Id
            WHERE fs.FS_Id = @FS_Id AND fs.FS_TenantId = @FS_TenantId;
        END

        IF @Action = 'GetPaged'
        BEGIN
            SELECT COUNT(*) AS TotalCount
            FROM FeeStructures_FS fs
            WHERE fs.FS_TenantId = @FS_TenantId
              AND (@SearchTerm IS NULL OR fs.FS_Name LIKE '%' + @SearchTerm + '%' OR fs.FS_Code LIKE '%' + @SearchTerm + '%')
              AND (@AcademicYearId IS NULL OR fs.FS_AcademicYearId = @AcademicYearId);

            SELECT fs.*, c.C_Name AS CourseName, bt.BT_Name AS BatchName, ay.AY_Name AS AcademicYearName
            FROM FeeStructures_FS fs
            LEFT JOIN Courses_C c ON fs.FS_CourseId = c.C_Id
            LEFT JOIN Batches_BT bt ON fs.FS_BatchId = bt.BT_Id
            LEFT JOIN AcademicYears_AY ay ON fs.FS_AcademicYearId = ay.AY_Id
            WHERE fs.FS_TenantId = @FS_TenantId
              AND (@SearchTerm IS NULL OR fs.FS_Name LIKE '%' + @SearchTerm + '%' OR fs.FS_Code LIKE '%' + @SearchTerm + '%')
              AND (@AcademicYearId IS NULL OR fs.FS_AcademicYearId = @AcademicYearId)
            ORDER BY fs.FS_Name
            OFFSET (@PageNumber - 1) * @PageSize ROWS
            FETCH NEXT @PageSize ROWS ONLY;
        END

        IF @Action = 'ExistsByCode'
        BEGIN
            SELECT COUNT(*) FROM FeeStructures_FS
            WHERE FS_TenantId = @FS_TenantId AND FS_Code = @FS_Code
              AND (@ExcludeId IS NULL OR FS_Id <> @ExcludeId);
        END

        IF @Action = 'Insert'
        BEGIN
            INSERT INTO FeeStructures_FS (FS_Id, FS_TenantId, FS_Name, FS_Code, FS_CourseId, FS_BatchId, FS_AcademicYearId, FS_Description, FS_IsActive, FS_CreatedAt, FS_UpdatedAt)
            VALUES (@FS_Id, @FS_TenantId, @FS_Name, @FS_Code, @FS_CourseId, @FS_BatchId, @FS_AcademicYearId, @FS_Description, @FS_IsActive, SYSUTCDATETIME(), SYSUTCDATETIME());
            SELECT @FS_Id;
        END

        IF @Action = 'Update'
        BEGIN
            UPDATE FeeStructures_FS
            SET FS_Name = @FS_Name, FS_Code = @FS_Code, FS_CourseId = @FS_CourseId, FS_BatchId = @FS_BatchId,
                FS_AcademicYearId = @FS_AcademicYearId, FS_Description = @FS_Description, FS_IsActive = @FS_IsActive,
                FS_UpdatedAt = SYSUTCDATETIME()
            WHERE FS_Id = @FS_Id AND FS_TenantId = @FS_TenantId;
            SELECT @@ROWCOUNT;
        END

        IF @Action = 'Delete'
        BEGIN
            DELETE FROM FeeStructures_FS WHERE FS_Id = @FS_Id AND FS_TenantId = @FS_TenantId;
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
-- USP_FeeStructureItems_FSI
-- ============================================================
ALTER PROCEDURE [dbo].[USP_FeeStructureItems_FSI]
    @Action NVARCHAR(20),
    @FSI_Id UNIQUEIDENTIFIER = NULL,
    @FSI_FeeStructureId UNIQUEIDENTIFIER = NULL,
    @FSI_FeeCategoryId UNIQUEIDENTIFIER = NULL,
    @FSI_Amount DECIMAL(12,2) = NULL,
    @FSI_DueDays INT = NULL,
    @FSI_IsMandatory BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        IF @Action = 'GetByFeeStructure'
        BEGIN
            SELECT fsi.*, fc.FC_Name AS FeeCategoryName
            FROM FeeStructureItems_FSI fsi
            LEFT JOIN FeeCategories_FC fc ON fsi.FSI_FeeCategoryId = fc.FC_Id
            WHERE fsi.FSI_FeeStructureId = @FSI_FeeStructureId;
        END

        IF @Action = 'Insert'
        BEGIN
            INSERT INTO FeeStructureItems_FSI (FSI_Id, FSI_FeeStructureId, FSI_FeeCategoryId, FSI_Amount, FSI_DueDays, FSI_IsMandatory)
            VALUES (@FSI_Id, @FSI_FeeStructureId, @FSI_FeeCategoryId, @FSI_Amount, @FSI_DueDays, @FSI_IsMandatory);
        END

        IF @Action = 'DeleteByFeeStructure'
        BEGIN
            DELETE FROM FeeStructureItems_FSI WHERE FSI_FeeStructureId = @FSI_FeeStructureId;
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
-- USP_FeeInvoices_FI
-- ============================================================
ALTER PROCEDURE [dbo].[USP_FeeInvoices_FI]
    @Action NVARCHAR(20),
    @FI_Id UNIQUEIDENTIFIER = NULL,
    @FI_TenantId UNIQUEIDENTIFIER = NULL,
    @FI_StudentId UNIQUEIDENTIFIER = NULL,
    @FI_InvoiceNumber NVARCHAR(50) = NULL,
    @FI_InvoiceDate DATE = NULL,
    @FI_DueDate DATE = NULL,
    @FI_Subtotal DECIMAL(12,2) = NULL,
    @FI_DiscountAmount DECIMAL(12,2) = NULL,
    @FI_TaxAmount DECIMAL(12,2) = NULL,
    @FI_TotalAmount DECIMAL(12,2) = NULL,
    @FI_PaidAmount DECIMAL(12,2) = NULL,
    @FI_BalanceAmount DECIMAL(12,2) = NULL,
    @FI_Status NVARCHAR(20) = NULL,
    @FI_Notes NVARCHAR(MAX) = NULL,
    @SearchTerm NVARCHAR(255) = NULL,
    @Status NVARCHAR(20) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        IF @Action = 'GetById'
        BEGIN
            SELECT fi.*, s.S_FirstName + ' ' + s.S_LastName AS StudentName, s.S_StudentCode AS StudentCode
            FROM FeeInvoices_FI fi
            LEFT JOIN Students_S s ON fi.FI_StudentId = s.S_Id
            WHERE fi.FI_Id = @FI_Id AND fi.FI_TenantId = @FI_TenantId;
        END

        IF @Action = 'GetPaged'
        BEGIN
            SELECT COUNT(*) AS TotalCount
            FROM FeeInvoices_FI fi
            WHERE fi.FI_TenantId = @FI_TenantId
              AND (@SearchTerm IS NULL OR fi.FI_InvoiceNumber LIKE '%' + @SearchTerm + '%')
              AND (@Status IS NULL OR fi.FI_Status = @Status);

            SELECT fi.*, s.S_FirstName + ' ' + s.S_LastName AS StudentName, s.S_StudentCode AS StudentCode
            FROM FeeInvoices_FI fi
            LEFT JOIN Students_S s ON fi.FI_StudentId = s.S_Id
            WHERE fi.FI_TenantId = @FI_TenantId
              AND (@SearchTerm IS NULL OR fi.FI_InvoiceNumber LIKE '%' + @SearchTerm + '%')
              AND (@Status IS NULL OR fi.FI_Status = @Status)
            ORDER BY fi.FI_InvoiceDate DESC
            OFFSET (@PageNumber - 1) * @PageSize ROWS
            FETCH NEXT @PageSize ROWS ONLY;
        END

        IF @Action = 'GetNextNumber'
        BEGIN
            DECLARE @MaxNum INT;
            SELECT @MaxNum = ISNULL(MAX(CAST(SUBSTRING(FI_InvoiceNumber, 5, LEN(FI_InvoiceNumber) - 4) AS INT)), 0)
            FROM FeeInvoices_FI WHERE FI_TenantId = @FI_TenantId;
            SELECT 'INV-' + RIGHT('0000' + CAST(@MaxNum + 1 AS NVARCHAR), 4);
        END

        IF @Action = 'Insert'
        BEGIN
            INSERT INTO FeeInvoices_FI (FI_Id, FI_TenantId, FI_StudentId, FI_InvoiceNumber, FI_InvoiceDate, FI_DueDate, FI_Subtotal, FI_DiscountAmount, FI_TaxAmount, FI_TotalAmount, FI_PaidAmount, FI_BalanceAmount, FI_Status, FI_Notes, FI_CreatedAt, FI_UpdatedAt)
            VALUES (@FI_Id, @FI_TenantId, @FI_StudentId, @FI_InvoiceNumber, @FI_InvoiceDate, @FI_DueDate, @FI_Subtotal, @FI_DiscountAmount, @FI_TaxAmount, @FI_TotalAmount, @FI_PaidAmount, @FI_BalanceAmount, @FI_Status, @FI_Notes, SYSUTCDATETIME(), SYSUTCDATETIME());
            SELECT @FI_Id;
        END

        IF @Action = 'UpdatePaidAmount'
        BEGIN
            UPDATE FeeInvoices_FI
            SET FI_PaidAmount = @FI_PaidAmount,
                FI_BalanceAmount = FI_TotalAmount - @FI_PaidAmount,
                FI_Status = CASE
                    WHEN @FI_PaidAmount >= FI_TotalAmount THEN 'Paid'
                    WHEN @FI_PaidAmount > 0 THEN 'Partial'
                    ELSE FI_Status
                END,
                FI_UpdatedAt = SYSUTCDATETIME()
            WHERE FI_Id = @FI_Id;
            SELECT @@ROWCOUNT;
        END

        IF @Action = 'Delete'
        BEGIN
            DELETE FROM FeeInvoices_FI WHERE FI_Id = @FI_Id AND FI_TenantId = @FI_TenantId;
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
-- USP_Payments_PAY
-- ============================================================
ALTER PROCEDURE [dbo].[USP_Payments_PAY]
    @Action NVARCHAR(20),
    @PAY_Id UNIQUEIDENTIFIER = NULL,
    @PAY_TenantId UNIQUEIDENTIFIER = NULL,
    @PAY_StudentId UNIQUEIDENTIFIER = NULL,
    @PAY_PaymentNumber NVARCHAR(50) = NULL,
    @PAY_PaymentDate DATETIME2 = NULL,
    @PAY_Amount DECIMAL(12,2) = NULL,
    @PAY_PaymentMethodId UNIQUEIDENTIFIER = NULL,
    @PAY_Status NVARCHAR(20) = NULL,
    @PAY_TransactionReference NVARCHAR(150) = NULL,
    @PAY_Notes NVARCHAR(MAX) = NULL,
    @PAY_CreatedBy UNIQUEIDENTIFIER = NULL,
    @SearchTerm NVARCHAR(255) = NULL,
    @Status NVARCHAR(20) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        IF @Action = 'GetById'
        BEGIN
            SELECT p.*, s.S_FirstName + ' ' + s.S_LastName AS StudentName, s.S_StudentCode AS StudentCode, pm.PM_Name AS PaymentMethodName
            FROM Payments_PAY p
            LEFT JOIN Students_S s ON p.PAY_StudentId = s.S_Id
            LEFT JOIN PaymentMethods_PM pm ON p.PAY_PaymentMethodId = pm.PM_Id
            WHERE p.PAY_Id = @PAY_Id AND p.PAY_TenantId = @PAY_TenantId;
        END

        IF @Action = 'GetPaged'
        BEGIN
            SELECT COUNT(*) AS TotalCount
            FROM Payments_PAY p
            WHERE p.PAY_TenantId = @PAY_TenantId
              AND (@SearchTerm IS NULL OR p.PAY_PaymentNumber LIKE '%' + @SearchTerm + '%')
              AND (@Status IS NULL OR p.PAY_Status = @Status);

            SELECT p.*, s.S_FirstName + ' ' + s.S_LastName AS StudentName, s.S_StudentCode AS StudentCode, pm.PM_Name AS PaymentMethodName
            FROM Payments_PAY p
            LEFT JOIN Students_S s ON p.PAY_StudentId = s.S_Id
            LEFT JOIN PaymentMethods_PM pm ON p.PAY_PaymentMethodId = pm.PM_Id
            WHERE p.PAY_TenantId = @PAY_TenantId
              AND (@SearchTerm IS NULL OR p.PAY_PaymentNumber LIKE '%' + @SearchTerm + '%')
              AND (@Status IS NULL OR p.PAY_Status = @Status)
            ORDER BY p.PAY_PaymentDate DESC
            OFFSET (@PageNumber - 1) * @PageSize ROWS
            FETCH NEXT @PageSize ROWS ONLY;
        END

        IF @Action = 'GetNextNumber'
        BEGIN
            DECLARE @MaxNum INT;
            SELECT @MaxNum = ISNULL(MAX(CAST(SUBSTRING(PAY_PaymentNumber, 5, LEN(PAY_PaymentNumber) - 4) AS INT)), 0)
            FROM Payments_PAY WHERE PAY_TenantId = @PAY_TenantId;
            SELECT 'PAY-' + RIGHT('0000' + CAST(@MaxNum + 1 AS NVARCHAR), 4);
        END

        IF @Action = 'Insert'
        BEGIN
            INSERT INTO Payments_PAY (PAY_Id, PAY_TenantId, PAY_StudentId, PAY_PaymentNumber, PAY_PaymentDate, PAY_Amount, PAY_PaymentMethodId, PAY_Status, PAY_TransactionReference, PAY_Notes, PAY_CreatedBy, PAY_CreatedAt, PAY_UpdatedAt)
            VALUES (@PAY_Id, @PAY_TenantId, @PAY_StudentId, @PAY_PaymentNumber, @PAY_PaymentDate, @PAY_Amount, @PAY_PaymentMethodId, @PAY_Status, @PAY_TransactionReference, @PAY_Notes, @PAY_CreatedBy, SYSUTCDATETIME(), SYSUTCDATETIME());
            SELECT @PAY_Id;
        END

        IF @Action = 'Delete'
        BEGIN
            DELETE FROM Payments_PAY WHERE PAY_Id = @PAY_Id AND PAY_TenantId = @PAY_TenantId;
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
-- USP_FeeCategories_FC (fix GetAll — no tenant filter for MasterDAL)
-- ============================================================
ALTER PROCEDURE [dbo].[USP_FeeCategories_FC]
    @Action NVARCHAR(20),
    @FC_Id UNIQUEIDENTIFIER = NULL,
    @FC_TenantId UNIQUEIDENTIFIER = NULL,
    @FC_Name NVARCHAR(100) = NULL,
    @FC_Code NVARCHAR(50) = NULL,
    @FC_Description NVARCHAR(MAX) = NULL,
    @FC_IsRefundable BIT = NULL,
    @ColumnName NVARCHAR(100) = NULL,
    @Value NVARCHAR(MAX) = NULL,
    @NewId UNIQUEIDENTIFIER = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        IF @Action = 'GetAll'
        BEGIN
            SELECT FC_Id, FC_TenantId, FC_Name, FC_Code, FC_Description, FC_IsRefundable
            FROM FeeCategories_FC
            ORDER BY FC_Name;
        END

        IF @Action = 'GetById'
        BEGIN
            SELECT * FROM FeeCategories_FC WHERE FC_Id = @FC_Id;
        END

        IF @Action = 'ExistsByField'
        BEGIN
            DECLARE @Sql NVARCHAR(MAX);
            SET @Sql = N'SELECT COUNT(1) FROM FeeCategories_FC WHERE ' + QUOTENAME(@ColumnName) + N' = @P_Val';
            EXEC sp_executesql @Sql, N'@P_Val NVARCHAR(MAX)', @Value;
        END

        IF @Action = 'Insert'
        BEGIN
            SET @NewId = ISNULL(@NewId, NEWID());
            INSERT INTO FeeCategories_FC (FC_Id, FC_TenantId, FC_Name, FC_Code, FC_Description, FC_IsRefundable, FC_CreatedAt, FC_UpdatedAt)
            VALUES (@NewId, @FC_TenantId, @FC_Name, @FC_Code, @FC_Description, ISNULL(@FC_IsRefundable, 0), SYSUTCDATETIME(), SYSUTCDATETIME());
            SELECT @NewId;
        END

        IF @Action = 'Update'
        BEGIN
            UPDATE FeeCategories_FC
            SET FC_Name = @FC_Name, FC_Code = @FC_Code, FC_Description = @FC_Description,
                FC_IsRefundable = ISNULL(@FC_IsRefundable, FC_IsRefundable), FC_UpdatedAt = SYSUTCDATETIME()
            WHERE FC_Id = @FC_Id;
            SELECT @@ROWCOUNT;
        END

        IF @Action = 'Delete'
        BEGIN
            RAISERROR('FeeCategories_FC does not support hard delete.', 16, 1);
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
