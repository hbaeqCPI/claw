-- Schema updates required for the current working tree.
-- Apply these against the main LawPortal SQL Server database before running the site.

-- ReportNotes columns on tblRelease — free-form text rendered at the top of the
-- generated release-report PDF (author-time announcements that aren't driven by
-- MDB diffs, e.g. "all Opposition actions renamed to Opposition Period Ends").
-- Separate patent / trademark so each report can carry its own narrative.

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'tblRelease' AND COLUMN_NAME = 'ReportNotesPatent'
)
BEGIN
    ALTER TABLE tblRelease ADD ReportNotesPatent NVARCHAR(MAX) NULL;
END;
GO

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'tblRelease' AND COLUMN_NAME = 'ReportNotesTrademark'
)
BEGIN
    ALTER TABLE tblRelease ADD ReportNotesTrademark NVARCHAR(MAX) NULL;
END;
GO

-- If the legacy single ReportNotes column exists, copy its content to both new
-- columns (only where those are still NULL) before dropping it.
-- Use dynamic SQL so the statements still parse on databases where the legacy
-- column was never present.
IF EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'tblRelease' AND COLUMN_NAME = 'ReportNotes'
)
BEGIN
    EXEC sp_executesql N'
        UPDATE tblRelease
           SET ReportNotesPatent = ReportNotes
         WHERE ReportNotesPatent IS NULL AND ReportNotes IS NOT NULL;

        UPDATE tblRelease
           SET ReportNotesTrademark = ReportNotes
         WHERE ReportNotesTrademark IS NULL AND ReportNotes IS NOT NULL;
    ';

    ALTER TABLE tblRelease DROP COLUMN ReportNotes;
END;
GO

-- InternalRemarks on tblPatCountryLaw / tblTmkCountryLaw — author-time notes
-- that are NEVER exported to MDB and NEVER rendered in release PDFs.
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'tblPatCountryLaw' AND COLUMN_NAME = 'InternalRemarks'
)
BEGIN
    ALTER TABLE tblPatCountryLaw ADD InternalRemarks NVARCHAR(MAX) NOT NULL CONSTRAINT DF_tblPatCountryLaw_InternalRemarks DEFAULT '';
END;
GO

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'tblTmkCountryLaw' AND COLUMN_NAME = 'InternalRemarks'
)
BEGIN
    ALTER TABLE tblTmkCountryLaw ADD InternalRemarks NVARCHAR(MAX) NOT NULL CONSTRAINT DF_tblTmkCountryLaw_InternalRemarks DEFAULT '';
END;
GO

-- ActionParameter tables — per-ActionType templates (Yr/Mo/Dy offsets + Indicator)
-- used to generate action dues retroactively. Exported to MDB as a temp comparison
-- target so we can diff parameter changes between releases. Ported from R10v22.
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_NAME = 'tblPatActionParameter'
)
BEGIN
    CREATE TABLE tblPatActionParameter (
        ActParamId   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_tblPatActionParameter PRIMARY KEY,
        ActionTypeID INT NOT NULL,
        ActionDue    NVARCHAR(60) NOT NULL,
        Yr           INT NOT NULL CONSTRAINT DF_tblPatActionParameter_Yr DEFAULT 0,
        Mo           INT NOT NULL CONSTRAINT DF_tblPatActionParameter_Mo DEFAULT 0,
        Dy           INT NOT NULL CONSTRAINT DF_tblPatActionParameter_Dy DEFAULT 0,
        Indicator    NVARCHAR(20) NOT NULL CONSTRAINT DF_tblPatActionParameter_Indicator DEFAULT 'Reminder',
        CreatedBy    NVARCHAR(20) NULL,
        UpdatedBy    NVARCHAR(20) NULL,
        DateCreated  DATETIME NULL,
        LastUpdate   DATETIME NULL
    );
    CREATE UNIQUE INDEX UX_tblPatActionParameter
        ON tblPatActionParameter (ActionTypeID, ActionDue, Yr, Mo, Dy);
