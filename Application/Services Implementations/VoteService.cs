using Application.DTOs.Requests.Votes;
using Application.Services_Interfaces;
using Domain.Common.Abstractions;
using Domain.Common.Abstractions.Errors;
using Domain.Contracts;
using Domain.Models;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services_Implementations;

public class VoteService : IVoteService
{
    private readonly IUnitOfWork UnitOfWork;

    public VoteService(IUnitOfWork unitOfWork)
    {
        UnitOfWork = unitOfWork;
    }

    public async Task<Result> AddAsync(int pollId, string userId, VoteRequest request, CancellationToken cancellationToken = default)
    {
        var hasVote = await UnitOfWork.Votes.HasUserVotedAsync(pollId , userId , cancellationToken);

        if (hasVote)
            return VoteErrors.DuplicateContent;

        var pollIsExists = await UnitOfWork.Polls.ExistsAsync(x => x.Id == pollId && x.IsPublished && x.StartsAt <= DateOnly.FromDateTime(DateTime.UtcNow) && x.EndsAt >= DateOnly.FromDateTime(DateTime.UtcNow), cancellationToken);

        if (!pollIsExists)
            return PollErrors.NotFound;

        var activeQuestionIds = await UnitOfWork.Questions.GetActiveQuestionIdsAsync(pollId, cancellationToken);

        var submittedQuestionIds = request.Answers.Select(a => a.QuestionId);

        if (!submittedQuestionIds.SequenceEqual(activeQuestionIds))
            return VoteErrors.NotFound;

        var vote = new Vote
        {
            PollId = pollId,
            UserId = userId,
            VoteAnswers = request.Answers.Adapt<IEnumerable<VoteAnswer>>().ToList()
        };

        await UnitOfWork.Votes.AddAsync(vote, cancellationToken);
        await UnitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }

}



