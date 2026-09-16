using Identity.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Identity.Api.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IdentityDbContext _identityDbContext;

    public UserRepository(IdentityDbContext identityDbContext)
    {
        _identityDbContext = identityDbContext;
    }

    public Task<UserEntity[]> GetUsersAsync(string? search, int? roleId, bool? isActive, CancellationToken ct)
    {
        IQueryable<UserEntity> query = _identityDbContext.Users.AsNoTracking()
            .Include(x => x.UserRoles).ThenInclude(x => x.Role);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.UserName.Contains(term) ||
                (x.EmpCode != null && x.EmpCode.Contains(term)) ||
                (x.FirstName != null && x.FirstName.Contains(term)) ||
                (x.LastName != null && x.LastName.Contains(term)) ||
                (x.DisplayName != null && x.DisplayName.Contains(term)) ||
                (x.Email != null && x.Email.Contains(term)) ||
                (x.Nationality != null && x.Nationality.Contains(term)) ||
                (x.Designation != null && x.Designation.Contains(term)));
        }
        if (roleId.HasValue) query = query.Where(x => x.UserRoles.Any(r => r.RoleId == roleId.Value));
        if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);
        return query.OrderBy(x => x.DisplayName ?? x.UserName).ToArrayAsync(ct);
    }

    public Task<UserEntity?> GetUserAsync(int id, CancellationToken ct) =>
        _identityDbContext.Users.Include(x => x.UserRoles).ThenInclude(x => x.Role).SingleOrDefaultAsync(x => x.Id == id, ct);
    public Task<bool> UserNameExistsAsync(string value, int? exceptId, CancellationToken ct) => ExistsAsync(x => x.UserName == value, exceptId, ct);
    public Task<bool> EmpCodeExistsAsync(string value, int? exceptId, CancellationToken ct) => ExistsAsync(x => x.EmpCode == value, exceptId, ct);
    public Task<bool> EmailExistsAsync(string value, int? exceptId, CancellationToken ct) => ExistsAsync(x => x.Email == value, exceptId, ct);
    private Task<bool> ExistsAsync(System.Linq.Expressions.Expression<Func<UserEntity, bool>> predicate, int? exceptId, CancellationToken ct) =>
        _identityDbContext.Users.Where(predicate).AnyAsync(x => !exceptId.HasValue || x.Id != exceptId.Value, ct);
    public Task<RoleEntity[]> GetRolesAsync(CancellationToken ct) => _identityDbContext.Roles.AsNoTracking().OrderBy(x => x.Name).ToArrayAsync(ct);
    public async Task<bool> UserHasHistoryAsync(int id, CancellationToken ct) =>
        await _identityDbContext.UserSessions.AnyAsync(x => x.UserId == id, ct) ||
        await _identityDbContext.AuditEvents.AnyAsync(x => x.UserId == id, ct) ||
        await _identityDbContext.UserRoles.AnyAsync(x => x.AssignedBy == id, ct) ||
        await _identityDbContext.RolePermissions.AnyAsync(x => x.AssignedBy == id, ct);
    public Task AddUserAsync(UserEntity user, CancellationToken ct) => _identityDbContext.Users.AddAsync(user, ct).AsTask();
    public void RemoveUser(UserEntity user) => _identityDbContext.Users.Remove(user);
    public Task AddAuditEventAsync(AuditEventEntity item, CancellationToken ct) => _identityDbContext.AuditEvents.AddAsync(item, ct).AsTask();
    public Task SaveChangesAsync(CancellationToken ct) => _identityDbContext.SaveChangesAsync(ct);
}
