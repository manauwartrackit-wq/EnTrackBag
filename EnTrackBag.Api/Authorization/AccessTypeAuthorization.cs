using Microsoft.AspNetCore.Authorization;
namespace EnTrackBag.Api.Authorization;

public class AccessTypeRequirement : IAuthorizationRequirement
{
    public AccessTypeRequirement(string permission, string accessType)
    {
        Permission = permission;
        AccessType = accessType;
    }
    public string Permission
    {
        get;
    }
    public string AccessType
    {
        get;
    }
}
public class AccessTypeAuthorizationHandler : AuthorizationHandler<AccessTypeRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AccessTypeRequirement requirement)
    {
        var expected = requirement.Permission + ":" + requirement.AccessType;
        if (context.User.Claims.Any(c => c.Type == "permission_access" && c.Value.Equals(expected, StringComparison.OrdinalIgnoreCase)))
            context.Succeed(requirement);
        return Task.CompletedTask;
    }
}
public static class AuthorizationPolicyExtensions
{
    public static void AddAccessPolicy(this AuthorizationOptions options, string policyName, string permission, string accessType = "VIEW")
    {
        options.AddPolicy(policyName, p => p.RequireAuthenticatedUser().AddRequirements(new AccessTypeRequirement(permission, accessType)));
    }
}
