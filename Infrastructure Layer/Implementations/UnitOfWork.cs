using Domain.Contracts;
using Domain.Contracts.Repositories;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure_Layer.Implementations;

public class UnitOfWork : IUnitOfWork
{
    public readonly ApplicationDbContext _context;

    private readonly Lazy<IPollRepository> _PollRepository;

    private readonly Lazy<IQuestionRespository> _QuestionRepository;

    private readonly Lazy<IVoteRepository> _VoteRepository;

    private readonly Lazy<IRoleRepository> _RoleRepository;

    private readonly UserManager<ApplicationUser> _userManager= default!;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        _PollRepository = new Lazy<IPollRepository>(() => new PollRepository(_context));
        _QuestionRepository = new Lazy<IQuestionRespository>(() => new QuestionRepository(_context));
        _VoteRepository = new Lazy<IVoteRepository>(() => new VoteRepository(_context));
        _RoleRepository = new Lazy<IRoleRepository>(() => new RoleRepository(_context, _userManager!));

    }

    public IPollRepository Polls => _PollRepository.Value;

    public IVoteRepository Votes => _VoteRepository.Value;
    public IQuestionRespository Questions => _QuestionRepository.Value;

    public IRoleRepository Roles => _RoleRepository.Value;

    public async Task<int> CompleteAsync(CancellationToken cancellationToken = default) =>
        await _context.SaveChangesAsync(cancellationToken);

    public void Dispose() => _context.Dispose();
}
