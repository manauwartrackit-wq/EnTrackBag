# Session management investigation and fix

## Root cause (verified against BLTSMFT)

The initial database inspection found 30 UserSessions rows, all marked active;
25 already had expired JWT timestamps. The table contained SessionId, UserId,
LoginAt, LogoutAt, RemoteIp, UserAgent, CorrelationId, TokenIssuedAt,
TokenExpiresAt, IsActive and LogoutReason. There was no last-activity column.

- Login inserted a session without closing earlier sessions for that user.
- Angular logout only removed local storage. There was no logout endpoint.
- Both APIs validated JWT signatures/lifetimes but ignored the session_id state.
- There was no inactivity enforcement or abandoned-session cleanup.
- Administration counted IsActive flags from the latest 100 history rows,
  rather than querying all valid sessions.
- The global retry interceptor could retry POST login after transient failures.
- No dashboard, polling or SignalR code was found that directly created sessions.

## Implementation

- Login serializes on the user row within a SQL transaction, revokes previous
  active sessions and saves exactly one new session. A filtered unique index is
  the final database guarantee against duplicates.
- JWT session_id and user identity are checked against BLTSMFT on every
  authenticated request in **both** APIs. The eight-hour JWT lifetime is an
  absolute ceiling, not an eight-hour idle allowance. Clock skew is zero.
- Inactivity of **300 seconds or more** rejects requests immediately, including
  late activity reports. Rejected sessions cannot be revived.
- The Identity API sweeps abandoned sessions every 10 seconds. The status flag
  can lag the deadline by up to that sweep interval when there are no requests;
  API enforcement and the active-count query do not have this grace period.
- Foreground requests explicitly use X-User-Activity. Interaction/navigation
  reports are coalesced to at most about one per second, with a trailing report
  for the final event. Timestamps are assigned by SQL Server, not the browser.
- Background HTTP calls opt out through BACKGROUND_REQUEST. Notification
  refresh, administration session polling and auth status checks opt out.
  There is no automatic token-refresh flow in the existing application.
- SignalR handshakes/keep-alives never touch activity. Existing sockets check
  validity every five seconds and disconnect on revocation/expiry.
- Angular clears storage and redirects after five minutes, on session 401,
  or a logout in another tab. A delayed 401 from an older token cannot clear a
  newer login. Closing the browser is handled by server expiry, not unreliable
  unload requests. A disconnected browser's failed logout is bounded by expiry.
- Administration obtains the count from a separate full-database query. Its
  history retains Active / Logged out / Expired / Revoked statuses, with UTC
  activity/logout timestamps available in the status tooltip. No theme changes.
- Writes and foreground requests are not automatically retried.

Activity reports indicate activity, not proof of human presence: a custom client
holding a valid token can imitate activity, as with other client-driven idle
policies. The server still enforces the deadline against its stored timestamp.

## Explicit schema change and deployment

1. Stop both APIs to prevent old-version logins during the update.
2. Back up the existing database according to the deployment procedure.
3. Run `Database/Update-Session-Lifecycle.sql` manually against BLTSMFT.
4. Deploy/restart both APIs and deploy the Angular build together.
5. Sign in again. Existing sessions with no recent provable activity expire.

The idempotent transactional script adds **LastActivityAt datetime2 NOT NULL**,
backfills it from LoginAt, closes obsolete/duplicate sessions, and adds
**UX_UserSessions_OneActivePerUser** on UserId filtered by IsActive=1. It reuses
the existing LogoutReason/LogoutAt columns. It deletes no history and alters no
MFT operational table. No EF migration, EnsureCreated or Database.Migrate is used.

Applied twice on the local database successfully; all 30 original session rows
were retained. Integration-test sessions are also retained as closed history.
If rolling back the application, keep these additive schema changes and assess
the old application's lack of session enforcement; do not silently drop history.

## Changed files

| Area | Files |
| --- | --- |
| Shared enforcement | Shared/SessionRepository.cs; both API Program.cs and .csproj files |
| Login/session persistence | Identity.Api/Data/Repositories/IdentityRepository.cs; IdentityDomainComponent.cs; UserSessionEntity.cs |
| Logout/activity/status | Identity.Api/Controllers/AuthController.cs |
| Administration | AdministrationController.cs; AdministrationRepository.cs and interface; SessionDomainComponent.cs and interface; AdministrationDtos.cs |
| Persistent connections | EnTrackBag.Api/Hubs/MonitoringHub.cs |
| Browser lifecycle | UI/src/app/services/auth.service.ts; core/auth.interceptor.ts; core/request-activity.ts; core/retry.interceptor.ts |
| Background classification | core/api.service.ts; services/notification.service.ts; administration.service.ts |
| Session display | administration.component.ts and .html |
| SQL and tests | Database/Update-Session-Lifecycle.sql; Tests/Test-SessionLifecycle.ps1; UI/tests/*.cjs |

The working tree already contained the requested permission-code refactor and
SLA width change, on top of two unpublished local commits (administration/journey
configuration and notification/account controls). These dependencies are kept
in the draft branch and must be distinguished from the session-only work during
review. Existing personal admin-seeding SQL and generated cache changes are not
part of the new commit.

## Verification (2026-09-17)

- `dotnet build EnTrackBag.sln -c Release --no-restore`: passed; one existing
  NU1510 warning for Microsoft.Extensions.Identity.Core.
- `npm run build` in UI: passed (required unsandboxed execution on Windows).
- `node --test tests/session-management.test.cjs` in UI: **8 tests passed**.
  These exercise actual TS logic using mocked Angular DI/DOM and a fake clock;
  they are not full-browser end-to-end tests.
- `Tests/Test-SessionLifecycle.ps1 -Credential (Get-Credential)`: **28 assertions
  passed**, including repeat/concurrent login, both APIs, activity exclusion,
  logout, exact 300-second boundary, expiry sweep, count and retained history.
  The idle boundary tests backdate only newly created test-session timestamps.
- `UI/tests/signalr-session.integration.cjs`: **2 checks passed** with the real
  APIs, including 16 seconds of connection/keep-alive time and socket revocation.
  Set TEST_USER/TEST_PASSWORD only in the calling environment, never commit them.
- Login page opened successfully in the local browser. No full browser idle
  end-to-end run was performed; UI timing is covered by the deterministic tests.
- `git diff --check`: passed.

The first SignalR run was interrupted by a separate browser login revoking the
test token. The isolated rerun passed. Live tests replace the supplied user's
session, so use a dedicated test account in shared environments.

An unrelated pre-existing SLA-data error remains: SlaThresholdMinutes has not
been configured in SystemSettings. This session fix does not invent that value.
