-- Run manually against the existing BLTSMFT database before deploying the APIs.
-- Adds only LastActivityAt (UTC) and a filtered uniqueness constraint.
-- Existing session/audit history and all MFT operational tables are preserved.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF OBJECT_ID('dbo.UserSessions', 'U') IS NULL
    THROW 50001, 'Existing dbo.UserSessions is required.', 1;
IF COL_LENGTH('dbo.UserSessions', 'LastActivityAt') IS NULL
    ALTER TABLE dbo.UserSessions ADD LastActivityAt datetime2 NULL;
-- Dynamic SQL permits adding and using the column in one transactional batch.
EXEC sys.sp_executesql N'
UPDATE dbo.UserSessions SET LastActivityAt = LoginAt WHERE LastActivityAt IS NULL;
UPDATE dbo.UserSessions SET IsActive=0, LogoutAt=COALESCE(LogoutAt,SYSUTCDATETIME()),
    LogoutReason=''Expired''
WHERE IsActive=1 AND (LogoutAt IS NOT NULL OR TokenExpiresAt IS NULL
    OR TokenExpiresAt<=SYSUTCDATETIME() OR LastActivityAt<=DATEADD(minute,-5,SYSUTCDATETIME()));
;WITH duplicates AS (
    SELECT *, ROW_NUMBER() OVER(PARTITION BY UserId ORDER BY LoginAt DESC,SessionId DESC) AS rn
    FROM dbo.UserSessions WHERE IsActive=1
)
UPDATE duplicates SET IsActive=0, LogoutAt=SYSUTCDATETIME(), LogoutReason=''Revoked'' WHERE rn>1;
ALTER TABLE dbo.UserSessions ALTER COLUMN LastActivityAt datetime2 NOT NULL;
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(''dbo.UserSessions'') AND name=''UX_UserSessions_OneActivePerUser'')
    CREATE UNIQUE INDEX UX_UserSessions_OneActivePerUser ON dbo.UserSessions(UserId) WHERE IsActive=1;
';
COMMIT;
