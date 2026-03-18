-- =============================================================================
-- Add Auxiliary Menu Items for Patent and Trademark areas
-- Run against CLAW database after deploying the new controllers
-- =============================================================================
-- Tables: tblCPiMenuPages (pages/routes), tblCPiMenuItems (menu hierarchy)
-- Menu hierarchy: Top-level Area > Category > Leaf Item
-- MenuComponent.IsValidMenuItem() auto-hides items for missing controllers
-- =============================================================================

BEGIN TRANSACTION;

-- ============================================================
-- Step 1: Find existing top-level Patent and Trademark menu IDs
-- ============================================================
DECLARE @PatentId NVARCHAR(450), @TrademarkId NVARCHAR(450);

SELECT @PatentId = Id FROM tblCPiMenuItems WHERE ParentId = '' AND Title = 'Patent';
SELECT @TrademarkId = Id FROM tblCPiMenuItems WHERE ParentId = '' AND Title = 'Trademark';

IF @PatentId IS NULL OR @TrademarkId IS NULL
BEGIN
    RAISERROR('Could not find Patent or Trademark top-level menu items. Check tblCPiMenuItems.', 16, 1);
    ROLLBACK;
    RETURN;
END

-- ============================================================
-- Step 2: Find or create "Auxiliary" category under each area
-- ============================================================
DECLARE @PatAuxId NVARCHAR(450), @TmkAuxId NVARCHAR(450);

SELECT @PatAuxId = Id FROM tblCPiMenuItems WHERE ParentId = @PatentId AND Title = 'Auxiliary';
IF @PatAuxId IS NULL
BEGIN
    SET @PatAuxId = NEWID();
    INSERT INTO tblCPiMenuItems (Id, ParentId, Title, SortOrder, IsEnabled)
    VALUES (@PatAuxId, @PatentId, 'Auxiliary', 20, 1);
END

SELECT @TmkAuxId = Id FROM tblCPiMenuItems WHERE ParentId = @TrademarkId AND Title = 'Auxiliary';
IF @TmkAuxId IS NULL
BEGIN
    SET @TmkAuxId = NEWID();
    INSERT INTO tblCPiMenuItems (Id, ParentId, Title, SortOrder, IsEnabled)
    VALUES (@TmkAuxId, @TrademarkId, 'Auxiliary', 20, 1);
END

-- ============================================================
-- Step 3: Insert CPiMenuPages (controller routes) if not exists
-- ============================================================
-- Patent pages
IF NOT EXISTS (SELECT 1 FROM tblCPiMenuPages WHERE Controller = 'ActionType' AND RouteOptions = '{"area":"Patent"}')
    INSERT INTO tblCPiMenuPages (Name, Controller, Action, RouteOptions, Policy) VALUES ('Patent Action Type', 'ActionType', 'Index', '{"area":"Patent"}', '*');

IF NOT EXISTS (SELECT 1 FROM tblCPiMenuPages WHERE Controller = 'Area' AND RouteOptions = '{"area":"Patent"}')
    INSERT INTO tblCPiMenuPages (Name, Controller, Action, RouteOptions, Policy) VALUES ('Patent Area', 'Area', 'Index', '{"area":"Patent"}', '*');

IF NOT EXISTS (SELECT 1 FROM tblCPiMenuPages WHERE Controller = 'CountryDue' AND RouteOptions = '{"area":"Patent"}')
    INSERT INTO tblCPiMenuPages (Name, Controller, Action, RouteOptions, Policy) VALUES ('Patent Country Due', 'CountryDue', 'Index', '{"area":"Patent"}', '*');

IF NOT EXISTS (SELECT 1 FROM tblCPiMenuPages WHERE Controller = 'CountryExp' AND RouteOptions = '{"area":"Patent"}')
    INSERT INTO tblCPiMenuPages (Name, Controller, Action, RouteOptions, Policy) VALUES ('Patent Country Expiry', 'CountryExp', 'Index', '{"area":"Patent"}', '*');

IF NOT EXISTS (SELECT 1 FROM tblCPiMenuPages WHERE Controller = 'DesCaseType' AND RouteOptions = '{"area":"Patent"}')
    INSERT INTO tblCPiMenuPages (Name, Controller, Action, RouteOptions, Policy) VALUES ('Patent Des Case Type', 'DesCaseType', 'Index', '{"area":"Patent"}', '*');

IF NOT EXISTS (SELECT 1 FROM tblCPiMenuPages WHERE Controller = 'DesCaseTypeFields' AND RouteOptions = '{"area":"Patent"}')
    INSERT INTO tblCPiMenuPages (Name, Controller, Action, RouteOptions, Policy) VALUES ('Patent Des Case Type Fields', 'DesCaseTypeFields', 'Index', '{"area":"Patent"}', '*');

