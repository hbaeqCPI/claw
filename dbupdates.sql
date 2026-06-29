-- Schema updates required for the current working tree.

-- Lock fields on tblRelease — set when a Deploy record for the same Year+Quarter
-- is locked, preventing further MDB/report generation and note editing.
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tblRelease' AND COLUMN_NAME = 'IsLocked')
BEGIN
    ALTER TABLE tblRelease ADD IsLocked BIT NOT NULL CONSTRAINT DF_tblRelease_IsLocked DEFAULT 0;
END;
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tblRelease' AND COLUMN_NAME = 'LockedAt')
BEGIN
    ALTER TABLE tblRelease ADD LockedAt DATETIME NULL;
END;
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tblRelease' AND COLUMN_NAME = 'LockedBy')
BEGIN
    ALTER TABLE tblRelease ADD LockedBy NVARCHAR(100) NULL;
END;
GO

-- Quarter lock fields on tblDeployPassword. IsLocked is permanent once set;
-- LockedAt/LockedBy record who triggered the snapshot.
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tblDeployPassword' AND COLUMN_NAME = 'IsLocked')
BEGIN
    ALTER TABLE tblDeployPassword ADD IsLocked BIT NOT NULL CONSTRAINT DF_tblDeployPassword_IsLocked DEFAULT 0;
END;
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tblDeployPassword' AND COLUMN_NAME = 'LockedAt')
BEGIN
    ALTER TABLE tblDeployPassword ADD LockedAt DATETIME NULL;
END;
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tblDeployPassword' AND COLUMN_NAME = 'LockedBy')
BEGIN
    ALTER TABLE tblDeployPassword ADD LockedBy NVARCHAR(100) NULL;
END;
GO

-- Quarter snapshot tables (hist_*). Each mirrors a live tbl* table plus
-- SnapshotYear / SnapshotQuarter columns. Created via SELECT TOP 0 … INTO so the
-- schema is always derived from the source — no manual column lists to maintain.
-- The lock action does: INSERT INTO hist_tblXxx SELECT *, @year, @quarter FROM tblXxx

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblPatActionParameter')
   AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'hist_tblPatActionParameter')
BEGIN
    SELECT TOP 0 *, CAST(0 AS INT) AS SnapshotYear, CAST('  ' AS NVARCHAR(2)) AS SnapshotQuarter
    INTO hist_tblPatActionParameter FROM tblPatActionParameter;
    CREATE INDEX IX_hist_tblPatActionParameter ON hist_tblPatActionParameter (SnapshotYear, SnapshotQuarter);
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblPatActionType')
   AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'hist_tblPatActionType')
BEGIN
    SELECT TOP 0 *, CAST(0 AS INT) AS SnapshotYear, CAST('  ' AS NVARCHAR(2)) AS SnapshotQuarter
    INTO hist_tblPatActionType FROM tblPatActionType;
    CREATE INDEX IX_hist_tblPatActionType ON hist_tblPatActionType (SnapshotYear, SnapshotQuarter);
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblPatArea')
   AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'hist_tblPatArea')
BEGIN
    SELECT TOP 0 *, CAST(0 AS INT) AS SnapshotYear, CAST('  ' AS NVARCHAR(2)) AS SnapshotQuarter
    INTO hist_tblPatArea FROM tblPatArea;
    CREATE INDEX IX_hist_tblPatArea ON hist_tblPatArea (SnapshotYear, SnapshotQuarter);
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblPatAreaCountry')
   AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'hist_tblPatAreaCountry')
BEGIN
    SELECT TOP 0 *, CAST(0 AS INT) AS SnapshotYear, CAST('  ' AS NVARCHAR(2)) AS SnapshotQuarter
    INTO hist_tblPatAreaCountry FROM tblPatAreaCountry;
    CREATE INDEX IX_hist_tblPatAreaCountry ON hist_tblPatAreaCountry (SnapshotYear, SnapshotQuarter);
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblPatAreaCountryDelete')
   AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'hist_tblPatAreaCountryDelete')
BEGIN
    SELECT TOP 0 *, CAST(0 AS INT) AS SnapshotYear, CAST('  ' AS NVARCHAR(2)) AS SnapshotQuarter
    INTO hist_tblPatAreaCountryDelete FROM tblPatAreaCountryDelete;
    CREATE INDEX IX_hist_tblPatAreaCountryDelete ON hist_tblPatAreaCountryDelete (SnapshotYear, SnapshotQuarter);
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblPatAreaDelete')
   AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'hist_tblPatAreaDelete')
