using Domain.Common.Const;
using Domain.Contracts;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure_Layer.Implementations;

public class UserRepository(ApplicationDbContext context) : IUserRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task<IEnumerable<(ApplicationUser User, IEnumerable<string> Roles)>> GetAllUsersWithRolesAsync(
        CancellationToken cancellationToken = default)
    {
        var rawData = await (from u in _context.Users
                             join ur in _context.UserRoles on u.Id equals ur.UserId into userRolesGroup
                             from ur in userRolesGroup.DefaultIfEmpty()
                             join r in _context.Roles on ur.RoleId equals r.Id into rolesGroup
                             from r in rolesGroup.DefaultIfEmpty()
                             where r == null || r.Name != DefaultRoles.Member
                             select new
                             {
                                 User = u,
                                 RoleName = r != null ? r.Name : null
                             })
                            .ToListAsync(cancellationToken);

        var result = rawData
            .GroupBy(x => x.User)
            .Select(g => (
                User: g.Key,
                Roles: g.Where(x => x.RoleName != null).Select(x => x.RoleName!).Distinct()
            ))
            .ToList();

        return result;
    }

    public async Task RemoveUserRolesAsync(string userId, CancellationToken cancellationToken = default)
    {
        await _context.UserRoles
            .Where(x => x.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);
    }
}