-- Trademark pages
IF NOT EXISTS (SELECT 1 FROM tblCPiMenuPages WHERE Controller = 'ActionType' AND RouteOptions = '{"area":"Trademark"}')
    INSERT INTO tblCPiMenuPages (Name, Controller, Action, RouteOptions, Policy) VALUES ('Trademark Action Type', 'ActionType', 'Index', '{"area":"Trademark"}', '*');

IF NOT EXISTS (SELECT 1 FROM tblCPiMenuPages WHERE Controller = 'Area' AND RouteOptions = '{"area":"Trademark"}')
    INSERT INTO tblCPiMenuPages (Name, Controller, Action, RouteOptions, Policy) VALUES ('Trademark Area', 'Area', 'Index', '{"area":"Trademark"}', '*');

IF NOT EXISTS (SELECT 1 FROM tblCPiMenuPages WHERE Controller = 'CountryDue' AND RouteOptions = '{"area":"Trademark"}')
    INSERT INTO tblCPiMenuPages (Name, Controller, Action, RouteOptions, Policy) VALUES ('Trademark Country Due', 'CountryDue', 'Index', '{"area":"Trademark"}', '*');

IF NOT EXISTS (SELECT 1 FROM tblCPiMenuPages WHERE Controller = 'DesCaseType' AND RouteOptions = '{"area":"Trademark"}')
    INSERT INTO tblCPiMenuPages (Name, Controller, Action, RouteOptions, Policy) VALUES ('Trademark Des Case Type', 'DesCaseType', 'Index', '{"area":"Trademark"}', '*');

IF NOT EXISTS (SELECT 1 FROM tblCPiMenuPages WHERE Controller = 'DesCaseTypeFields' AND RouteOptions = '{"area":"Trademark"}')
    INSERT INTO tblCPiMenuPages (Name, Controller, Action, RouteOptions, Policy) VALUES ('Trademark Des Case Type Fields', 'DesCaseTypeFields', 'Index', '{"area":"Trademark"}', '*');

IF NOT EXISTS (SELECT 1 FROM tblCPiMenuPages WHERE Controller = 'StandardGood' AND RouteOptions = '{"area":"Trademark"}')
    INSERT INTO tblCPiMenuPages (Name, Controller, Action, RouteOptions, Policy) VALUES ('Trademark Standard Good', 'StandardGood', 'Index', '{"area":"Trademark"}', '*');

-- ============================================================
-- Step 4: Insert leaf menu items under Auxiliary categories
-- ============================================================
DECLARE @PageId INT;

-- Patent: Action Type
SELECT @PageId = Id FROM tblCPiMenuPages WHERE Controller = 'ActionType' AND RouteOptions = '{"area":"Patent"}';
IF NOT EXISTS (SELECT 1 FROM tblCPiMenuItems WHERE ParentId = @PatAuxId AND PageId = @PageId)
    INSERT INTO tblCPiMenuItems (Id, ParentId, Title, PageId, SortOrder, IsEnabled) VALUES (NEWID(), @PatAuxId, 'Action Type', @PageId, 10, 1);

-- Patent: Area
SELECT @PageId = Id FROM tblCPiMenuPages WHERE Controller = 'Area' AND RouteOptions = '{"area":"Patent"}';
IF NOT EXISTS (SELECT 1 FROM tblCPiMenuItems WHERE ParentId = @PatAuxId AND PageId = @PageId)
    INSERT INTO tblCPiMenuItems (Id, ParentId, Title, PageId, SortOrder, IsEnabled) VALUES (NEWID(), @PatAuxId, 'Area', @PageId, 20, 1);

-- Patent: Country Due
SELECT @PageId = Id FROM tblCPiMenuPages WHERE Controller = 'CountryDue' AND RouteOptions = '{"area":"Patent"}';
IF NOT EXISTS (SELECT 1 FROM tblCPiMenuItems WHERE ParentId = @PatAuxId AND PageId = @PageId)
    INSERT INTO tblCPiMenuItems (Id, ParentId, Title, PageId, SortOrder, IsEnabled) VALUES (NEWID(), @PatAuxId, 'Country Due', @PageId, 30, 1);

-- Patent: Country Expiry
SELECT @PageId = Id FROM tblCPiMenuPages WHERE Controller = 'CountryExp' AND RouteOptions = '{"area":"Patent"}';
IF NOT EXISTS (SELECT 1 FROM tblCPiMenuItems WHERE ParentId = @PatAuxId AND PageId = @PageId)
    INSERT INTO tblCPiMenuItems (Id, ParentId, Title, PageId, SortOrder, IsEnabled) VALUES (NEWID(), @PatAuxId, 'Country Expiry', @PageId, 40, 1);