BEGIN
    SELECT TOP 0 *, CAST(0 AS INT) AS SnapshotYear, CAST('  ' AS NVARCHAR(2)) AS SnapshotQuarter
    INTO hist_tblPatAreaDelete FROM tblPatAreaDelete;
    CREATE INDEX IX_hist_tblPatAreaDelete ON hist_tblPatAreaDelete (SnapshotYear, SnapshotQuarter);
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblPatAuditLog')
   AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'hist_tblPatAuditLog')
BEGIN
    SELECT TOP 0 *, CAST(0 AS INT) AS SnapshotYear, CAST('  ' AS NVARCHAR(2)) AS SnapshotQuarter
    INTO hist_tblPatAuditLog FROM tblPatAuditLog;
    CREATE INDEX IX_hist_tblPatAuditLog ON hist_tblPatAuditLog (SnapshotYear, SnapshotQuarter);
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblPatCaseType')
   AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'hist_tblPatCaseType')
BEGIN
    SELECT TOP 0 *, CAST(0 AS INT) AS SnapshotYear, CAST('  ' AS NVARCHAR(2)) AS SnapshotQuarter
    INTO hist_tblPatCaseType FROM tblPatCaseType;
    CREATE INDEX IX_hist_tblPatCaseType ON hist_tblPatCaseType (SnapshotYear, SnapshotQuarter);
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblPatCountry')
   AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'hist_tblPatCountry')
BEGIN
    SELECT TOP 0 *, CAST(0 AS INT) AS SnapshotYear, CAST('  ' AS NVARCHAR(2)) AS SnapshotQuarter
    INTO hist_tblPatCountry FROM tblPatCountry;
    CREATE INDEX IX_hist_tblPatCountry ON hist_tblPatCountry (SnapshotYear, SnapshotQuarter);
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblPatCountryDue')
   AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'hist_tblPatCountryDue')
BEGIN
    SELECT TOP 0 *, CAST(0 AS INT) AS SnapshotYear, CAST('  ' AS NVARCHAR(2)) AS SnapshotQuarter
    INTO hist_tblPatCountryDue FROM tblPatCountryDue;
    CREATE INDEX IX_hist_tblPatCountryDue ON hist_tblPatCountryDue (SnapshotYear, SnapshotQuarter);
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblPatCountryExp')
   AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'hist_tblPatCountryExp')
BEGIN
    SELECT TOP 0 *, CAST(0 AS INT) AS SnapshotYear, CAST('  ' AS NVARCHAR(2)) AS SnapshotQuarter
    INTO hist_tblPatCountryExp FROM tblPatCountryExp;
    CREATE INDEX IX_hist_tblPatCountryExp ON hist_tblPatCountryExp (SnapshotYear, SnapshotQuarter);
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblPatCountryExpDelete')
   AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'hist_tblPatCountryExpDelete')
BEGIN
    SELECT TOP 0 *, CAST(0 AS INT) AS SnapshotYear, CAST('  ' AS NVARCHAR(2)) AS SnapshotQuarter
    INTO hist_tblPatCountryExpDelete FROM tblPatCountryExpDelete;
    CREATE INDEX IX_hist_tblPatCountryExpDelete ON hist_tblPatCountryExpDelete (SnapshotYear, SnapshotQuarter);
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblPatCountryLaw')
   AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'hist_tblPatCountryLaw')
BEGIN
    SELECT TOP 0 *, CAST(0 AS INT) AS SnapshotYear, CAST('  ' AS NVARCHAR(2)) AS SnapshotQuarter
    INTO hist_tblPatCountryLaw FROM tblPatCountryLaw;
    CREATE INDEX IX_hist_tblPatCountryLaw ON hist_tblPatCountryLaw (SnapshotYear, SnapshotQuarter);
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblPatCountryLaw_Ext')
   AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'hist_tblPatCountryLaw_Ext')
BEGIN
    SELECT TOP 0 *, CAST(0 AS INT) AS SnapshotYear, CAST('  ' AS NVARCHAR(2)) AS SnapshotQuarter
    INTO hist_tblPatCountryLaw_Ext FROM tblPatCountryLaw_Ext;
    CREATE INDEX IX_hist_tblPatCountryLaw_Ext ON hist_tblPatCountryLaw_Ext (SnapshotYear, SnapshotQuarter);
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblPatCountryLawUpdate')
   AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'hist_tblPatCountryLawUpdate')
