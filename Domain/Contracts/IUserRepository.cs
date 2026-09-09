using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Contracts;

public interface IUserRepository
{
    Task<IEnumerable<(ApplicationUser User, IEnumerable<string> Roles)>> GetAllUsersWithRolesAsync(CancellationToken cancellationToken = default);
    Task RemoveUserRolesAsync(string userId, CancellationToken cancellationToken = default);
}