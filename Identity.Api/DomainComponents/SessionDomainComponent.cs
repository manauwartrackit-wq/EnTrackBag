using Identity.Api.Data.Repositories;
using Identity.Api.DTOs;

namespace Identity.Api.DomainComponents;

public sealed class SessionDomainComponent : ISessionDomainComponent
{
    private readonly IAdministrationRepository _administrationRepository;

    public SessionDomainComponent(IAdministrationRepository administrationRepository)
    {
        _administrationRepository = administrationRepository;
    }

    public async Task<SessionListItemDto[]> GetSessionsAsync(int take, CancellationToken ct) =>
        (await _administrationRepository.GetSessionsAsync(ClampTake(take), ct))
            .Select(x => new SessionListItemDto(
                x.SessionId,
                x.UserId,
                x.User.UserName,
                x.User.DisplayName ?? x.User.UserName,
                x.LoginAt,
                x.LogoutAt,
                x.RemoteIp,
                x.UserAgent,
                x.TokenExpiresAt,
                x.IsActive))
            .ToArray();

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
