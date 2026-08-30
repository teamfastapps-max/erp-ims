-- ============================================================
-- USP_Expenses_EXP
-- ============================================================
ALTER PROCEDURE [dbo].[USP_Expenses_EXP]
    @Action NVARCHAR(20),
    @EXP_Id UNIQUEIDENTIFIER = NULL,
    @EXP_TenantId UNIQUEIDENTIFIER = NULL,
    @EXP_BranchId UNIQUEIDENTIFIER = NULL,
    @EXP_ExpenseCategoryId UNIQUEIDENTIFIER = NULL,
    @EXP_VendorId UNIQUEIDENTIFIER = NULL,
    @EXP_ExpenseNumber NVARCHAR(50) = NULL,
    @EXP_ExpenseDate DATE = NULL,
    @EXP_Amount DECIMAL(12,2) = NULL,
    @EXP_Description NVARCHAR(MAX) = NULL,
    @EXP_PaymentMethodId UNIQUEIDENTIFIER = NULL,
    @EXP_CreatedBy UNIQUEIDENTIFIER = NULL,
    @SearchTerm NVARCHAR(255) = NULL,
    @BranchId UNIQUEIDENTIFIER = NULL,
    @ExpenseCategoryId UNIQUEIDENTIFIER = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        IF @Action = 'GetById'
        BEGIN
            SELECT e.*, 
                   b.B_Name AS BranchName,
                   ec.EC_Name AS ExpenseCategoryName,
                   v.V_Name AS VendorName,
                   pm.PM_Name AS PaymentMethodName
            FROM Expenses_EXP e
            LEFT JOIN Branches_B b ON e.EXP_BranchId = b.B_Id
            LEFT JOIN ExpenseCategories_EC ec ON e.EXP_ExpenseCategoryId = ec.EC_Id
            LEFT JOIN Vendors_V v ON e.EXP_VendorId = v.V_Id
            LEFT JOIN PaymentMethods_PM pm ON e.EXP_PaymentMethodId = pm.PM_Id
            WHERE e.EXP_Id = @EXP_Id AND e.EXP_TenantId = @EXP_TenantId;
        END

        IF @Action = 'GetPaged'
        BEGIN
            SELECT COUNT(*) AS TotalCount
            FROM Expenses_EXP e
            WHERE e.EXP_TenantId = @EXP_TenantId
              AND (@SearchTerm IS NULL OR e.EXP_ExpenseNumber LIKE '%' + @SearchTerm + '%' OR e.EXP_Description LIKE '%' + @SearchTerm + '%')
              AND (@BranchId IS NULL OR e.EXP_BranchId = @BranchId)
              AND (@ExpenseCategoryId IS NULL OR e.EXP_ExpenseCategoryId = @ExpenseCategoryId);

            SELECT e.*, 
                   b.B_Name AS BranchName,
                   ec.EC_Name AS ExpenseCategoryName,
                   v.V_Name AS VendorName,
                   pm.PM_Name AS PaymentMethodName
            FROM Expenses_EXP e
            LEFT JOIN Branches_B b ON e.EXP_BranchId = b.B_Id
            LEFT JOIN ExpenseCategories_EC ec ON e.EXP_ExpenseCategoryId = ec.EC_Id
            LEFT JOIN Vendors_V v ON e.EXP_VendorId = v.V_Id
            LEFT JOIN PaymentMethods_PM pm ON e.EXP_PaymentMethodId = pm.PM_Id
            WHERE e.EXP_TenantId = @EXP_TenantId
              AND (@SearchTerm IS NULL OR e.EXP_ExpenseNumber LIKE '%' + @SearchTerm + '%' OR e.EXP_Description LIKE '%' + @SearchTerm + '%')
              AND (@BranchId IS NULL OR e.EXP_BranchId = @BranchId)
              AND (@ExpenseCategoryId IS NULL OR e.EXP_ExpenseCategoryId = @ExpenseCategoryId)
            ORDER BY e.EXP_ExpenseDate DESC
            OFFSET (@PageNumber - 1) * @PageSize ROWS
            FETCH NEXT @PageSize ROWS ONLY;
        END

        IF @Action = 'GetNextNumber'
        BEGIN
            DECLARE @MaxNum INT;
            SELECT @MaxNum = ISNULL(MAX(CAST(SUBSTRING(e.EXP_ExpenseNumber, 5, LEN(e.EXP_ExpenseNumber) - 4) AS INT)), 0)
            FROM Expenses_EXP e
            WHERE e.EXP_TenantId = @EXP_TenantId;

            SELECT 'EXP-' + RIGHT('0000' + CAST(@MaxNum + 1 AS NVARCHAR), 4);
        END

        IF @Action = 'Insert'
        BEGIN
            INSERT INTO Expenses_EXP (EXP_Id, EXP_TenantId, EXP_BranchId, EXP_ExpenseCategoryId, EXP_VendorId, EXP_ExpenseNumber, EXP_ExpenseDate, EXP_Amount, EXP_Description, EXP_PaymentMethodId, EXP_CreatedBy, EXP_CreatedAt, EXP_UpdatedAt)
            VALUES (@EXP_Id, @EXP_TenantId, @EXP_BranchId, @EXP_ExpenseCategoryId, @EXP_VendorId, @EXP_ExpenseNumber, @EXP_ExpenseDate, @EXP_Amount, @EXP_Description, @EXP_PaymentMethodId, @EXP_CreatedBy, SYSUTCDATETIME(), SYSUTCDATETIME());

            SELECT @EXP_Id;
        END

        IF @Action = 'Update'
        BEGIN
            UPDATE Expenses_EXP
            SET EXP_BranchId = @EXP_BranchId,
                EXP_ExpenseCategoryId = @EXP_ExpenseCategoryId,
                EXP_VendorId = @EXP_VendorId,
                EXP_ExpenseDate = @EXP_ExpenseDate,
                EXP_Amount = @EXP_Amount,
                EXP_Description = @EXP_Description,
                EXP_PaymentMethodId = @EXP_PaymentMethodId,
                EXP_UpdatedAt = SYSUTCDATETIME()
            WHERE EXP_Id = @EXP_Id AND EXP_TenantId = @EXP_TenantId;

            SELECT @@ROWCOUNT;
        END

        IF @Action = 'Delete'
        BEGIN
            DELETE FROM Expenses_EXP WHERE EXP_Id = @EXP_Id AND EXP_TenantId = @EXP_TenantId;
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
-- USP_ExpenseCategories_EC
-- ============================================================
ALTER PROCEDURE [dbo].[USP_ExpenseCategories_EC]
    @Action NVARCHAR(20),
    @EC_Id UNIQUEIDENTIFIER = NULL,
    @EC_TenantId UNIQUEIDENTIFIER = NULL,
    @EC_Name NVARCHAR(100) = NULL,
    @EC_Code NVARCHAR(50) = NULL,
    @EC_Description NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        IF @Action = 'GetAll'
        BEGIN
            SELECT * FROM ExpenseCategories_EC
            ORDER BY EC_Name;
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
