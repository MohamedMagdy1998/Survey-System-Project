using Application.DTOs.Responses.Result;
using Domain.Common.Abstractions;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Contracts;

public interface IVoteRepository
{
    Task<PollVotesResponse?> GetPollVotesAsync(int pollId, CancellationToken cancellationToken = default);
    Task<IEnumerable<VotesPerDayResponse>> GetVotesPerDayAsync(int pollId, CancellationToken cancellationToken = default);
    Task<IEnumerable<VotesPerQuestionResponse>> GetVotesPerQuestionAsync(int pollId, CancellationToken cancellationToken = default);

    public Task<bool> ExistsAsync(int pollId, string userId, CancellationToken cancellationToken = default);

    Task<bool> HasUserVotedAsync(int pollId, string userId, CancellationToken cancellationToken = default);
    Task AddAsync(Vote vote, CancellationToken cancellationToken = default);

}
