USE [IMS]
GO

-- =========================================================================================
-- Drop ALL Foreign Key constraints that reference Organizations_O (O_Id)
-- Covers every _TenantId FK across the entire database, not just known tables
-- Columns stay NOT NULL, only the FK validation is removed
-- =========================================================================================

DECLARE @sql NVARCHAR(MAX) = '';

SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(fk.parent_object_id)) 
    + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) 
    + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc 
    ON fk.object_id = fkc.constraint_object_id
INNER JOIN sys.columns c 
    ON fkc.parent_object_id = c.object_id AND fkc.parent_column_id = c.column_id
INNER JOIN sys.tables t 
    ON fk.referenced_object_id = t.object_id
WHERE t.name = 'Organizations_O'
  AND c.name LIKE '%TenantId';

IF @sql <> ''
BEGIN
    EXEC sp_executesql @sql;
    PRINT 'Dropped all TenantId FK constraints:';
    PRINT @sql;
END
ELSE
BEGIN
    PRINT 'No TenantId FK constraints found referencing Organizations_O.';
END
GO
