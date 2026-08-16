using Application.DTOs.Requests.Questions;
using Application.DTOs.Responses.Questions;
using Application.Services_Interfaces;
using Domain;
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

public class QuestionService : IQuestionService
{
    private readonly IUnitOfWork UnitOfWork;

    public QuestionService(IUnitOfWork unitOfWork)
    {
        UnitOfWork = unitOfWork;
    }

    public async Task<Result<QuestionResponse>> AddAsync(int PollId, QuestionRequest request, CancellationToken cancellationToken = default)
    {
        var pollExists = await UnitOfWork.Polls.ExistsAsync(x => x.Id == PollId, cancellationToken);
       
        if (!pollExists)
            return PollErrors.NotFound;

        var questionExists = await UnitOfWork.Questions.ExistsAsync(
            q => q.PollId == PollId && q.Content == request.Content,
            cancellationToken);

        if (questionExists)
            return QuestionErrors.DuplicateContent;

        var question = request.Adapt<Question>();
        question.PollId = PollId;

        // Populate child answers
        request.Answers.ForEach(answerContent =>
            question.Answers.Add(new Answer { Content = answerContent }));

        await UnitOfWork.Questions.AddAsync(question, cancellationToken);
        await UnitOfWork.CompleteAsync(cancellationToken);

        var response = question.Adapt<QuestionResponse>();

        return response;
    }


    public async Task<Result<IEnumerable<QuestionResponse>>> GetAllAsync(int PollId, CancellationToken cancellationToken = default)
    {
        var pollExists = await UnitOfWork.Polls.ExistsAsync(x => x.Id == PollId, cancellationToken);
        
        if (!pollExists)
            return PollErrors.NotFound;

        var questions = await UnitOfWork.Questions.GetAllAsync(PollId, cancellationToken);

        if(!questions.Any())
            return QuestionErrors.NotFound;

        var response = questions.Adapt<IEnumerable<QuestionResponse>>();

        return Result.Success(response);
    }


    public async Task<Result<QuestionResponse>> GetByIdAsync(int PollId, int Id, CancellationToken cancellationToken=default)
    {
        var question = await UnitOfWork.Questions.GetAsync(PollId, Id, cancellationToken);
        
        if (question is null)
            return QuestionErrors.NotFound;

        var response = question.Adapt<QuestionResponse>();

        return response;
    }


    public async Task<Result> UpdateAsync(int PollId, int Id, QuestionRequest request, CancellationToken cancellationToken = default)
    {
        var question = await UnitOfWork.Questions.GetAsync(PollId, Id, cancellationToken);
      
        if (question is null)
            return QuestionErrors.NotFound;

        var duplicateExists = await UnitOfWork.Questions.ExistsAsync(
            q => q.PollId == PollId && q.Content == request.Content && q.Id != Id,
            cancellationToken);


        if (duplicateExists)
            return QuestionErrors.DuplicateContent;


        // Update the question content
        question.Content = request.Content;
        // Clear existing answers and add new ones
        // Synchronize Answers
        var currentAnswers = question.Answers.Select(a => a.Content).ToList();
        var newAnswers = request.Answers.Except(currentAnswers).ToList();

        newAnswers.ForEach(answerContent =>
        {
            question.Answers.Add(new Answer { Content = answerContent });
        });

        foreach (var answer in question.Answers)
        {
            answer.IsActive = request.Answers.Contains(answer.Content);
        }

        await UnitOfWork.Questions.UpdateAsync(question, cancellationToken);
        await UnitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> ToggleStatusAsync(int PollId, int Id, CancellationToken cancellationToken = default)
    {
        var result = await UnitOfWork.Questions.ToggleStatusAsync(PollId, Id, cancellationToken);
        
        if(result is null)
            return QuestionErrors.NotFound;

        await UnitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();

    }




}
