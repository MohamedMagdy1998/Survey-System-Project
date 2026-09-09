using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Contracts;

public interface IRoleRepository
{
    public Task<(IEnumerable<string> roles, IEnumerable<string> permissions)> GetUserRolesAndPermissions(ApplicationUser user, CancellationToken cancellationToken);
   
    Task<IEnumerable<string>> GetPermissionsByRoleIdAsync(string roleId, CancellationToken cancellationToken = default);
    Task AddPermissionsAsync(string roleId, IEnumerable<string> permissions, CancellationToken cancellationToken = default);
    Task UpdatePermissionsAsync(string roleId, IEnumerable<string> newPermissions, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetPermissionsForRolesAsync(IEnumerable<string> roles, CancellationToken cancellationToken = default);
}
