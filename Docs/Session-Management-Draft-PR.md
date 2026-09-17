## Summary

Fix duplicate active sessions and enforce a five-minute idle timeout across
Identity API, operational API, SignalR and Angular. Preserve BLTSMFT and history.

## Root cause

30 rows were flagged active, including 25 with expired JWTs. Login never revoked
older sessions; logout was browser-only; JWT validation ignored database session
state; no expiry worker existed; UI counted flags in a limited history page.

## Changes

- Transactional, concurrency-safe one-session-per-user login.
- Explicit manual SQL update for LastActivityAt and filtered unique index.
- Server validation and expiry, real logout endpoint and foreground activity.
- No activity renewal from polling, SignalR keep-alives or auth status checks.
- Browser idle redirect/401 handling and accurate Administration count/statuses.
- Tests for both APIs, Angular logic and real SignalR connections.

## Verification

- .NET Release build passed (one existing NU1510 warning).
- Angular build passed.
- 28 API/database assertions passed.
- 8 Angular logic tests passed.
- 2 real SignalR lifecycle checks passed.
- SQL update applied twice; original history retained.

See `Docs/Session-Management-Review.md` for exact files, deployment sequence,
test commands, limitations and the unrelated missing SLA setting.

## Review scope / deployment

Draft only: **do not merge automatically**. Run the SQL update manually before
deploying both APIs/UI. No automatic migrations or database creation.

GitHub master predates the two existing local application commits. This branch
also contains those unpublished administration/journey and notification/account
changes, plus the already-requested permission refactor and SLA-width fix.
Review the new session commit separately where useful. Generated caches and the
local personal admin-seeding script are excluded from the new commit.
