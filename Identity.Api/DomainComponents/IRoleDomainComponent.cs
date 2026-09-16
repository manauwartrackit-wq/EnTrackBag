using Identity.Api.DTOs;

namespace Identity.Api.DomainComponents;

public interface IRoleDomainComponent
{
    Task<RoleListItemDto[]> GetRolesAsync(CancellationToken ct);
    Task<PermissionOptionDto[]> GetPermissionsAsync(CancellationToken ct);
    Task<AccessTypeOptionDto[]> GetAccessTypesAsync(CancellationToken ct);
    Task<RoleListItemDto?> UpdatePermissionsAsync(int roleId, UpdateRolePermissionsRequestDto request, int actorUserId, string actorUserName, CancellationToken ct);
}

