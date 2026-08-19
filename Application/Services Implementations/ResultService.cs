using Application.DTOs.Responses.Result;
using Application.Services_Interfaces;
using Domain.Common.Abstractions;
using Domain.Common.Abstractions.Errors;
using Domain.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services_Implementations;

public class ResultService(IUnitOfWork unitOfWork) : IResultService
{
    public async Task<Result<PollVotesResponse>> GetPollVotesAsync(int pollId, CancellationToken cancellationToken = default)
    {
        var pollVotes = await unitOfWork.Votes.GetPollVotesAsync(pollId, cancellationToken);

        if (pollVotes is null)
            return PollErrors.NotFound; 

        return pollVotes; 
    }

    public async Task<Result<IEnumerable<VotesPerDayResponse>>> GetVotesPerDayAsync(int pollId, CancellationToken cancellationToken = default)
    {
        var pollExists = await unitOfWork.Polls.ExistsAsync(x => x.Id == pollId, cancellationToken);
        if (!pollExists)
            return PollErrors.NotFound;

        var votesPerDay = await unitOfWork.Votes.GetVotesPerDayAsync(pollId, cancellationToken);

        return Result.Success(votesPerDay);
    }

    public async Task<Result<IEnumerable<VotesPerQuestionResponse>>> GetVotesPerQuestionAsync(int pollId, CancellationToken cancellationToken = default)
    {
        var pollExists = await unitOfWork.Polls.ExistsAsync(x => x.Id == pollId, cancellationToken);
        if (!pollExists)
            return PollErrors.NotFound;

        var votesPerQuestion = await unitOfWork.Votes.GetVotesPerQuestionAsync(pollId, cancellationToken);

        return Result.Success(votesPerQuestion);
    }
}