-- Patent: Des Case Type
SELECT @PageId = Id FROM tblCPiMenuPages WHERE Controller = 'DesCaseType' AND RouteOptions = '{"area":"Patent"}';
IF NOT EXISTS (SELECT 1 FROM tblCPiMenuItems WHERE ParentId = @PatAuxId AND PageId = @PageId)
    INSERT INTO tblCPiMenuItems (Id, ParentId, Title, PageId, SortOrder, IsEnabled) VALUES (NEWID(), @PatAuxId, 'Des Case Type', @PageId, 50, 1);

-- Patent: Des Case Type Fields
SELECT @PageId = Id FROM tblCPiMenuPages WHERE Controller = 'DesCaseTypeFields' AND RouteOptions = '{"area":"Patent"}';
IF NOT EXISTS (SELECT 1 FROM tblCPiMenuItems WHERE ParentId = @PatAuxId AND PageId = @PageId)
    INSERT INTO tblCPiMenuItems (Id, ParentId, Title, PageId, SortOrder, IsEnabled) VALUES (NEWID(), @PatAuxId, 'Des Case Type Fields', @PageId, 60, 1);

-- Trademark: Action Type
SELECT @PageId = Id FROM tblCPiMenuPages WHERE Controller = 'ActionType' AND RouteOptions = '{"area":"Trademark"}';
IF NOT EXISTS (SELECT 1 FROM tblCPiMenuItems WHERE ParentId = @TmkAuxId AND PageId = @PageId)
    INSERT INTO tblCPiMenuItems (Id, ParentId, Title, PageId, SortOrder, IsEnabled) VALUES (NEWID(), @TmkAuxId, 'Action Type', @PageId, 10, 1);

-- Trademark: Area
SELECT @PageId = Id FROM tblCPiMenuPages WHERE Controller = 'Area' AND RouteOptions = '{"area":"Trademark"}';
IF NOT EXISTS (SELECT 1 FROM tblCPiMenuItems WHERE ParentId = @TmkAuxId AND PageId = @PageId)
    INSERT INTO tblCPiMenuItems (Id, ParentId, Title, PageId, SortOrder, IsEnabled) VALUES (NEWID(), @TmkAuxId, 'Area', @PageId, 20, 1);

-- Trademark: Country Due
SELECT @PageId = Id FROM tblCPiMenuPages WHERE Controller = 'CountryDue' AND RouteOptions = '{"area":"Trademark"}';
IF NOT EXISTS (SELECT 1 FROM tblCPiMenuItems WHERE ParentId = @TmkAuxId AND PageId = @PageId)
    INSERT INTO tblCPiMenuItems (Id, ParentId, Title, PageId, SortOrder, IsEnabled) VALUES (NEWID(), @TmkAuxId, 'Country Due', @PageId, 30, 1);

-- Trademark: Des Case Type
SELECT @PageId = Id FROM tblCPiMenuPages WHERE Controller = 'DesCaseType' AND RouteOptions = '{"area":"Trademark"}';
IF NOT EXISTS (SELECT 1 FROM tblCPiMenuItems WHERE ParentId = @TmkAuxId AND PageId = @PageId)
    INSERT INTO tblCPiMenuItems (Id, ParentId, Title, PageId, SortOrder, IsEnabled) VALUES (NEWID(), @TmkAuxId, 'Des Case Type', @PageId, 40, 1);

-- Trademark: Des Case Type Fields
SELECT @PageId = Id FROM tblCPiMenuPages WHERE Controller = 'DesCaseTypeFields' AND RouteOptions = '{"area":"Trademark"}';
IF NOT EXISTS (SELECT 1 FROM tblCPiMenuItems WHERE ParentId = @TmkAuxId AND PageId = @PageId)
    INSERT INTO tblCPiMenuItems (Id, ParentId, Title, PageId, SortOrder, IsEnabled) VALUES (NEWID(), @TmkAuxId, 'Des Case Type Fields', @PageId, 50, 1);

-- Trademark: Standard Good
SELECT @PageId = Id FROM tblCPiMenuPages WHERE Controller = 'StandardGood' AND RouteOptions = '{"area":"Trademark"}';
IF NOT EXISTS (SELECT 1 FROM tblCPiMenuItems WHERE ParentId = @TmkAuxId AND PageId = @PageId)
    INSERT INTO tblCPiMenuItems (Id, ParentId, Title, PageId, SortOrder, IsEnabled) VALUES (NEWID(), @TmkAuxId, 'Standard Good', @PageId, 60, 1);