BEGIN
    SELECT TOP 0 *, CAST(0 AS INT) AS SnapshotYear, CAST('  ' AS NVARCHAR(2)) AS SnapshotQuarter
    INTO hist_tblPatCountryLawUpdate FROM tblPatCountryLawUpdate;
    CREATE INDEX IX_hist_tblPatCountryLawUpdate ON hist_tblPatCountryLawUpdate (SnapshotYear, SnapshotQuarter);
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblPatDesCaseType')
   AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'hist_tblPatDesCaseType')
BEGIN
    SELECT TOP 0 *, CAST(0 AS INT) AS SnapshotYear, CAST('  ' AS NVARCHAR(2)) AS SnapshotQuarter
    INTO hist_tblPatDesCaseType FROM tblPatDesCaseType;
    CREATE INDEX IX_hist_tblPatDesCaseType ON hist_tblPatDesCaseType (SnapshotYear, SnapshotQuarter);
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblPatDesCaseType_Ext')
   AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'hist_tblPatDesCaseType_Ext')
BEGIN
    SELECT TOP 0 *, CAST(0 AS INT) AS SnapshotYear, CAST('  ' AS NVARCHAR(2)) AS SnapshotQuarter
    INTO hist_tblPatDesCaseType_Ext FROM tblPatDesCaseType_Ext;
    CREATE INDEX IX_hist_tblPatDesCaseType_Ext ON hist_tblPatDesCaseType_Ext (SnapshotYear, SnapshotQuarter);
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblPatDesCaseTypeDelete')
   AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'hist_tblPatDesCaseTypeDelete')
BEGIN
    SELECT TOP 0 *, CAST(0 AS INT) AS SnapshotYear, CAST('  ' AS NVARCHAR(2)) AS SnapshotQuarter
    INTO hist_tblPatDesCaseTypeDelete FROM tblPatDesCaseTypeDelete;
    CREATE INDEX IX_hist_tblPatDesCaseTypeDelete ON hist_tblPatDesCaseTypeDelete (SnapshotYear, SnapshotQuarter);
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblPatDesCaseTypeDelete_Ext')
   AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'hist_tblPatDesCaseTypeDelete_Ext')
BEGIN
    SELECT TOP 0 *, CAST(0 AS INT) AS SnapshotYear, CAST('  ' AS NVARCHAR(2)) AS SnapshotQuarter
    INTO hist_tblPatDesCaseTypeDelete_Ext FROM tblPatDesCaseTypeDelete_Ext;
    CREATE INDEX IX_hist_tblPatDesCaseTypeDelete_Ext ON hist_tblPatDesCaseTypeDelete_Ext (SnapshotYear, SnapshotQuarter);
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblPatDesCaseTypeFields')
   AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'hist_tblPatDesCaseTypeFields')
BEGIN
    SELECT TOP 0 *, CAST(0 AS INT) AS SnapshotYear, CAST('  ' AS NVARCHAR(2)) AS SnapshotQuarter
    INTO hist_tblPatDesCaseTypeFields FROM tblPatDesCaseTypeFields;
    CREATE INDEX IX_hist_tblPatDesCaseTypeFields ON hist_tblPatDesCaseTypeFields (SnapshotYear, SnapshotQuarter);
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblPatDesCaseTypeFields_Ext')
   AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'hist_tblPatDesCaseTypeFields_Ext')
BEGIN
    SELECT TOP 0 *, CAST(0 AS INT) AS SnapshotYear, CAST('  ' AS NVARCHAR(2)) AS SnapshotQuarter
    INTO hist_tblPatDesCaseTypeFields_Ext FROM tblPatDesCaseTypeFields_Ext;
    CREATE INDEX IX_hist_tblPatDesCaseTypeFields_Ext ON hist_tblPatDesCaseTypeFields_Ext (SnapshotYear, SnapshotQuarter);
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblPatDesCaseTypeFieldsDelete')
   AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'hist_tblPatDesCaseTypeFieldsDelete')
