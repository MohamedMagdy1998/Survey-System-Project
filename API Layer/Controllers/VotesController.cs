using API_Layer.Extentions;
using Application.DTOs.Requests.Votes;
using Application.Services_Implementations;
using Application.Services_Interfaces;
using Domain.Common.Const;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace API_Layer.Controllers;

[Route("api/polls/{pollId}/vote")]
[ApiController]
[Authorize(Roles = DefaultRoles.Member)]
[EnableRateLimiting(RateLimiters.Concurrency)]

public class VotesController : ControllerBase
{
    private readonly IQuestionService QuestionService;
    private readonly IVoteService VoteService;

    public VotesController(IQuestionService questionService, IVoteService voteService)
    {
        QuestionService = questionService;
        VoteService = voteService;
    }

    [HttpGet("")]

    public async Task<IActionResult> StartVote(int pollId,CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var result = await QuestionService.GetAvailableAsync(pollId, userId!,cancellationToken);


        return result.IsSuccess ? Ok(result.Value) : result.Problem();

    }


    [HttpPost("")]
    public async Task<IActionResult> Vote([FromRoute] int pollId, [FromBody] VoteRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var result = await VoteService.AddAsync(pollId, userId!, request, cancellationToken);

        return result.IsSuccess ? Created() : result.Problem();
    }


}
