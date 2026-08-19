using Application.DTOs.Responses.Result;
using Domain.Contracts;
using Domain.Models;
using Domain.Models.Result;
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

    public async Task<PollVotesResponse?> GetPollVotesAsync(int pollId, CancellationToken cancellationToken = default)
    {
        return await Context.Polls
            .Where(x => x.Id == pollId)
            .Select(x => new PollVotesResponse(
                x.Title,
                x.Votes.Select(v => new VoteResponse(
                    $"{v.User.FirstName} {v.User.LastName}",
                    v.SubmittedOn,
                    v.VoteAnswers.Select(a => new QuestionAnswerResponse(
                        a.Question.Content,
                        a.Answer.Content
                    ))
                ))
            ))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<VotesPerDayResponse>> GetVotesPerDayAsync(int pollId, CancellationToken cancellationToken = default)
    {
        return await Context.Votes
            .Where(x => x.PollId == pollId)
            .GroupBy(x => new { Date = DateOnly.FromDateTime(x.SubmittedOn) })
            .Select(g => new VotesPerDayResponse(
                g.Key.Date,
                g.Count()
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<VotesPerQuestionResponse>> GetVotesPerQuestionAsync(int pollId, CancellationToken cancellationToken = default)
    {
        return await Context.VoteAnswers
            .Where(x => x.Vote.PollId == pollId)
            .Select(x => new VotesPerQuestionResponse(
                x.Question.Content,
                x.Question.Votes
                    .GroupBy(v => new { AnswerId = v.Answer.Id, AnswerContent = v.Answer.Content })
                    .Select(g => new VotesPerAnswerResponse(
                        g.Key.AnswerContent,
                        g.Count()
                    ))
            ))
            .ToListAsync(cancellationToken);
    }


}
