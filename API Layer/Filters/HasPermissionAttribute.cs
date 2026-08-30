using Microsoft.AspNetCore.Authorization;

namespace API_Layer.Filters;

public class HasPermissionAttribute(string permission) : AuthorizeAttribute(permission)
{
}