END;
GO

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_NAME = 'tblTmkActionParameter'
)
BEGIN
    CREATE TABLE tblTmkActionParameter (
        ActParamId   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_tblTmkActionParameter PRIMARY KEY,
        ActionTypeID INT NOT NULL,
        ActionDue    NVARCHAR(60) NOT NULL,
        Yr           INT NOT NULL CONSTRAINT DF_tblTmkActionParameter_Yr DEFAULT 0,
        Mo           INT NOT NULL CONSTRAINT DF_tblTmkActionParameter_Mo DEFAULT 0,
        Dy           INT NOT NULL CONSTRAINT DF_tblTmkActionParameter_Dy DEFAULT 0,
        Indicator    NVARCHAR(20) NOT NULL CONSTRAINT DF_tblTmkActionParameter_Indicator DEFAULT 'Reminder',
        CreatedBy    NVARCHAR(20) NULL,
        UpdatedBy    NVARCHAR(20) NULL,
        DateCreated  DATETIME NULL,
        LastUpdate   DATETIME NULL
    );
    CREATE UNIQUE INDEX UX_tblTmkActionParameter
        ON tblTmkActionParameter (ActionTypeID, ActionDue, Yr, Mo, Dy);
END;
GO

-- Widen BasedOn / EffBasedOn to NVARCHAR(30) and make EffBasedOn nullable
-- across CountryDue / CountryExp tables on both the Pat and Tmk sides.
-- ALTER COLUMN is idempotent; the IF EXISTS guard just skips on databases
-- where the table is missing entirely.

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tblPatCountryDue' AND COLUMN_NAME = 'BasedOn')
BEGIN
    ALTER TABLE tblPatCountryDue ALTER COLUMN BasedOn NVARCHAR(30) NOT NULL;
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tblPatCountryDue' AND COLUMN_NAME = 'EffBasedOn')
BEGIN
    ALTER TABLE tblPatCountryDue ALTER COLUMN EffBasedOn NVARCHAR(30) NULL;
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tblPatCountryExp' AND COLUMN_NAME = 'BasedOn')
BEGIN
    ALTER TABLE tblPatCountryExp ALTER COLUMN BasedOn NVARCHAR(30) NOT NULL;
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tblPatCountryExp' AND COLUMN_NAME = 'EffBasedOn')
BEGIN
    ALTER TABLE tblPatCountryExp ALTER COLUMN EffBasedOn NVARCHAR(30) NULL;
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tblPatCountryExpDelete' AND COLUMN_NAME = 'BasedOn')
BEGIN
    ALTER TABLE tblPatCountryExpDelete ALTER COLUMN BasedOn NVARCHAR(30) NOT NULL;
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tblPatCountryExpDelete' AND COLUMN_NAME = 'EffBasedOn')
BEGIN
    ALTER TABLE tblPatCountryExpDelete ALTER COLUMN EffBasedOn NVARCHAR(30) NULL;
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tblTmkCountryDue' AND COLUMN_NAME = 'BasedOn')
BEGIN
    ALTER TABLE tblTmkCountryDue ALTER COLUMN BasedOn NVARCHAR(30) NOT NULL;
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tblTmkCountryDue' AND COLUMN_NAME = 'EffBasedOn')
BEGIN
    ALTER TABLE tblTmkCountryDue ALTER COLUMN EffBasedOn NVARCHAR(30) NULL;
END;
GO

-- Add PRIMARY KEY constraints on the surrogate-id columns of the CountryDue /
-- CountryExp tables. Without these, the table is a heap and the
-- "MAX(id)+1 then INSERT" generation pattern in the controllers races under
-- concurrent writes. tblTmkCountryDue had 169 colliding CDueIds from earlier
-- runs of that pattern; this script renumbers them before adding the PK.

