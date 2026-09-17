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

    public async Task AddSessionAsync(UserSessionEntity session, CancellationToken ct)
    {
        await using var transaction = await _identityDbContext.Database.BeginTransactionAsync(ct);
        // Serialize concurrent logins for this user before revoking/inserting.
        await _identityDbContext.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT Id FROM dbo.Users WITH (UPDLOCK, HOLDLOCK) WHERE Id={session.UserId}", ct);
        await _identityDbContext.UserSessions.Where(x => x.UserId == session.UserId && x.IsActive)
            .ExecuteUpdateAsync(set => set.SetProperty(x => x.IsActive, false)
                .SetProperty(x => x.LogoutAt, session.LoginAt)
                .SetProperty(x => x.LogoutReason, "Revoked"), ct);
        await _identityDbContext.UserSessions.AddAsync(session, ct);
        await _identityDbContext.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
    }
    public async Task AddAuditEventAsync(AuditEventEntity auditEvent, CancellationToken ct) => await _identityDbContext.AuditEvents.AddAsync(auditEvent, ct);
    public Task SaveChangesAsync(CancellationToken ct) => _identityDbContext.SaveChangesAsync(ct);
}
