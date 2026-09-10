/*
    Rename the patent R8/R10 system tag:  "PatR8-R10v2.1"  ->  "PatR8-10"

    WHY
      The tag is stored as data, not just in code. It appears in the Systems column
      of every tblPat and tblTmk table that carries one (a comma-separated list on most,
      a single tag on tblPatCountry/tblTmkCountry where it is part of the primary key)
      and in tblRelease.SystemType. SystemsHelper.SystemNames in the app now says
      "PatR8-10", so any row still holding the old tag stops matching: it drops out of
      systems filters on the search screens, and MDB generation takes the wrong branch
      for that release.

    HOW TO RUN
      1. Take a database backup first. This edits data in place.
      2. Run as-is. @Execute = 0 only REPORTS what would change - nothing is written.
      3. Read the report. If the tables and counts look right, set @Execute = 1
         and run again to apply.

    SAFETY
      - Idempotent: the WHERE clause matches only rows still holding the old tag, so
        re-running after a successful run changes nothing.
      - One transaction with XACT_ABORT, plus a sanity gate: if any targeted row still
        holds the old tag after the updates, everything is rolled back.
      - REPLACE only rewrites that exact substring, so neighbouring tags in the same
        comma-separated list (R4, PatR5-7, PatR10v2.2, ...) are untouched.

    SCOPE
      Main tables only (name LIKE 'tbl%'). The hist_* snapshots are deliberately left
      alone by default - see @IncludeHistory below.

    Run it against every database that holds these tables. WebUpdates has no Systems or
    SystemType column, so there is nothing to do there.

    Applied to the local LawPortalCPiR10 on 2026-09-10: 21,186 rows over 19 tables
    (18 tblPat* tables plus tblRelease.SystemType). Production counts will differ.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

------------------------------------------------------------------------------------
DECLARE @Execute        BIT = 0;   -- 0 = report only (default). Set to 1 to apply.
DECLARE @IncludeHistory BIT = 0;   -- see the note at the bottom of this file
------------------------------------------------------------------------------------

DECLARE @OldTag NVARCHAR(100) = N'PatR8-R10v2.1';
DECLARE @NewTag NVARCHAR(100) = N'PatR8-10';

-- Wildcards on both sides: the tag usually sits inside a comma-separated list.
DECLARE @Pattern NVARCHAR(120) = N'%' + @OldTag + N'%';

DECLARE @targets TABLE (TableName SYSNAME, ColName SYSNAME);

INSERT @targets (TableName, ColName)
SELECT c.TABLE_NAME, c.COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS c
JOIN INFORMATION_SCHEMA.TABLES t
    ON  t.TABLE_SCHEMA = c.TABLE_SCHEMA
    AND t.TABLE_NAME   = c.TABLE_NAME
    AND t.TABLE_TYPE   = 'BASE TABLE'
WHERE c.COLUMN_NAME IN ('Systems', 'SystemType')
  AND c.DATA_TYPE IN ('varchar', 'nvarchar', 'char', 'nchar')
  AND (c.TABLE_NAME LIKE 'tbl%' OR (@IncludeHistory = 1 AND c.TABLE_NAME LIKE 'hist[_]%'));

IF NOT EXISTS (SELECT 1 FROM @targets)
BEGIN
    PRINT 'No Systems/SystemType columns found in this database - nothing to do.';
    RETURN;
END

DECLARE @results TABLE (TableName SYSNAME, ColName SYSNAME, RowsAffected INT);
DECLARE @tbl SYSNAME, @col SYSNAME, @sql NVARCHAR(MAX), @n INT;

DECLARE cur CURSOR LOCAL FAST_FORWARD FOR
    SELECT TableName, ColName FROM @targets ORDER BY TableName, ColName;

------------------------------------------------------------------------------------
-- REPORT MODE
------------------------------------------------------------------------------------
IF @Execute = 0
BEGIN
    PRINT '*** REPORT ONLY - nothing has been written. Set @Execute = 1 to apply. ***';
    PRINT '';

    OPEN cur;
    FETCH NEXT FROM cur INTO @tbl, @col;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @sql = N'SELECT @c = COUNT(*) FROM ' + QUOTENAME(@tbl)
                 + N' WHERE ' + QUOTENAME(@col) + N' LIKE @pat;';
        EXEC sp_executesql @sql, N'@c INT OUTPUT, @pat NVARCHAR(120)',
                           @c = @n OUTPUT, @pat = @Pattern;

        IF @n > 0 INSERT @results VALUES (@tbl, @col, @n);

        FETCH NEXT FROM cur INTO @tbl, @col;
    END
    CLOSE cur; DEALLOCATE cur;

    IF NOT EXISTS (SELECT 1 FROM @results)
        PRINT 'No rows hold the old tag - this database is already renamed.';
    ELSE
    BEGIN
        SELECT TableName, ColName, RowsAffected AS RowsToChange FROM @results ORDER BY TableName;
        SELECT SUM(RowsAffected) AS TotalRowsToChange, COUNT(*) AS TablesAffected FROM @results;
    END

    RETURN;
END

------------------------------------------------------------------------------------
-- APPLY
------------------------------------------------------------------------------------
BEGIN TRAN;

OPEN cur;
FETCH NEXT FROM cur INTO @tbl, @col;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @sql = N'UPDATE ' + QUOTENAME(@tbl)
             + N'   SET ' + QUOTENAME(@col) + N' = REPLACE(' + QUOTENAME(@col) + N', @old, @new)'
             + N' WHERE ' + QUOTENAME(@col) + N' LIKE @pat;';

    EXEC sp_executesql @sql,
         N'@old NVARCHAR(100), @new NVARCHAR(100), @pat NVARCHAR(120)',
         @old = @OldTag, @new = @NewTag, @pat = @Pattern;

    SET @n = @@ROWCOUNT;
    IF @n > 0 INSERT @results VALUES (@tbl, @col, @n);

    FETCH NEXT FROM cur INTO @tbl, @col;
END
CLOSE cur; DEALLOCATE cur;

-- Sanity gate: nothing in scope may still hold the old tag.
DECLARE @left INT = 0, @check NVARCHAR(MAX) = N'';

SELECT @check = @check + N'SELECT @c = @c + COUNT(*) FROM ' + QUOTENAME(TableName)
              + N' WHERE ' + QUOTENAME(ColName) + N' LIKE ''%' + @OldTag + N'%'';'
FROM @targets;

EXEC sp_executesql @check, N'@c INT OUTPUT', @c = @left OUTPUT;

IF @left > 0
BEGIN
    ROLLBACK;
    RAISERROR('Rolled back: %d rows still hold the old tag after the update.', 16, 1, @left);
END
ELSE
BEGIN
    COMMIT;
    PRINT 'Committed.';
    SELECT TableName, ColName, RowsAffected AS RowsChanged FROM @results ORDER BY TableName;
    SELECT SUM(RowsAffected) AS TotalRowsChanged, COUNT(*) AS TablesChanged FROM @results;
END

/*
    ABOUT @IncludeHistory

    The hist_* tables are the baseline the Manual Updates report diffs against. Leaving
    them on the old tag (the default, and what was done locally) means the Systems column
    now differs from the live tables for every one of those rows, so that report will show
    them as changed. Renaming them too (@IncludeHistory = 1) keeps the diff quiet but
    rewrites historical snapshots.

    Locally that is 18 hist_ tables / ~42k rows. Decide which behaviour you want for
    production before running, and use the same setting everywhere so environments match.
*/
