-- Schema updates required for the current working tree.
-- Apply these against the main LawPortal SQL Server database before running the site.

-- ReportNotes column on tblRelease — free-form text rendered at the top of the
-- generated release-report PDF (author-time announcements that aren't driven by
-- MDB diffs, e.g. "all Opposition actions renamed to Opposition Period Ends").
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'tblRelease' AND COLUMN_NAME = 'ReportNotes'
)
BEGIN
    ALTER TABLE tblRelease ADD ReportNotes NVARCHAR(MAX) NULL;
END;
