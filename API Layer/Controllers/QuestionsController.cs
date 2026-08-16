using API_Layer.Extentions;
using Application.DTOs.Requests.Questions;
using Application.Services_Interfaces;
using Domain.Common.Abstractions.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API_Layer.Controllers;

[Route("api/polls/{pollId}/[controller]")]
[ApiController]
[Authorize]
public class QuestionsController : ControllerBase
{
    private readonly IQuestionService QuestionService;

    public QuestionsController(IQuestionService questionService)
    {
        QuestionService = questionService;
    }


    [HttpGet("")]
    public async Task<IActionResult> GetAllAsync([FromRoute] int pollId, CancellationToken cancellationToken = default)
    {
        var result = await QuestionService.GetAllAsync(pollId, cancellationToken);
      
                if (result.IsFailure)
                   return result.Problem();

                                return Ok(result.Value);
       
    }


    [HttpGet("{Id}")]
    public async Task<IActionResult> Get(int pollId, int Id, CancellationToken cancellationToken = default)
    {
        var result = await QuestionService.GetByIdAsync(pollId, Id, cancellationToken);
        if (result.IsFailure)
           return result.Problem();
        return Ok(result.Value);
    }


    [HttpPost]

    public async Task<IActionResult> Add([FromRoute] int pollId, [FromBody] QuestionRequest request, CancellationToken cancellationToken = default)
    {
        var result = await QuestionService.AddAsync(pollId, request, cancellationToken);

        if (result.IsFailure)
            return result.Problem();

        return CreatedAtAction(nameof(Get), new { pollId = pollId, Id = result.Value.Id }, result.Value);
    }

    [HttpPut("")]
    public async Task<IActionResult> Update([FromRoute] int pollId, [FromRoute] int Id, [FromBody] QuestionRequest request, CancellationToken cancellationToken = default)
    {
        var result = await QuestionService.UpdateAsync(pollId, Id, request, cancellationToken);
        if (result.IsFailure)
            return result.Problem();
        return NoContent();
    }

    [HttpPatch("{Id}/toggleStatus")]
    public async Task<IActionResult> ToggleStatus([FromRoute] int pollId, [FromRoute] int Id, CancellationToken cancellationToken = default)
    {
        var result = await QuestionService.ToggleStatusAsync(pollId, Id, cancellationToken);
        if (result.IsFailure)
            return result.Problem();
        return NoContent();
    }

}
