using Domain.Models;
using Infrastructure_Layer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Contracts.Repositories;



public class PollRepository(ApplicationDbContext context) : IPollRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task<IEnumerable<Poll>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _context.Polls.AsNoTracking().ToListAsync(cancellationToken);

    public async Task<Poll?> GetAsync(int id, CancellationToken cancellationToken = default) =>
        await _context.Polls.FindAsync(id, cancellationToken);

    public async Task<Poll> AddAsync(Poll poll, CancellationToken cancellationToken = default)
    {
        await _context.Polls.AddAsync(poll, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return poll;
    }

    public async Task UpdateAsync(Poll poll, CancellationToken cancellationToken = default)
    {
        _context.Polls.Update(poll);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Poll poll, CancellationToken cancellationToken = default)
    {
        _context.Polls.Remove(poll);
        await _context.SaveChangesAsync(cancellationToken);
    }


    public async Task<bool> ExistsAsync(Expression<Func<Poll, bool>> predicate, CancellationToken cancellationToken = default) =>
        await _context.Polls.AnyAsync(predicate, cancellationToken);

}