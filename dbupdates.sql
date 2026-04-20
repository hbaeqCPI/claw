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
