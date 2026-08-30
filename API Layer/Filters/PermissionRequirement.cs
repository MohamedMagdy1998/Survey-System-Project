using Microsoft.AspNetCore.Authorization;

namespace API_Layer.Filters;

public class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