BEGIN
    SELECT TOP 0 *, CAST(0 AS INT) AS SnapshotYear, CAST('  ' AS NVARCHAR(2)) AS SnapshotQuarter
    INTO hist_tblPatDesCaseTypeFieldsDelete FROM tblPatDesCaseTypeFieldsDelete;
    CREATE INDEX IX_hist_tblPatDesCaseTypeFieldsDelete ON hist_tblPatDesCaseTypeFieldsDelete (SnapshotYear, SnapshotQuarter);
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblPatDesCaseTypeFieldsDelete_Ext')
   AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'hist_tblPatDesCaseTypeFieldsDelete_Ext')
BEGIN
    SELECT TOP 0 *, CAST(0 AS INT) AS SnapshotYear, CAST('  ' AS NVARCHAR(2)) AS SnapshotQuarter
    INTO hist_tblPatDesCaseTypeFieldsDelete_Ext FROM tblPatDesCaseTypeFieldsDelete_Ext;
    CREATE INDEX IX_hist_tblPatDesCaseTypeFieldsDelete_Ext ON hist_tblPatDesCaseTypeFieldsDelete_Ext (SnapshotYear, SnapshotQuarter);
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblPatDesignatedCountry')
   AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'hist_tblPatDesignatedCountry')
BEGIN
    SELECT TOP 0 *, CAST(0 AS INT) AS SnapshotYear, CAST('  ' AS NVARCHAR(2)) AS SnapshotQuarter
    INTO hist_tblPatDesignatedCountry FROM tblPatDesignatedCountry;
    CREATE INDEX IX_hist_tblPatDesignatedCountry ON hist_tblPatDesignatedCountry (SnapshotYear, SnapshotQuarter);
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblPatIndicator')
   AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'hist_tblPatIndicator')
BEGIN
    SELECT TOP 0 *, CAST(0 AS INT) AS SnapshotYear, CAST('  ' AS NVARCHAR(2)) AS SnapshotQuarter
    INTO hist_tblPatIndicator FROM tblPatIndicator;
    CREATE INDEX IX_hist_tblPatIndicator ON hist_tblPatIndicator (SnapshotYear, SnapshotQuarter);
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblTmkActionParameter')
   AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'hist_tblTmkActionParameter')
BEGIN
    SELECT TOP 0 *, CAST(0 AS INT) AS SnapshotYear, CAST('  ' AS NVARCHAR(2)) AS SnapshotQuarter
    INTO hist_tblTmkActionParameter FROM tblTmkActionParameter;
    CREATE INDEX IX_hist_tblTmkActionParameter ON hist_tblTmkActionParameter (SnapshotYear, SnapshotQuarter);
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblTmkActionType')
   AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'hist_tblTmkActionType')
BEGIN
    SELECT TOP 0 *, CAST(0 AS INT) AS SnapshotYear, CAST('  ' AS NVARCHAR(2)) AS SnapshotQuarter
    INTO hist_tblTmkActionType FROM tblTmkActionType;
    CREATE INDEX IX_hist_tblTmkActionType ON hist_tblTmkActionType (SnapshotYear, SnapshotQuarter);
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblTmkArea')
   AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'hist_tblTmkArea')
BEGIN
    SELECT TOP 0 *, CAST(0 AS INT) AS SnapshotYear, CAST('  ' AS NVARCHAR(2)) AS SnapshotQuarter
    INTO hist_tblTmkArea FROM tblTmkArea;
    CREATE INDEX IX_hist_tblTmkArea ON hist_tblTmkArea (SnapshotYear, SnapshotQuarter);
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblTmkAreaCountry')
   AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'hist_tblTmkAreaCountry')
BEGIN
    SELECT TOP 0 *, CAST(0 AS INT) AS SnapshotYear, CAST('  ' AS NVARCHAR(2)) AS SnapshotQuarter
    INTO hist_tblTmkAreaCountry FROM tblTmkAreaCountry;
    CREATE INDEX IX_hist_tblTmkAreaCountry ON hist_tblTmkAreaCountry (SnapshotYear, SnapshotQuarter);
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblTmkAreaCountryDelete')
   AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'hist_tblTmkAreaCountryDelete')
BEGIN
    SELECT TOP 0 *, CAST(0 AS INT) AS SnapshotYear, CAST('  ' AS NVARCHAR(2)) AS SnapshotQuarter
    INTO hist_tblTmkAreaCountryDelete FROM tblTmkAreaCountryDelete;
    CREATE INDEX IX_hist_tblTmkAreaCountryDelete ON hist_tblTmkAreaCountryDelete (SnapshotYear, SnapshotQuarter);
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblTmkAreaDelete')
   AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'hist_tblTmkAreaDelete')
