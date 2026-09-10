/*
    Rename the patent R8/R10 tag where it appears in NAMES:
        "PatR8-R10v2.1"  ->  "PatR8-10"
        "PatR8-10v2.1"   ->  "PatR8-10"

    Companion to rename-PatR8-10.sql. That script fixes the Systems / SystemType
    columns, which is what the app matches on. This one fixes the places the old
    name is only DISPLAYED - most visibly the document dropdowns on the Deploy
    detail page, whose text is tblDocDocument.DocName (prefixed with the release's
    SystemType when a path merges two release types).

    Both old spellings are handled, because both have been in circulation.
    "PatR10v2.2" and "TmkR9-10v2.2" are different systems and are NOT touched -
    neither contains either search string.

    HOW TO RUN
      1. Take a database backup first.
      2. Run as-is. @Execute = 0 only REPORTS, and lists the actual values it
         would rewrite so you can eyeball them.
      3. If the list looks right, set @Execute = 1 and run again.

    WHAT IT TOUCHES
      tblCPiSystem.SystemName    the systems catalog (Shared > System screen)
      tblRelease.Name            release name; also drives the document folder name
      tblDocDocument.DocName     ** the Deploy dropdown text **
      tblDocDocument.Remarks     free text that may mention the tag
      tblDocFile.UserFileName    the name a download is saved as - opt in with
                                 @IncludeFileNames, since it changes what users get

    WHAT IT WILL NOT TOUCH
      tblDocFile.DocFileName     the physical storage key for the stored file.
                                 Rewriting it would orphan the file on disk / in
                                 blob storage. Left alone deliberately.

    NOTE ON tblCPiSystem
      SystemName has a unique index (UQ_tblCPiSystem_SystemName). If a database
      somehow holds both "PatR8-10" and one of the old spellings, renaming would
      violate it, so the script reports that and refuses rather than failing
      halfway. Delete or merge the duplicate row by hand first.

    The local LawPortalCPiR10 had nothing to change when this was written: no
    column in the database held either old spelling. Production is expected to.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

------------------------------------------------------------------------------------
DECLARE @Execute          BIT = 0;   -- 0 = report only (default). 1 = apply.
DECLARE @IncludeFileNames BIT = 0;   -- 1 = also rewrite tblDocFile.UserFileName
------------------------------------------------------------------------------------

DECLARE @NewTag NVARCHAR(50) = N'PatR8-10';

DECLARE @old TABLE (Spelling NVARCHAR(50));
INSERT @old (Spelling) VALUES (N'PatR8-R10v2.1'), (N'PatR8-10v2.1');

DECLARE @targets TABLE (TableName SYSNAME, ColName SYSNAME);
INSERT @targets (TableName, ColName) VALUES
    ('tblCPiSystem',   'SystemName'),
    ('tblRelease',     'Name'),
    ('tblDocDocument', 'DocName'),
    ('tblDocDocument', 'Remarks');

IF @IncludeFileNames = 1
    INSERT @targets (TableName, ColName) VALUES ('tblDocFile', 'UserFileName');

-- Skip anything this database does not have.
DELETE t FROM @targets t
WHERE NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS c
    WHERE c.TABLE_NAME = t.TableName AND c.COLUMN_NAME = t.ColName);

IF NOT EXISTS (SELECT 1 FROM @targets)
BEGIN
    PRINT 'None of the target tables exist in this database - nothing to do.';
    RETURN;
END

DECLARE @tbl SYSNAME, @col SYSNAME, @sql NVARCHAR(MAX), @n INT;
DECLARE @results TABLE (TableName SYSNAME, ColName SYSNAME, RowsAffected INT);
DECLARE @preview TABLE (TableName SYSNAME, ColName SYSNAME, CurrentValue NVARCHAR(400), WouldBecome NVARCHAR(400));

DECLARE @filter NVARCHAR(MAX) =
    N'(' + STUFF((SELECT N' OR %COL% LIKE ''%' + Spelling + N'%''' FROM @old FOR XML PATH('')), 1, 4, N'') + N')';

DECLARE @rewrite NVARCHAR(MAX) = N'%COL%';
SELECT @rewrite = N'REPLACE(' + @rewrite + N', ''' + Spelling + N''', ''' + @NewTag + N''')' FROM @old;

DECLARE cur CURSOR LOCAL FAST_FORWARD FOR
    SELECT TableName, ColName FROM @targets ORDER BY TableName, ColName;

------------------------------------------------------------------------------------
-- REPORT
------------------------------------------------------------------------------------
IF @Execute = 0
BEGIN
    PRINT '*** REPORT ONLY - nothing has been written. Set @Execute = 1 to apply. ***';
    PRINT '';

    OPEN cur;
    FETCH NEXT FROM cur INTO @tbl, @col;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @sql = N'SELECT TOP 50 ''' + @tbl + N''', ''' + @col + N''','
                 + N' CONVERT(NVARCHAR(400), ' + QUOTENAME(@col) + N'),'
                 + N' CONVERT(NVARCHAR(400), ' + REPLACE(@rewrite, N'%COL%', QUOTENAME(@col)) + N')'
                 + N' FROM ' + QUOTENAME(@tbl)
                 + N' WHERE ' + REPLACE(@filter, N'%COL%', QUOTENAME(@col)) + N';';
        INSERT @preview EXEC sp_executesql @sql;

        SET @sql = N'SELECT @c = COUNT(*) FROM ' + QUOTENAME(@tbl)
                 + N' WHERE ' + REPLACE(@filter, N'%COL%', QUOTENAME(@col)) + N';';
        EXEC sp_executesql @sql, N'@c INT OUTPUT', @c = @n OUTPUT;
        IF @n > 0 INSERT @results VALUES (@tbl, @col, @n);

        FETCH NEXT FROM cur INTO @tbl, @col;
    END
    CLOSE cur; DEALLOCATE cur;

    IF NOT EXISTS (SELECT 1 FROM @results)
    BEGIN
        PRINT 'No names hold either old spelling - nothing to change in this database.';
        RETURN;
    END

    SELECT TableName, ColName, RowsAffected AS RowsToChange FROM @results ORDER BY TableName, ColName;
    PRINT 'Values that would be rewritten (first 50 per column):';
    SELECT TableName, ColName, CurrentValue, WouldBecome FROM @preview ORDER BY TableName, ColName, CurrentValue;

    IF EXISTS (
        SELECT 1 FROM tblCPiSystem a
        JOIN tblCPiSystem b ON b.SystemName = @NewTag
        WHERE a.SystemName IN (SELECT Spelling FROM @old))
        PRINT '!! tblCPiSystem already has a "' + @NewTag + '" row as well as an old-spelling row. Merge them by hand before applying.';

    RETURN;
END

------------------------------------------------------------------------------------
-- APPLY
------------------------------------------------------------------------------------

-- Pre-flight: the unique index on tblCPiSystem.SystemName.
IF EXISTS (
    SELECT 1 FROM tblCPiSystem a
    JOIN tblCPiSystem b ON b.SystemName = @NewTag
    WHERE a.SystemName IN (SELECT Spelling FROM @old))
BEGIN
    RAISERROR('Refusing to run: tblCPiSystem holds both "%s" and an old spelling. Renaming would violate UQ_tblCPiSystem_SystemName. Merge the rows by hand first.', 16, 1, @NewTag);
    RETURN;
END

BEGIN TRAN;

OPEN cur;
FETCH NEXT FROM cur INTO @tbl, @col;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @sql = N'UPDATE ' + QUOTENAME(@tbl)
             + N'   SET ' + QUOTENAME(@col) + N' = ' + REPLACE(@rewrite, N'%COL%', QUOTENAME(@col))
             + N' WHERE ' + REPLACE(@filter, N'%COL%', QUOTENAME(@col)) + N';';
    EXEC sp_executesql @sql;

    SET @n = @@ROWCOUNT;
    IF @n > 0 INSERT @results VALUES (@tbl, @col, @n);

    FETCH NEXT FROM cur INTO @tbl, @col;
END
CLOSE cur; DEALLOCATE cur;

-- Sanity gate: no targeted column may still hold either old spelling.
DECLARE @left INT = 0, @check NVARCHAR(MAX) = N'';

SELECT @check = @check + N'SELECT @c = @c + COUNT(*) FROM ' + QUOTENAME(TableName)
              + N' WHERE ' + REPLACE(@filter, N'%COL%', QUOTENAME(ColName)) + N';'
FROM @targets;

EXEC sp_executesql @check, N'@c INT OUTPUT', @c = @left OUTPUT;

IF @left > 0
BEGIN
    ROLLBACK;
    RAISERROR('Rolled back: %d rows still hold an old spelling after the update.', 16, 1, @left);
END
ELSE
BEGIN
    COMMIT;
    PRINT 'Committed.';
    IF EXISTS (SELECT 1 FROM @results)
    BEGIN
        SELECT TableName, ColName, RowsAffected AS RowsChanged FROM @results ORDER BY TableName, ColName;
        SELECT SUM(RowsAffected) AS TotalRowsChanged FROM @results;
    END
    ELSE
        PRINT 'Nothing needed changing.';
END
