namespace Identity.Api.Data.Entities;

public class UserEntity
{
    public int Id
    {
        get; set;
    }
    public string? EmpCode { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? DisplayName
    {
        get; set;
    }
    public string? Email { get; set; }
    public byte[]? PassportNumberEncrypted { get; set; }
    public string? PassportLast4 { get; set; }
    public string? Nationality { get; set; }
    public string? Designation { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive
    {
        get; set;
    }
    public bool MustChangePassword
    {
        get; set;
    }
    public DateTime? LastLoginAt
    {
        get; set;
    }
    public DateTime? LastLogoutAt { get; set; }
    public int FailedLoginCount { get; set; }
    public DateTime? LockedUntil { get; set; }
    public DateTime CreatedAt
    {
        get; set;
    }
    public DateTime? UpdatedAt
    {
        get; set;
    }
    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }
    public ICollection<UserRoleEntity> UserRoles { get; set; } = new List<UserRoleEntity>();
    public ICollection<UserSessionEntity> Sessions { get; set; } = new List<UserSessionEntity>();
}
