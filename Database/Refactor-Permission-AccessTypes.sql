USE [BLTSMFT];
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @Codes TABLE (OldCode varchar(150), NewCode varchar(150));
INSERT @Codes VALUES
('Dashboard.View','Dashboard'),('Dashboard.SLA.View','Dashboard.SLA'),
('DeviceStatus.View','DeviceStatus'),('TagReport.View','TagReport'),
('BagJourney.View','BagJourney'),('Administration.View','Administration'),
('Users.Manage','Users'),('Roles.Manage','Roles'),
('Sessions.Manage','Sessions'),('AuditLog.View','AuditLog');
-- Stop rather than discard either ID or silently merge grants when both codes exist.
IF EXISTS (SELECT 1 FROM @Codes m JOIN dbo.Permissions p ON p.Code=m.OldCode
    JOIN dbo.Permissions n ON n.Code=m.NewCode)
    THROW 51000, 'Both legacy and feature codes exist. Resolve conflicting IDs before retrying.', 1;
UPDATE p SET Code=m.NewCode FROM dbo.Permissions p JOIN @Codes m ON p.Code=m.OldCode;
IF NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE Code='BagJourney.Configuration')
    INSERT dbo.Permissions(Code,Name,Description)
    VALUES('BagJourney.Configuration','Bag Journey Configuration','Configure journey routing and thresholds.');
-- Preserve the previous Admin-only configuration entitlement.
INSERT dbo.RolePermissions(RoleId,PermissionId,AccessTypeId)
SELECT r.Id,p.Id,a.Id FROM dbo.Roles r CROSS JOIN dbo.Permissions p CROSS JOIN dbo.AccessTypes a
WHERE r.Name='Admin' AND p.Code='BagJourney.Configuration' AND a.Code IN ('VIEW','EDIT')
AND NOT EXISTS (SELECT 1 FROM dbo.RolePermissions x WHERE x.RoleId=r.Id AND x.PermissionId=p.Id AND x.AccessTypeId=a.Id);
IF EXISTS (SELECT Code FROM dbo.Permissions GROUP BY Code HAVING COUNT(*)>1)
    THROW 51001, 'Duplicate permission codes detected.', 1;
IF EXISTS (SELECT RoleId,PermissionId,AccessTypeId FROM dbo.RolePermissions GROUP BY RoleId,PermissionId,AccessTypeId HAVING COUNT(*)>1)
    THROW 51002, 'Duplicate role permission combinations detected.', 1;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.Permissions') AND name='UX_Permissions_FeatureCode')
    CREATE UNIQUE INDEX UX_Permissions_FeatureCode ON dbo.Permissions(Code);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.RolePermissions') AND name='UX_RolePermissions_FeatureAccess')
    CREATE UNIQUE INDEX UX_RolePermissions_FeatureAccess ON dbo.RolePermissions(RoleId,PermissionId,AccessTypeId);
COMMIT;
-- Verification: both duplicate queries must return no rows.
SELECT Id,Code,Name FROM dbo.Permissions ORDER BY Id;
SELECT Code,COUNT(*) AS DuplicateCount FROM dbo.Permissions GROUP BY Code HAVING COUNT(*)>1;
SELECT RoleId,PermissionId,AccessTypeId,COUNT(*) AS DuplicateCount FROM dbo.RolePermissions GROUP BY RoleId,PermissionId,AccessTypeId HAVING COUNT(*)>1;
