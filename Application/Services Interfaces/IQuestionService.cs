using Application.DTOs.Requests.Questions;
using Application.DTOs.Responses.Questions;
using Domain.Common.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services_Interfaces;

public interface IQuestionService
{
    public Task<Result<QuestionResponse>> AddAsync(int PollId, QuestionRequest request, CancellationToken cancellationToken = default);


    public Task<Result<IEnumerable<QuestionResponse>>> GetAllAsync(int PollId, CancellationToken cancellationToken = default);

    public Task<Result<IEnumerable<QuestionResponse>>> GetAvailableAsync(int PollId, string UserId, CancellationToken cancellationToken = default);


    public Task<Result<QuestionResponse>> GetByIdAsync(int PollId, int Id, CancellationToken cancellationToken = default);


    public Task<Result> UpdateAsync(int PollId, int Id, QuestionRequest request, CancellationToken cancellationToken = default);


    public Task<Result> ToggleStatusAsync(int PollId, int Id, CancellationToken cancellationToken = default);


}


