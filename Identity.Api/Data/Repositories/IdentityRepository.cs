using Identity.Api.Data.Entities;
using Identity.Api.DTOs;
using Microsoft.EntityFrameworkCore;
namespace Identity.Api.Data.Repositories;

public class IdentityRepository : IIdentityRepository
{
    private readonly IdentityDbContext _identityDbContext;

    public IdentityRepository(IdentityDbContext identityDbContext)
    {
        _identityDbContext = identityDbContext;
    }

    //public Task<UserEntity?> GetUserForLoginAsync(string userName, CancellationToken ct) =>
    //    _identityDbContext.Users.Include(x => x.UserRoles).ThenInclude(x => x.Role).SingleOrDefaultAsync(x => x.UserName == userName, ct);

    public async Task<UserEntity?> GetUserForLoginAsync(string userName, CancellationToken ct)
    {
        return await _identityDbContext.Users
            .Include(u => u.UserRoles)      // Force-loads the user-role mapping bridge
                .ThenInclude(ur => ur.Role) // Force-loads the actual Role name metadata
            .SingleOrDefaultAsync(u => u.UserName == userName, ct);
    }


    public Task<PermissionAccessDto[]> GetPermissionAccessAsync(int userId, CancellationToken ct) =>
        _identityDbContext.UserRoles.Where(x => x.UserId == userId)
        .SelectMany(x => x.Role.RolePermissions)
        .Where(x => x.Permission.IsActive && x.AccessType.IsActive)
        .Select(x => new PermissionAccessDto(x.Permission.Code, x.AccessType.Code))
        .Distinct()
        .ToArrayAsync(ct);

    public async Task AddSessionAsync(UserSessionEntity session, CancellationToken ct) => await _identityDbContext.UserSessions.AddAsync(session, ct);
    public async Task AddAuditEventAsync(AuditEventEntity auditEvent, CancellationToken ct) => await _identityDbContext.AuditEvents.AddAsync(auditEvent, ct);
    public Task SaveChangesAsync(CancellationToken ct) => _identityDbContext.SaveChangesAsync(ct);
}
