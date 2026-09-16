using Identity.Api.DTOs;

namespace Identity.Api.DomainComponents;

public interface IUserDomainComponent
{
    Task<UserListItemDto[]> GetUsersAsync(string? search, int? roleId, bool? isActive, CancellationToken ct);
    Task<PassportDetailDto?> GetPassportAsync(int id, int actorUserId, string actorUserName, CancellationToken ct);
    Task<UserListItemDto> CreateUserAsync(CreateUserRequestDto request, int actorUserId, string actorUserName, CancellationToken ct);
    Task<UserListItemDto?> UpdateUserAsync(int id, UpdateUserRequestDto request, int actorUserId, string actorUserName, CancellationToken ct);
    Task<bool> ResetPasswordAsync(int id, ResetUserPasswordRequestDto request, int actorUserId, string actorUserName, CancellationToken ct);
    Task<bool> RemoveUserAsync(int id, int actorUserId, string actorUserName, CancellationToken ct);
}
