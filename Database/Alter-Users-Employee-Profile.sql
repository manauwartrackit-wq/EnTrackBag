/*
  EnTrackBag Users employee-profile extension for the existing BLTSMFT database.
  Inspected against dbo.Users, dbo.Roles and dbo.UserRoles before authoring.
  This script is idempotent, preserves all users and PasswordHash values, and never recreates a table.

  Existing legacy rows intentionally remain nullable for employee profile fields because their real
  employee code, name and email are not known. The application requires these values for new/edited users.
*/
USE [BLTSMFT];
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.Users', N'U') IS NULL THROW 51000, 'dbo.Users does not exist.', 1;
    IF OBJECT_ID(N'dbo.Roles', N'U') IS NULL THROW 51000, 'dbo.Roles does not exist.', 1;
    IF OBJECT_ID(N'dbo.UserRoles', N'U') IS NULL THROW 51000, 'dbo.UserRoles does not exist.', 1;

    IF COL_LENGTH('dbo.Users', 'EmpCode') IS NULL ALTER TABLE dbo.Users ADD EmpCode varchar(50) NULL;
    IF COL_LENGTH('dbo.Users', 'FirstName') IS NULL ALTER TABLE dbo.Users ADD FirstName varchar(100) NULL;
    IF COL_LENGTH('dbo.Users', 'LastName') IS NULL ALTER TABLE dbo.Users ADD LastName varchar(100) NULL;
    IF COL_LENGTH('dbo.Users', 'Email') IS NULL ALTER TABLE dbo.Users ADD Email varchar(256) NULL;
    IF COL_LENGTH('dbo.Users', 'PassportNumberEncrypted') IS NULL ALTER TABLE dbo.Users ADD PassportNumberEncrypted varbinary(512) NULL;
    IF COL_LENGTH('dbo.Users', 'PassportLast4') IS NULL ALTER TABLE dbo.Users ADD PassportLast4 varchar(4) NULL;
    IF COL_LENGTH('dbo.Users', 'Nationality') IS NULL ALTER TABLE dbo.Users ADD Nationality varchar(100) NULL;
    IF COL_LENGTH('dbo.Users', 'Designation') IS NULL ALTER TABLE dbo.Users ADD Designation varchar(150) NULL;
    IF COL_LENGTH('dbo.Users', 'LastLogoutAt') IS NULL ALTER TABLE dbo.Users ADD LastLogoutAt datetime2(3) NULL;
    IF COL_LENGTH('dbo.Users', 'FailedLoginCount') IS NULL
        ALTER TABLE dbo.Users ADD FailedLoginCount int NOT NULL CONSTRAINT DF_Users_FailedLoginCount DEFAULT (0);
    IF COL_LENGTH('dbo.Users', 'LockedUntil') IS NULL ALTER TABLE dbo.Users ADD LockedUntil datetime2(3) NULL;
    IF COL_LENGTH('dbo.Users', 'CreatedBy') IS NULL ALTER TABLE dbo.Users ADD CreatedBy int NULL;
    IF COL_LENGTH('dbo.Users', 'UpdatedBy') IS NULL ALTER TABLE dbo.Users ADD UpdatedBy int NULL;

    DECLARE @HasDuplicates bit = 0;
    EXEC sys.sp_executesql N'SELECT @result = CASE WHEN EXISTS (SELECT EmpCode FROM dbo.Users WHERE EmpCode IS NOT NULL GROUP BY EmpCode HAVING COUNT(*) > 1) THEN 1 ELSE 0 END', N'@result bit OUTPUT', @HasDuplicates OUTPUT;
    IF @HasDuplicates = 1 THROW 51001, 'Duplicate EmpCode values must be resolved before creating the unique index.', 1;
    EXEC sys.sp_executesql N'SELECT @result = CASE WHEN EXISTS (SELECT Email FROM dbo.Users WHERE Email IS NOT NULL GROUP BY Email HAVING COUNT(*) > 1) THEN 1 ELSE 0 END', N'@result bit OUTPUT', @HasDuplicates OUTPUT;
    IF @HasDuplicates = 1 THROW 51002, 'Duplicate Email values must be resolved before creating the unique index.', 1;
    IF EXISTS (SELECT UserName FROM dbo.Users GROUP BY UserName HAVING COUNT(*) > 1)
        THROW 51003, 'Duplicate UserName values must be resolved.', 1;

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.Users') AND name='UQ_Users_EmpCode')
        EXEC sys.sp_executesql N'CREATE UNIQUE INDEX UQ_Users_EmpCode ON dbo.Users(EmpCode) WHERE EmpCode IS NOT NULL';
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.Users') AND name='UQ_Users_Email')
        EXEC sys.sp_executesql N'CREATE UNIQUE INDEX UQ_Users_Email ON dbo.Users(Email) WHERE Email IS NOT NULL';

    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id=OBJECT_ID('dbo.Users') AND name='FK_Users_CreatedBy')
        EXEC sys.sp_executesql N'ALTER TABLE dbo.Users WITH CHECK ADD CONSTRAINT FK_Users_CreatedBy FOREIGN KEY(CreatedBy) REFERENCES dbo.Users(Id)';
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id=OBJECT_ID('dbo.Users') AND name='FK_Users_UpdatedBy')
        EXEC sys.sp_executesql N'ALTER TABLE dbo.Users WITH CHECK ADD CONSTRAINT FK_Users_UpdatedBy FOREIGN KEY(UpdatedBy) REFERENCES dbo.Users(Id)';

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

-- Validation (does not expose PasswordHash values)
SELECT c.column_id, c.name, TYPE_NAME(c.user_type_id) AS DataType, c.max_length, c.is_nullable
FROM sys.columns c WHERE c.object_id=OBJECT_ID('dbo.Users') ORDER BY c.column_id;
SELECT i.name, i.is_unique, i.filter_definition FROM sys.indexes i
WHERE i.object_id=OBJECT_ID('dbo.Users') AND i.name IN ('UQ_Users_UserName','UQ_Users_EmpCode','UQ_Users_Email');
SELECT COUNT_BIG(*) AS PreservedUserCount,
       SUM(CASE WHEN PasswordHash IS NULL OR PasswordHash='' THEN 1 ELSE 0 END) AS MissingPasswordHashCount
FROM dbo.Users;
GO

/*
  Rollback guidance (manual and intentionally not automatic):
  1. Back up any new profile/audit values.
  2. Drop FK_Users_CreatedBy and FK_Users_UpdatedBy.
  3. Drop UQ_Users_EmpCode and UQ_Users_Email.
  4. Drop DF_Users_FailedLoginCount (look up its name if it predated this script).
  5. Drop only the thirteen columns added above. Dropping columns permanently loses their data.
*/
