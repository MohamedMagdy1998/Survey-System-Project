using Domain.Common.Const;
using Domain.Contracts;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure_Layer.Implementations;

public class RoleRepository : IRoleRepository
{
    private readonly ApplicationDbContext Context;
    private readonly UserManager<ApplicationUser> UserManager;

    public RoleRepository(ApplicationDbContext context,UserManager<ApplicationUser> userManager)
    {
        Context = context;
        UserManager = userManager;
    }
    public async Task<(IEnumerable<string> roles, IEnumerable<string> permissions)> GetUserRolesAndPermissions(ApplicationUser user, CancellationToken cancellationToken)
    {
        var userRoles = await UserManager.GetRolesAsync(user);

        //var userPermissions = await _context.Roles
        //    .Join(_context.RoleClaims,
        //        role => role.Id,
        //        claim => claim.RoleId,
        //        (role, claim) => new { role, claim }
        //    )
        //    .Where(x => userRoles.Contains(x.role.Name!))
        //    .Select(x => x.claim.ClaimValue!)
        //    .Distinct()
        //    .ToListAsync(cancellationToken);

        var userPermissions = await(from r in Context.Roles
                                    join p in Context.RoleClaims
                                    on r.Id equals p.RoleId
                                    where userRoles.Contains(r.Name!)
                                    select p.ClaimValue!)
                                     .Distinct()
                                     .ToListAsync(cancellationToken);

        return (userRoles, userPermissions);
    }

    public async Task<IEnumerable<string>> GetPermissionsByRoleIdAsync(string roleId, CancellationToken cancellationToken = default)
    {
        return await Context.RoleClaims
            .Where(rc => rc.RoleId == roleId && rc.ClaimType == Permissions.Type)
            .Select(rc => rc.ClaimValue!)
            .ToListAsync(cancellationToken);
    }

    public async Task AddPermissionsAsync(string roleId, IEnumerable<string> permissions, CancellationToken cancellationToken = default)
    {
        var roleClaims = permissions.Select(permission => new IdentityRoleClaim<string>
        {
            RoleId = roleId,
            ClaimType = Permissions.Type,
            ClaimValue = permission
        });

        await Context.RoleClaims.AddRangeAsync(roleClaims, cancellationToken);
    }

    public async Task UpdatePermissionsAsync(string roleId, IEnumerable<string> newPermissions, CancellationToken cancellationToken = default)
    {
        var currentPermissions = await Context.RoleClaims
            .Where(rc => rc.RoleId == roleId && rc.ClaimType == Permissions.Type)
            .Select(rc => rc.ClaimValue!)
            .ToListAsync(cancellationToken);

        var permissionsToAdd = newPermissions.Except(currentPermissions)
            .Select(permission => new IdentityRoleClaim<string>
            {
                RoleId = roleId,
                ClaimType = Permissions.Type,
                ClaimValue = permission
            });

        var permissionsToRemove = currentPermissions.Except(newPermissions);

        await Context.RoleClaims
            .Where(rc => rc.RoleId == roleId && rc.ClaimType == Permissions.Type && permissionsToRemove.Contains(rc.ClaimValue!))
            .ExecuteDeleteAsync(cancellationToken);

        await Context.RoleClaims.AddRangeAsync(permissionsToAdd, cancellationToken);
    }

    public async Task<IEnumerable<string>> GetPermissionsForRolesAsync(IEnumerable<string> roles, CancellationToken cancellationToken = default)
    {
        return await (from r in Context.Roles
                      join rc in Context.RoleClaims on r.Id equals rc.RoleId
                      where roles.Contains(r.Name!) && rc.ClaimType == Permissions.Type
                      select rc.ClaimValue!)
                     .Distinct()
                     .ToListAsync(cancellationToken);
    }
}
