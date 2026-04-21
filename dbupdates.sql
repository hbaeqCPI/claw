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
