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
}