-- 1) Deduplicate any rows that share a CDueId in tblTmkCountryDue.
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblTmkCountryDue')
   AND NOT EXISTS (
       SELECT 1 FROM sys.key_constraints
       WHERE name = 'PK_tblTmkCountryDue' AND parent_object_id = OBJECT_ID('dbo.tblTmkCountryDue')
   )
   AND EXISTS (SELECT 1 FROM tblTmkCountryDue GROUP BY CDueId HAVING COUNT(*) > 1)
BEGIN
    ALTER TABLE tblTmkCountryDue ADD __dedupeKey uniqueidentifier NULL;

    EXEC sp_executesql N'UPDATE tblTmkCountryDue SET __dedupeKey = NEWID();';

    EXEC sp_executesql N'
        DECLARE @baseId int = (SELECT ISNULL(MAX(CDueId),0) FROM tblTmkCountryDue);
        ;WITH ranked AS (
            SELECT __dedupeKey, CDueId, ROW_NUMBER() OVER (PARTITION BY CDueId ORDER BY __dedupeKey) AS rn
            FROM tblTmkCountryDue
        ),
        toRenumber AS (
            SELECT __dedupeKey, ROW_NUMBER() OVER (ORDER BY CDueId, __dedupeKey) AS new_offset
            FROM ranked WHERE rn > 1
        )
        UPDATE t SET t.CDueId = @baseId + r.new_offset
        FROM tblTmkCountryDue t
        JOIN toRenumber r ON t.__dedupeKey = r.__dedupeKey;
    ';

    ALTER TABLE tblTmkCountryDue DROP COLUMN __dedupeKey;
END;
GO

-- 2) Add the PRIMARY KEY constraints (idempotent).
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblPatCountryDue')
   AND NOT EXISTS (
       SELECT 1 FROM sys.key_constraints
       WHERE name = 'PK_tblPatCountryDue' AND parent_object_id = OBJECT_ID('dbo.tblPatCountryDue')
   )
BEGIN
    ALTER TABLE tblPatCountryDue ADD CONSTRAINT PK_tblPatCountryDue PRIMARY KEY (CDueId);
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblPatCountryExp')
   AND NOT EXISTS (
       SELECT 1 FROM sys.key_constraints
       WHERE name = 'PK_tblPatCountryExp' AND parent_object_id = OBJECT_ID('dbo.tblPatCountryExp')
   )
BEGIN
    ALTER TABLE tblPatCountryExp ADD CONSTRAINT PK_tblPatCountryExp PRIMARY KEY (CExpId);
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblPatCountryExpDelete')
   AND NOT EXISTS (
       SELECT 1 FROM sys.key_constraints
       WHERE name = 'PK_tblPatCountryExpDelete' AND parent_object_id = OBJECT_ID('dbo.tblPatCountryExpDelete')
   )
BEGIN
    ALTER TABLE tblPatCountryExpDelete ADD CONSTRAINT PK_tblPatCountryExpDelete PRIMARY KEY (CExpId);
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblTmkCountryDue')
   AND NOT EXISTS (
       SELECT 1 FROM sys.key_constraints
       WHERE name = 'PK_tblTmkCountryDue' AND parent_object_id = OBJECT_ID('dbo.tblTmkCountryDue')
   )
BEGIN
    ALTER TABLE tblTmkCountryDue ADD CONSTRAINT PK_tblTmkCountryDue PRIMARY KEY (CDueId);
END;
GO

-- tblDeployPassword — stores deployments keyed by Year + Quarter + PatentPassword
-- + TrademarkPassword. Multiple deployments per year/quarter are allowed;
-- uniqueness covers the full tuple to prevent exact duplicates.
-- PatentPassword / TrademarkPassword are plain text (no validation, max 30 chars)
-- so they can be displayed back to the user on the Deploy screen. Both default
-- to '' (empty string) rather than NULL so the unique index treats "no password"
-- consistently across rows.
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_NAME = 'tblDeployPassword'
)
BEGIN
    CREATE TABLE tblDeployPassword (
        DeployPasswordId   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_tblDeployPassword PRIMARY KEY,
        [Year]             INT NOT NULL,
        Quarter            NVARCHAR(2) NOT NULL,
        PatentPassword     NVARCHAR(30) NOT NULL CONSTRAINT DF_tblDeployPassword_PatentPassword DEFAULT '',
        TrademarkPassword  NVARCHAR(30) NOT NULL CONSTRAINT DF_tblDeployPassword_TrademarkPassword DEFAULT '',
        CreatedBy          NVARCHAR(20) NULL,
        UpdatedBy          NVARCHAR(20) NULL,
        DateCreated        DATETIME NULL,
        LastUpdate         DATETIME NULL
    );
    CREATE UNIQUE INDEX UX_tblDeployPassword
        ON tblDeployPassword ([Year], Quarter, PatentPassword, TrademarkPassword);