BEGIN
    SELECT TOP 0 *, CAST(0 AS INT) AS SnapshotYear, CAST('  ' AS NVARCHAR(2)) AS SnapshotQuarter
    INTO hist_tblTmkAreaDelete FROM tblTmkAreaDelete;
    CREATE INDEX IX_hist_tblTmkAreaDelete ON hist_tblTmkAreaDelete (SnapshotYear, SnapshotQuarter);
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblTmkAuditLog')
   AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'hist_tblTmkAuditLog')
BEGIN
    SELECT TOP 0 *, CAST(0 AS INT) AS SnapshotYear, CAST('  ' AS NVARCHAR(2)) AS SnapshotQuarter
    INTO hist_tblTmkAuditLog FROM tblTmkAuditLog;
    CREATE INDEX IX_hist_tblTmkAuditLog ON hist_tblTmkAuditLog (SnapshotYear, SnapshotQuarter);
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblTmkCaseType')
   AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'hist_tblTmkCaseType')
BEGIN
    SELECT TOP 0 *, CAST(0 AS INT) AS SnapshotYear, CAST('  ' AS NVARCHAR(2)) AS SnapshotQuarter
    INTO hist_tblTmkCaseType FROM tblTmkCaseType;
    CREATE INDEX IX_hist_tblTmkCaseType ON hist_tblTmkCaseType (SnapshotYear, SnapshotQuarter);
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblTmkCountry')
   AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'hist_tblTmkCountry')
BEGIN
    SELECT TOP 0 *, CAST(0 AS INT) AS SnapshotYear, CAST('  ' AS NVARCHAR(2)) AS SnapshotQuarter
    INTO hist_tblTmkCountry FROM tblTmkCountry;
    CREATE INDEX IX_hist_tblTmkCountry ON hist_tblTmkCountry (SnapshotYear, SnapshotQuarter);
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblTmkCountryDue')
   AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'hist_tblTmkCountryDue')
BEGIN
    SELECT TOP 0 *, CAST(0 AS INT) AS SnapshotYear, CAST('  ' AS NVARCHAR(2)) AS SnapshotQuarter
    INTO hist_tblTmkCountryDue FROM tblTmkCountryDue;
    CREATE INDEX IX_hist_tblTmkCountryDue ON hist_tblTmkCountryDue (SnapshotYear, SnapshotQuarter);
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblTmkCountryLaw')
   AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'hist_tblTmkCountryLaw')
BEGIN
    SELECT TOP 0 *, CAST(0 AS INT) AS SnapshotYear, CAST('  ' AS NVARCHAR(2)) AS SnapshotQuarter
    INTO hist_tblTmkCountryLaw FROM tblTmkCountryLaw;
    CREATE INDEX IX_hist_tblTmkCountryLaw ON hist_tblTmkCountryLaw (SnapshotYear, SnapshotQuarter);
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblTmkCountryLawUpdate')
   AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'hist_tblTmkCountryLawUpdate')
BEGIN
    SELECT TOP 0 *, CAST(0 AS INT) AS SnapshotYear, CAST('  ' AS NVARCHAR(2)) AS SnapshotQuarter
    INTO hist_tblTmkCountryLawUpdate FROM tblTmkCountryLawUpdate;
    CREATE INDEX IX_hist_tblTmkCountryLawUpdate ON hist_tblTmkCountryLawUpdate (SnapshotYear, SnapshotQuarter);
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblTmkDesCaseType')
   AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'hist_tblTmkDesCaseType')
BEGIN
    SELECT TOP 0 *, CAST(0 AS INT) AS SnapshotYear, CAST('  ' AS NVARCHAR(2)) AS SnapshotQuarter
    INTO hist_tblTmkDesCaseType FROM tblTmkDesCaseType;
    CREATE INDEX IX_hist_tblTmkDesCaseType ON hist_tblTmkDesCaseType (SnapshotYear, SnapshotQuarter);
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblTmkDesCaseType_Ext')
   AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'hist_tblTmkDesCaseType_Ext')
