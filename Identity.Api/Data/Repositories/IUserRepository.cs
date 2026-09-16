using Identity.Api.Data.Entities;

namespace Identity.Api.Data.Repositories;

public interface IUserRepository
{
    Task<UserEntity[]> GetUsersAsync(string? search, int? roleId, bool? isActive, CancellationToken ct);
    Task<UserEntity?> GetUserAsync(int id, CancellationToken ct);
    Task<bool> UserNameExistsAsync(string userName, int? exceptUserId, CancellationToken ct);
    Task<bool> EmpCodeExistsAsync(string empCode, int? exceptUserId, CancellationToken ct);
    Task<bool> EmailExistsAsync(string email, int? exceptUserId, CancellationToken ct);
    Task<RoleEntity[]> GetRolesAsync(CancellationToken ct);
    Task<bool> UserHasHistoryAsync(int userId, CancellationToken ct);
    Task AddUserAsync(UserEntity user, CancellationToken ct);
    void RemoveUser(UserEntity user);
    Task AddAuditEventAsync(AuditEventEntity auditEvent, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
