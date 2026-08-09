using Application.DTOs.Requests.Polls;
using Application.DTOs.Responses.Polls;
using Application.Services_Interfaces;
using Domain.Contracts;
using Domain.Models;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services_Implementations;

public class PollService(IUnitOfWork unitOfWork) : IPollService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<IEnumerable<PollResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var polls = await _unitOfWork.Polls.GetAllAsync(cancellationToken);
        return polls.Adapt<IEnumerable<PollResponse>>();
    }

    public async Task<PollResponse?> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var poll = await _unitOfWork.Polls.GetAsync(id, cancellationToken);
        return poll?.Adapt<PollResponse>();
    }

    public async Task<PollResponse> AddAsync(PollRequest request, CancellationToken cancellationToken = default)
    {
        var poll = request.Adapt<Poll>();

        await _unitOfWork.Polls.AddAsync(poll, cancellationToken);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return poll.Adapt<PollResponse>();
    }

    public async Task<bool> UpdateAsync(int id, PollRequest request, CancellationToken cancellationToken = default)
    {
        var currentPoll = await _unitOfWork.Polls.GetAsync(id, cancellationToken);
        if (currentPoll is null)
            return false;

        currentPoll.Title = request.Title;
        currentPoll.Summary = request.Summary;
        currentPoll.StartsAt = request.StartsAt;
        currentPoll.EndsAt = request.EndsAt;

        await _unitOfWork.Polls.UpdateAsync(currentPoll, cancellationToken);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var poll = await _unitOfWork.Polls.GetAsync(id, cancellationToken);
        if (poll is null)
            return false;

        await _unitOfWork.Polls.DeleteAsync(poll, cancellationToken);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return true;
    }

    public async Task<bool> TogglePublishStatusAsync(int id, CancellationToken cancellationToken = default)
    {
        var poll = await _unitOfWork.Polls.GetAsync(id, cancellationToken);
        if (poll is null)
            return false;

        poll.IsPublished = !poll.IsPublished;
        await _unitOfWork.Polls.UpdateAsync(poll, cancellationToken);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return true;
    }
}