BEGIN
    SELECT TOP 0 *, CAST(0 AS INT) AS SnapshotYear, CAST('  ' AS NVARCHAR(2)) AS SnapshotQuarter
    INTO hist_tblTmkDesCaseType_Ext FROM tblTmkDesCaseType_Ext;
    CREATE INDEX IX_hist_tblTmkDesCaseType_Ext ON hist_tblTmkDesCaseType_Ext (SnapshotYear, SnapshotQuarter);
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblTmkDesCaseTypeDelete')
   AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'hist_tblTmkDesCaseTypeDelete')
BEGIN
    SELECT TOP 0 *, CAST(0 AS INT) AS SnapshotYear, CAST('  ' AS NVARCHAR(2)) AS SnapshotQuarter
    INTO hist_tblTmkDesCaseTypeDelete FROM tblTmkDesCaseTypeDelete;
    CREATE INDEX IX_hist_tblTmkDesCaseTypeDelete ON hist_tblTmkDesCaseTypeDelete (SnapshotYear, SnapshotQuarter);
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblTmkDesCaseTypeDelete_Ext')
   AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'hist_tblTmkDesCaseTypeDelete_Ext')
BEGIN
    SELECT TOP 0 *, CAST(0 AS INT) AS SnapshotYear, CAST('  ' AS NVARCHAR(2)) AS SnapshotQuarter
    INTO hist_tblTmkDesCaseTypeDelete_Ext FROM tblTmkDesCaseTypeDelete_Ext;
    CREATE INDEX IX_hist_tblTmkDesCaseTypeDelete_Ext ON hist_tblTmkDesCaseTypeDelete_Ext (SnapshotYear, SnapshotQuarter);
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblTmkDesCaseTypeFields')
   AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'hist_tblTmkDesCaseTypeFields')
BEGIN
    SELECT TOP 0 *, CAST(0 AS INT) AS SnapshotYear, CAST('  ' AS NVARCHAR(2)) AS SnapshotQuarter
    INTO hist_tblTmkDesCaseTypeFields FROM tblTmkDesCaseTypeFields;
    CREATE INDEX IX_hist_tblTmkDesCaseTypeFields ON hist_tblTmkDesCaseTypeFields (SnapshotYear, SnapshotQuarter);
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblTmkDesCaseTypeFields_Ext')
   AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'hist_tblTmkDesCaseTypeFields_Ext')
BEGIN
    SELECT TOP 0 *, CAST(0 AS INT) AS SnapshotYear, CAST('  ' AS NVARCHAR(2)) AS SnapshotQuarter
    INTO hist_tblTmkDesCaseTypeFields_Ext FROM tblTmkDesCaseTypeFields_Ext;
    CREATE INDEX IX_hist_tblTmkDesCaseTypeFields_Ext ON hist_tblTmkDesCaseTypeFields_Ext (SnapshotYear, SnapshotQuarter);
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblTmkDesCaseTypeFieldsDelete')
   AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'hist_tblTmkDesCaseTypeFieldsDelete')
BEGIN
    SELECT TOP 0 *, CAST(0 AS INT) AS SnapshotYear, CAST('  ' AS NVARCHAR(2)) AS SnapshotQuarter
    INTO hist_tblTmkDesCaseTypeFieldsDelete FROM tblTmkDesCaseTypeFieldsDelete;
    CREATE INDEX IX_hist_tblTmkDesCaseTypeFieldsDelete ON hist_tblTmkDesCaseTypeFieldsDelete (SnapshotYear, SnapshotQuarter);
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblTmkDesCaseTypeFieldsDelete_Ext')
   AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'hist_tblTmkDesCaseTypeFieldsDelete_Ext')
BEGIN
    SELECT TOP 0 *, CAST(0 AS INT) AS SnapshotYear, CAST('  ' AS NVARCHAR(2)) AS SnapshotQuarter
    INTO hist_tblTmkDesCaseTypeFieldsDelete_Ext FROM tblTmkDesCaseTypeFieldsDelete_Ext;
    CREATE INDEX IX_hist_tblTmkDesCaseTypeFieldsDelete_Ext ON hist_tblTmkDesCaseTypeFieldsDelete_Ext (SnapshotYear, SnapshotQuarter);
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblTmkDesignatedCountry')
   AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'hist_tblTmkDesignatedCountry')