END;
GO

-- Migrate older databases: split single UpdatePassword column into
-- PatentPassword + TrademarkPassword, and rebuild the unique index over all
-- four columns. Idempotent.
IF EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'tblDeployPassword' AND COLUMN_NAME = 'UpdatePassword'
)
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_NAME = 'tblDeployPassword' AND COLUMN_NAME = 'PatentPassword'
    )
    BEGIN
        ALTER TABLE tblDeployPassword
            ADD PatentPassword NVARCHAR(30) NOT NULL CONSTRAINT DF_tblDeployPassword_PatentPassword DEFAULT '';
    END;

    IF NOT EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_NAME = 'tblDeployPassword' AND COLUMN_NAME = 'TrademarkPassword'
    )
    BEGIN
        ALTER TABLE tblDeployPassword
            ADD TrademarkPassword NVARCHAR(30) NOT NULL CONSTRAINT DF_tblDeployPassword_TrademarkPassword DEFAULT '';
    END;

    -- Carry over any existing password into PatentPassword (best-effort, since
    -- the old single column had no concept of which side it belonged to).
    EXEC sp_executesql N'
        UPDATE tblDeployPassword
           SET PatentPassword = UpdatePassword
         WHERE PatentPassword = '''' AND UpdatePassword <> '''';
    ';

    -- Drop the legacy index before dropping the column it references.
    IF EXISTS (
        SELECT 1 FROM sys.indexes i
        JOIN sys.tables t ON i.object_id = t.object_id
        WHERE t.name = 'tblDeployPassword' AND i.name = 'UX_tblDeployPassword'
    )
    BEGIN
        DROP INDEX UX_tblDeployPassword ON tblDeployPassword;
    END;

    ALTER TABLE tblDeployPassword DROP COLUMN UpdatePassword;
END;
GO

-- Make sure the unique index is on the full (Year, Quarter, PatentPassword,
-- TrademarkPassword) tuple. Rebuild if a previous version is present with a
-- different column set.
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_NAME = 'tblDeployPassword'
)
BEGIN
    -- Table doesn't exist yet (fresh DB before the first CREATE TABLE block ran);
    -- nothing to migrate.
    PRINT 'tblDeployPassword not present yet — skipping index rebuild.';
END
ELSE IF NOT EXISTS (
    SELECT 1 FROM sys.indexes i
    JOIN sys.tables t ON i.object_id = t.object_id
    WHERE t.name = 'tblDeployPassword' AND i.name = 'UX_tblDeployPassword'
)
BEGIN
    CREATE UNIQUE INDEX UX_tblDeployPassword
        ON tblDeployPassword ([Year], Quarter, PatentPassword, TrademarkPassword);
END
ELSE IF NOT EXISTS (
    SELECT 1 FROM sys.indexes i
    JOIN sys.tables t ON i.object_id = t.object_id
    JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
    JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
    WHERE t.name = 'tblDeployPassword' AND i.name = 'UX_tblDeployPassword'
      AND c.name = 'TrademarkPassword'
)
BEGIN
    DROP INDEX UX_tblDeployPassword ON tblDeployPassword;
    CREATE UNIQUE INDEX UX_tblDeployPassword
        ON tblDeployPassword ([Year], Quarter, PatentPassword, TrademarkPassword);
END;
GO