-- ============================================================
-- Step 5: Create tblTmkStandardGood table if not exists
-- ============================================================
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblTmkStandardGood')
BEGIN
    CREATE TABLE tblTmkStandardGood (
        ClassId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Class NVARCHAR(3) NOT NULL,
        ClassType NVARCHAR(40) NOT NULL,
        StandardGoods NVARCHAR(MAX) NULL,
        CreatedBy NVARCHAR(256) NULL,
        DateCreated DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedBy NVARCHAR(256) NULL,
        LastUpdate DATETIME2 NOT NULL DEFAULT GETDATE(),
        tStamp ROWVERSION NULL
    );
    CREATE UNIQUE INDEX IX_TmkStandardGood_Class_ClassType ON tblTmkStandardGood (Class, ClassType);
    PRINT 'Created tblTmkStandardGood table.';
END
ELSE
    PRINT 'tblTmkStandardGood already exists, skipping creation.';

-- ============================================================
-- Step 6: Seed tblTmkStandardGood with original records from SQL_R10v22
-- Restore SQL_R10v22_20260225101141.bak to a temp database, then run:
--
--   INSERT INTO tblTmkStandardGood (Class, ClassType, StandardGoods, CreatedBy, DateCreated, UpdatedBy, LastUpdate)
--   SELECT s.Class, s.ClassType, s.StandardGoods, s.CreatedBy, s.DateCreated, s.UpdatedBy, s.LastUpdate
--   FROM [TempR10v22].dbo.tblTmkStandardGood s
--   WHERE NOT EXISTS (
--       SELECT 1 FROM tblTmkStandardGood t
--       WHERE t.Class = s.Class AND t.ClassType = s.ClassType
--   );
-- ============================================================

COMMIT;

PRINT 'Done. Restart the app to clear menu cache (24hr cache / 20min sliding expiration).';
PRINT 'To seed tblTmkStandardGood: restore SQL_R10v22 .bak and copy data per Step 6 comments.';

-- =============================================================================
-- Allow duplicate CountryLaw entries with different Systems
-- Drops unique index on (Country, CaseType) and redundant Country+CaseType FKs.
-- Overlap check is done in application code (CheckSystemsOverlap in controllers).
-- Child tables already have CountryLawID FKs (FK1 variants) which remain.
-- =============================================================================

-- Patent: drop Country+CaseType FKs that depend on the unique index
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_tblPatCountryDue_tblPatCountryLaw')
    ALTER TABLE tblPatCountryDue DROP CONSTRAINT FK_tblPatCountryDue_tblPatCountryLaw;
GO
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_tblPatCountryExp_tblPatCountryLaw')
    ALTER TABLE tblPatCountryExp DROP CONSTRAINT FK_tblPatCountryExp_tblPatCountryLaw;
GO
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_tblPatDesCaseType_tblPatCountryLaw')
    ALTER TABLE tblPatDesCaseType DROP CONSTRAINT FK_tblPatDesCaseType_tblPatCountryLaw;
GO
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_tblPatCountryLaw' AND object_id = OBJECT_ID('tblPatCountryLaw'))
    DROP INDEX IX_tblPatCountryLaw ON tblPatCountryLaw;
GO

-- Trademark: drop Country+CaseType FKs that depend on the unique index
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_tblTmkCountryDue_tblTmkCountryLaw')
    ALTER TABLE tblTmkCountryDue DROP CONSTRAINT FK_tblTmkCountryDue_tblTmkCountryLaw;
GO
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_tblTmkDesCaseType_tblTmkDesCaseType')
    ALTER TABLE tblTmkDesCaseType DROP CONSTRAINT FK_tblTmkDesCaseType_tblTmkDesCaseType;
GO
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_tblTmkCountryLaw' AND object_id = OBJECT_ID('tblTmkCountryLaw'))
    DROP INDEX IX_tblTmkCountryLaw ON tblTmkCountryLaw;
GO

-- =============================================================================
-- Drop unique indexes on child tables (CountryDue)
-- These prevented copying child records when CountryLaw allows duplicate Country+CaseType.
-- Child records are now uniquely identified by their PK (CDueId) and linked via CountryLawID.
-- =============================================================================
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_tblPatCountryDue' AND object_id = OBJECT_ID('tblPatCountryDue'))
    DROP INDEX IX_tblPatCountryDue ON tblPatCountryDue;
GO
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_tblTmkCountryDue' AND object_id = OBJECT_ID('tblTmkCountryDue'))
    DROP INDEX IX_tblTmkCountryDue ON tblTmkCountryDue;
GO