BEGIN
    SELECT TOP 0 *, CAST(0 AS INT) AS SnapshotYear, CAST('  ' AS NVARCHAR(2)) AS SnapshotQuarter
    INTO hist_tblTmkDesignatedCountry FROM tblTmkDesignatedCountry;
    CREATE INDEX IX_hist_tblTmkDesignatedCountry ON hist_tblTmkDesignatedCountry (SnapshotYear, SnapshotQuarter);
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblTmkIndicator')
   AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'hist_tblTmkIndicator')
BEGIN
    SELECT TOP 0 *, CAST(0 AS INT) AS SnapshotYear, CAST('  ' AS NVARCHAR(2)) AS SnapshotQuarter
    INTO hist_tblTmkIndicator FROM tblTmkIndicator;
    CREATE INDEX IX_hist_tblTmkIndicator ON hist_tblTmkIndicator (SnapshotYear, SnapshotQuarter);
END;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblTmkStandardGood')
   AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'hist_tblTmkStandardGood')
BEGIN
    SELECT TOP 0 *, CAST(0 AS INT) AS SnapshotYear, CAST('  ' AS NVARCHAR(2)) AS SnapshotQuarter
    INTO hist_tblTmkStandardGood FROM tblTmkStandardGood;
    CREATE INDEX IX_hist_tblTmkStandardGood ON hist_tblTmkStandardGood (SnapshotYear, SnapshotQuarter);
END;
GO


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

-- Patent audit log table
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblPatAuditLog')
BEGIN
    CREATE TABLE tblPatAuditLog (
        AuditLogId  BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ChangedAt   DATETIME2           NOT NULL,
        ChangedBy   NVARCHAR(100)       NULL,
        Action      CHAR(1)             NOT NULL,  -- I=Insert, U=Update, D=Delete
        TableName   NVARCHAR(100)       NULL,
        RecordId    NVARCHAR(500)       NULL,
        OldValues   NVARCHAR(MAX)       NULL,
        NewValues   NVARCHAR(MAX)       NULL
    );
END;
GO

-- Trademark audit log table
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblTmkAuditLog')
BEGIN
    CREATE TABLE tblTmkAuditLog (
        AuditLogId  BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ChangedAt   DATETIME2           NOT NULL,
        ChangedBy   NVARCHAR(100)       NULL,
        Action      CHAR(1)             NOT NULL,  -- I=Insert, U=Update, D=Delete
        TableName   NVARCHAR(100)       NULL,
        RecordId    NVARCHAR(500)       NULL,
        OldValues   NVARCHAR(MAX)       NULL,
        NewValues   NVARCHAR(MAX)       NULL
    );
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

-- Per-path document selections on tblDeployPassword. Each nullable INT column
-- stores the DocId of the document the user picked from the dropdown next to
-- the corresponding deploy path (see DeployPassword.cs for path-to-tag map).
-- Columns are added in a single ALTER so the table is rewritten at most once.
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'tblDeployPassword' AND COLUMN_NAME = 'PatVer9And10LawDocId'
)
BEGIN
    ALTER TABLE tblDeployPassword
        ADD PatVer9And10LawDocId  INT NULL,
            PatVer9And10MdbId     INT NULL,
            PatR5LawDocId         INT NULL,
            PatR5MdbId            INT NULL,
            PatR8LawDocId         INT NULL,
            PatR8MdbId            INT NULL,
            TmkVer9And10LawDocId  INT NULL,
            TmkVer9And10MdbId     INT NULL,
            TmkR5LawDocId         INT NULL,
            TmkR5MdbId            INT NULL,
            TmkR9LawDocId         INT NULL,
            TmkR9MdbId            INT NULL;
END;
GO

-- Deploy activity log — one row per Populate Tables / Generate Script / Push action.
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblDeployLog'
)
BEGIN
    CREATE TABLE tblDeployLog (
        DeployLogId      INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_tblDeployLog PRIMARY KEY,
        DeployPasswordId INT NOT NULL,
        Action           NVARCHAR(50)  NULL,   -- PopulateTables | GenerateScript | PushMdbs
        Side             NVARCHAR(10)  NULL,   -- Pat | Tmk (PushMdbs only)
        PerformedBy      NVARCHAR(100) NULL,
        PerformedAt      DATETIME      NOT NULL,
        Status           NVARCHAR(20)  NULL,   -- Success | Error
        Detail           NVARCHAR(MAX) NULL
    );
    CREATE INDEX IX_tblDeployLog_DeployPasswordId
        ON tblDeployLog (DeployPasswordId);
END;
GO
