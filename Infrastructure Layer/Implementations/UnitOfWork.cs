using Domain.Contracts;
using Domain.Contracts.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure_Layer.Implementations;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    private readonly Lazy<IPollRepository> _PollRepository;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        _PollRepository = new Lazy<IPollRepository>(() => new PollRepository(_context));
    }

    public IPollRepository Polls => _PollRepository.Value;

    public async Task<int> CompleteAsync(CancellationToken cancellationToken = default) =>
        await _context.SaveChangesAsync(cancellationToken);

    public void Dispose() =>
        _context.Dispose();
}
