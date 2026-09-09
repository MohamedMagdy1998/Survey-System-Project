using API_Layer.Extentions;
using API_Layer.Filters;
using Application.Common.Contracts;
using Application.DTOs.Requests.Questions;
using Application.Services_Interfaces;
using Domain.Common.Abstractions.Errors;
using Domain.Common.Const;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API_Layer.Controllers;

[Route("api/polls/{pollId}/[controller]")]
[ApiController]
public class QuestionsController : ControllerBase
{
    private readonly IQuestionService QuestionService;

    public QuestionsController(IQuestionService questionService)
    {
        QuestionService = questionService;
    }


    [HttpGet("")]
    [HasPermission(Permissions.GetQuestions)]

    public async Task<IActionResult> GetAllAsync([FromRoute] int pollId, [FromQuery] RequestFilters filters, CancellationToken cancellationToken = default)
    {
        var result = await QuestionService.GetAllAsync(pollId, filters, cancellationToken);
      
                if (result.IsFailure)
                   return result.Problem();

                                return Ok(result.Value);
    }


    [HttpGet("{Id}")]
    [HasPermission(Permissions.GetQuestions)]

    public async Task<IActionResult> Get(int pollId, int Id, CancellationToken cancellationToken = default)
    {
        var result = await QuestionService.GetByIdAsync(pollId, Id, cancellationToken);
        if (result.IsFailure)
           return result.Problem();
        return Ok(result.Value);
    }


    [HttpPost]
    [HasPermission(Permissions.AddQuestions)]

    public async Task<IActionResult> Add([FromRoute] int pollId, [FromBody] QuestionRequest request, CancellationToken cancellationToken = default)
    {
        var result = await QuestionService.AddAsync(pollId, request, cancellationToken);

        if (result.IsFailure)
            return result.Problem();

        return CreatedAtAction(nameof(Get), new { pollId = pollId, Id = result.Value.Id }, result.Value);
    }

    [HttpPut("")]
    [HasPermission(Permissions.UpdateQuestions)]

    public async Task<IActionResult> Update([FromRoute] int pollId, [FromRoute] int Id, [FromBody] QuestionRequest request, CancellationToken cancellationToken = default)
    {
        var result = await QuestionService.UpdateAsync(pollId, Id, request, cancellationToken);
        if (result.IsFailure)
            return result.Problem();
        return NoContent();
    }

    [HttpPatch("{Id}/toggleStatus")]
    [HasPermission(Permissions.UpdateQuestions)]

    public async Task<IActionResult> ToggleStatus([FromRoute] int pollId, [FromRoute] int Id, CancellationToken cancellationToken = default)
    {
        var result = await QuestionService.ToggleStatusAsync(pollId, Id, cancellationToken);
        if (result.IsFailure)
            return result.Problem();
        return NoContent();
    }

}
