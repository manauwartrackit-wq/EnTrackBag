using Identity.Api.Data.Repositories;
using Identity.Api.DTOs;

namespace Identity.Api.DomainComponents;

public sealed class SessionDomainComponent : ISessionDomainComponent
{
    private readonly IAdministrationRepository _administrationRepository;
    private readonly EnTrackBag.Sessions.SessionRepository _sessions;

    public SessionDomainComponent(IAdministrationRepository administrationRepository, EnTrackBag.Sessions.SessionRepository sessions)
    {
        _administrationRepository = administrationRepository;
        _sessions = sessions;
    }

    public async Task<SessionListItemDto[]> GetSessionsAsync(int take, CancellationToken ct)
    {
        await _sessions.ExpireAsync(ct);
        var now = DateTime.UtcNow;
        return (await _administrationRepository.GetSessionsAsync(ClampTake(take), ct))
            .Select(x => new SessionListItemDto(
                x.SessionId,
                x.UserId,
                x.User.UserName,
                x.User.DisplayName ?? x.User.UserName,
                DateTime.SpecifyKind(x.LoginAt, DateTimeKind.Utc),
                x.LogoutAt.HasValue ? DateTime.SpecifyKind(x.LogoutAt.Value, DateTimeKind.Utc) : null,
                x.RemoteIp,
                x.UserAgent,
                x.TokenExpiresAt.HasValue ? DateTime.SpecifyKind(x.TokenExpiresAt.Value, DateTimeKind.Utc) : null,
                IsLive(x, now),
                DateTime.SpecifyKind(x.LastActivityAt, DateTimeKind.Utc),
                DateTime.SpecifyKind(x.LastActivityAt.AddMinutes(5), DateTimeKind.Utc),
                IsLive(x, now) ? "Active" : x.LogoutReason == "Logged out" ? "Logged out" :
                    x.LogoutReason == "Revoked" || !x.User.IsActive ? "Revoked" : "Expired"))
            .ToArray();
    }

    private static bool IsLive(Identity.Api.Data.Entities.UserSessionEntity x, DateTime now) =>
        x.IsActive && x.LogoutAt == null && x.TokenExpiresAt > now && x.LastActivityAt > now.AddMinutes(-5) && x.User.IsActive;

    public async Task<int> GetActiveSessionCountAsync(CancellationToken ct)
    {
        await _sessions.ExpireAsync(ct);
        return await _administrationRepository.GetActiveSessionCountAsync(ct);
    }

    public async Task<AuditEventListItemDto[]> GetAuditEventsAsync(int take, CancellationToken ct) =>
        (await _administrationRepository.GetAuditEventsAsync(ClampTake(take), ct))
            .Select(x => new AuditEventListItemDto(
                x.Id,
                x.OccurredAt,
                x.UserName,
                x.Action,
                x.EntityType,
                x.EntityId,
                x.Description,
                x.Success,
                x.CorrelationId))
            .ToArray();

    private static int ClampTake(int take) => Math.Clamp(take, 1, 500);
}
