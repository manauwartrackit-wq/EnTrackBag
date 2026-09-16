using Identity.Api.DTOs;

namespace Identity.Api.DomainComponents;

public interface ISessionDomainComponent
{
    Task<SessionListItemDto[]> GetSessionsAsync(int take, CancellationToken ct);
    Task<AuditEventListItemDto[]> GetAuditEventsAsync(int take, CancellationToken ct);
}

