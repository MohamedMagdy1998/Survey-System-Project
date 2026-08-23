using Application.DTOs.Requests.Questions;
using Application.DTOs.Responses.Answers;
using Application.DTOs.Responses.Questions;
using Application.Services_Interfaces;
using Domain.Common.Abstractions;
using Domain.Common.Abstractions.Errors;
using Domain.Contracts;
using Domain.Models;
using Mapster;

namespace Application.Services_Implementations;

public class QuestionService(IUnitOfWork unitOfWork, ICacheService cacheService) : IQuestionService
{
    private const string CachePrefix = "questions";

    public async Task<Result<QuestionResponse>> AddAsync(int pollId, QuestionRequest request, CancellationToken cancellationToken = default)
    {
        var pollExists = await unitOfWork.Polls.ExistsAsync(x => x.Id == pollId, cancellationToken);
        if (!pollExists)
            return PollErrors.NotFound;

        var questionExists = await unitOfWork.Questions.ExistsAsync(
            q => q.PollId == pollId && q.Content == request.Content,
            cancellationToken);

        if (questionExists)
            return QuestionErrors.DuplicateContent;

        var question = request.Adapt<Question>();
        question.PollId = pollId;

        request.Answers.ForEach(answerContent =>
            question.Answers.Add(new Answer { Content = answerContent }));

        await unitOfWork.Questions.AddAsync(question, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        await cacheService.RemoveAsync($"{CachePrefix}:poll:{pollId}:all", cancellationToken);
        await cacheService.RemoveAsync($"{CachePrefix}:poll:{pollId}:available", cancellationToken);

        var response = question.Adapt<QuestionResponse>();
        return response;
    }

    public async Task<Result<IEnumerable<QuestionResponse>>> GetAllAsync(int pollId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{CachePrefix}:poll:{pollId}:all";

        var cachedQuestions = await cacheService.GetAsync<List<QuestionResponse>>(cacheKey, cancellationToken);
        if (cachedQuestions is not null)
            return Result.Success<IEnumerable<QuestionResponse>>(cachedQuestions);

        var pollExists = await unitOfWork.Polls.ExistsAsync(x => x.Id == pollId, cancellationToken);
        if (!pollExists)
            return PollErrors.NotFound;

        var questions = await unitOfWork.Questions.GetAllAsync(pollId, cancellationToken);
        if (!questions.Any())
            return QuestionErrors.NotFound;

        var response = questions.Adapt<List<QuestionResponse>>();

        await cacheService.SetAsync(cacheKey, response, cancellationToken);

        return Result.Success<IEnumerable<QuestionResponse>>(response);
    }

    public async Task<Result<IEnumerable<QuestionResponse>>> GetAvailableAsync(int pollId, string userId, CancellationToken cancellationToken = default)
    {
        var hasVoted = await unitOfWork.Votes.ExistsAsync(pollId, userId, cancellationToken);
        if (hasVoted)
            return VoteErrors.DuplicateContent;

        var pollExists = await unitOfWork.Polls.ExistsAsync(x => x.Id == pollId
            && x.IsPublished
            && x.StartsAt <= DateOnly.FromDateTime(DateTime.UtcNow)
            && x.EndsAt >= DateOnly.FromDateTime(DateTime.UtcNow),
            cancellationToken);

        if (!pollExists)
            return PollErrors.NotFound;

        var cacheKey = $"{CachePrefix}:poll:{pollId}:available";

        var cachedQuestions = await cacheService.GetAsync<List<QuestionResponse>>(cacheKey, cancellationToken);
        if (cachedQuestions is not null)
            return Result.Success<IEnumerable<QuestionResponse>>(cachedQuestions);

        var questions = await unitOfWork.Questions.GetAvailableAsync(pollId, userId, cancellationToken);
        if (!questions.Any())
            return QuestionErrors.NotFound;

        var activeQuestions = questions.Select(q => new QuestionResponse(
            q.Id,
            q.Content,
            q.Answers.Where(a => a.IsActive).Select(a => new AnswerResponse(a.Id, a.Content))
        )).ToList();

        await cacheService.SetAsync(cacheKey, activeQuestions, cancellationToken);

        return Result.Success<IEnumerable<QuestionResponse>>(activeQuestions);
    }

    public async Task<Result<QuestionResponse>> GetByIdAsync(int pollId, int id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{CachePrefix}:poll:{pollId}:{id}";

        var cachedQuestion = await cacheService.GetAsync<QuestionResponse>(cacheKey, cancellationToken);
        if (cachedQuestion is not null)
            return cachedQuestion;

        var question = await unitOfWork.Questions.GetAsync(pollId, id, cancellationToken);
        if (question is null)
            return QuestionErrors.NotFound;

        var response = question.Adapt<QuestionResponse>();

        await cacheService.SetAsync(cacheKey, response, cancellationToken);

        return response;
    }

    public async Task<Result> UpdateAsync(int pollId, int id, QuestionRequest request, CancellationToken cancellationToken = default)
    {
        var question = await unitOfWork.Questions.GetAsync(pollId, id, cancellationToken);
        if (question is null)
            return QuestionErrors.NotFound;

        var duplicateExists = await unitOfWork.Questions.ExistsAsync(
            q => q.PollId == pollId && q.Content == request.Content && q.Id != id,
            cancellationToken);

        if (duplicateExists)
            return QuestionErrors.DuplicateContent;

        question.Content = request.Content;

        var currentAnswers = question.Answers.Select(a => a.Content).ToList();
        var newAnswers = request.Answers.Except(currentAnswers).ToList();

        newAnswers.ForEach(answerContent =>
            question.Answers.Add(new Answer { Content = answerContent }));

        foreach (var answer in question.Answers)
        {
            answer.IsActive = request.Answers.Contains(answer.Content);
        }

        await unitOfWork.Questions.UpdateAsync(question, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        await InvalidateCacheAsync(pollId, id, cancellationToken);

        return Result.Success();
    }

    public async Task<Result> ToggleStatusAsync(int pollId, int id, CancellationToken cancellationToken = default)
    {
        var result = await unitOfWork.Questions.ToggleStatusAsync(pollId, id, cancellationToken);
        if (result is null)
            return QuestionErrors.NotFound;

        await unitOfWork.CompleteAsync(cancellationToken);

        await InvalidateCacheAsync(pollId, id, cancellationToken);

        return Result.Success();
    }

    private async Task InvalidateCacheAsync(int pollId, int id, CancellationToken cancellationToken)
    {
        await cacheService.RemoveAsync($"{CachePrefix}:poll:{pollId}:{id}", cancellationToken);
        await cacheService.RemoveAsync($"{CachePrefix}:poll:{pollId}:all", cancellationToken);
        await cacheService.RemoveAsync($"{CachePrefix}:poll:{pollId}:available", cancellationToken);
    }
}