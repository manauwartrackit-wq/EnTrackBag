Run `Refactor-Permission-AccessTypes.sql` against the existing BLTSMFT database before starting the updated APIs. It renames existing permission rows in place, preserves their IDs and grants, and adds BagJourney.Configuration with VIEW/EDIT for Admin. It is safe to run repeatedly. A conflicting old/new code pair stops the transaction for review rather than discarding an ID.

The fresh-install scripts now seed feature codes. For an existing installation, run the refactoring script first; do not rerun the original full installer.

Sign out and sign in after deployment to refresh permission claims in the token and browser storage. Previously issued tokens contain the old codes and will no longer authorize renamed features.

Validation: the script includes duplicate-code and duplicate-grant queries, which should return no rows. Permission IDs 1–11 remain unchanged. No operational MFT tables are changed, and neither API performs schema creation or automatic migration.

Local verification (2026-09-16): .NET Release build and Angular build passed. The migration was run twice inside a rolled-back transaction, then applied successfully. Admin login, users, roles and configuration GET returned HTTP 200; unauthenticated configuration access returned 401; VIEW-only and unrelated grants were rejected with 403 for configuration PUT. The UI responded on port 4200.

Separate existing configuration issue: SLA GET returns 500 because SystemSettings lacks a valid SlaThresholdMinutes value. The business-approved threshold must be supplied before SLA data can be verified. No default operational threshold was invented.
