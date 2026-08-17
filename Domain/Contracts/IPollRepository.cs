using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Contracts;

public interface IPollRepository
{
    Task<IEnumerable<Poll>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Poll>> GetCurrentAsync(CancellationToken cancellationToken = default);
    Task<Poll?> GetAsync(int id, CancellationToken cancellationToken = default);
    Task<Poll> AddAsync(Poll poll, CancellationToken cancellationToken = default);
    Task UpdateAsync(Poll poll, CancellationToken cancellationToken = default);
    Task DeleteAsync(Poll poll, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Expression<Func<Poll, bool>> predicate, CancellationToken cancellationToken = default);
}
