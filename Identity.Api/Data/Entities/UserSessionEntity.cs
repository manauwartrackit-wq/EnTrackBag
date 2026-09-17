namespace Identity.Api.Data.Entities;

public class UserSessionEntity
{
    public DateTime LastActivityAt { get; set; }
    public long SessionId
    {
        get; set;
    }
    public int UserId
    {
        get; set;
    }
    public DateTime LoginAt
    {
        get; set;
    }
    public DateTime? LogoutAt
    {
        get; set;
    }
    public string? RemoteIp
    {
        get; set;
    }
    public string? UserAgent
    {
        get; set;
    }
    public string? CorrelationId
    {
        get; set;
    }
    public DateTime? TokenIssuedAt
    {
        get; set;
    }
    public DateTime? TokenExpiresAt
    {
        get; set;
    }
    public bool IsActive
    {
        get; set;
    }
    public string? LogoutReason
    {
        get; set;
    }
    public UserEntity User { get; set; } = null!;
}
