using Application.DTOs.Requests.Polls;
using Application.DTOs.Responses.Polls;
using Application.Services_Interfaces;
using Domain.Common.Abstractions;
using Domain.Common.Abstractions.Errors;
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

    public async Task<Result<IEnumerable<PollResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var polls = await _unitOfWork.Polls.GetAllAsync(cancellationToken);

        if (polls is null)
            return (PollErrors.NotFound);

        var pollresponse = polls.Adapt<IEnumerable<PollResponse>>();


        return Result.Success(pollresponse);
    }

    public async Task<Result<PollResponse>> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var poll = await _unitOfWork.Polls.GetAsync(id, cancellationToken);
        if (poll is null)
            return (PollErrors.NotFound);

        var pollresponse = poll.Adapt<PollResponse>();

        return pollresponse;
    }

    public async Task<Result<PollResponse>> AddAsync(PollRequest request, CancellationToken cancellationToken = default)
    {
        var existingPoll = await _unitOfWork.Polls.ExistsAsync(x => x.Title == request.Title, cancellationToken);
        
        if (existingPoll)
            return (PollErrors.DuplicateTitle);

        var poll = request.Adapt<Poll>();

        await _unitOfWork.Polls.AddAsync(poll, cancellationToken);
        await _unitOfWork.CompleteAsync(cancellationToken);

        var pollResponse = poll.Adapt<PollResponse>();

        return pollResponse;
    }

    public async Task<Result> UpdateAsync(int id, PollRequest request, CancellationToken cancellationToken = default)
    {
        var currentPoll = await _unitOfWork.Polls.GetAsync(id, cancellationToken);
       
        if (currentPoll is null)
            return (PollErrors.NotFound);

        var existingPoll = await _unitOfWork.Polls.ExistsAsync(x => x.Title == request.Title && x.Id != id, cancellationToken);

        if (existingPoll)
            return (PollErrors.DuplicateTitle);


        //currentPoll.Title = request.Title;
        //currentPoll.Summary = request.Summary;
        //currentPoll.StartsAt = request.StartsAt;
        //currentPoll.EndsAt = request.EndsAt;
        request.Adapt(currentPoll);


        await _unitOfWork.Polls.UpdateAsync(currentPoll, cancellationToken);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var poll = await _unitOfWork.Polls.GetAsync(id, cancellationToken);
        if (poll is null)
            return (PollErrors.NotFound);

        await _unitOfWork.Polls.DeleteAsync(poll, cancellationToken);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> TogglePublishStatusAsync(int id, CancellationToken cancellationToken = default)
    {
        var poll = await _unitOfWork.Polls.GetAsync(id, cancellationToken);
        if (poll is null)
            return (PollErrors.NotFound);
        poll.IsPublished = !poll.IsPublished;
        await _unitOfWork.Polls.UpdateAsync(poll, cancellationToken);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}