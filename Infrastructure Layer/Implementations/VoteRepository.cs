using Domain.Contracts;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure_Layer.Implementations;

public class VoteRepository : IVoteRepository
{
    private readonly ApplicationDbContext Context;

    public VoteRepository(ApplicationDbContext context)
    {
        Context = context;
    }

    public Task<bool> ExistsAsync(int pollId, string userId, CancellationToken cancellationToken = default)
    {
        return Context.Votes.AnyAsync(v => v.PollId == pollId && v.UserId == userId, cancellationToken);

    }

    public async Task<bool> HasUserVotedAsync(int pollId, string userId, CancellationToken cancellationToken = default)
    {
        return await Context.Votes
            .AnyAsync(v => v.PollId == pollId && v.UserId == userId, cancellationToken);
    }

    public async Task AddAsync(Vote vote, CancellationToken cancellationToken = default)
    {
        await Context.Votes.AddAsync(vote, cancellationToken);
    }


}
