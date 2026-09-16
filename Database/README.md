# EnTrackBag Database Installation

The application uses the existing `BLTSMFT` SQL Server database. The install script creates only EnTrackBag-owned Identity/authorization/session/audit/exception tables; it does not recreate or modify operational baggage tables.

## EnTrackBag-owned tables
1. `Roles`
2. `Permissions`
3. `AccessTypes`
4. `Users`
5. `UserRoles`
6. `RolePermissions`
7. `UserSessions`
8. `AuditEvents`
9. `EnTrackBagExceptions`

## Authorization model
`Permissions` identify a page/function. `AccessTypes` identify the operation (`VIEW`, `CREATE`, `EDIT`, `DELETE`, `EXPORT`). `RolePermissions` joins Role + Permission + AccessType.

Role matrix:
- Ground Floor: Summary Dashboard VIEW only.
- Supervisor: Summary Dashboard VIEW + SLA Dashboard VIEW.
- Site Manager: all operational pages VIEW except Administration.
- Admin: all pages VIEW including Administration; Users VIEW/CREATE/EDIT/DELETE; Roles VIEW/EDIT; Sessions and Audit Log VIEW.

After installation, scaffold the tables with EF Core Database-First and verify the generated types against the live database. No migrations, `EnsureCreated()`, or `Database.Migrate()` are used.

The separately supplied `EnTrackBag-Identity-Exception-AccessTypes-Final-INT.sql` matches these application-owned tables and also inserts the initial Admin account. Its seeded password payload uses the confirmed legacy AES layout. Identity.Api accepts that format only as a compatibility bridge and replaces it with ASP.NET Core `PasswordHasher` format immediately after a successful login. New and reset passwords are never reversibly encrypted.
