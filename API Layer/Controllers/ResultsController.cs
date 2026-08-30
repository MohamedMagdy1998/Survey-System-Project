using API_Layer.Extentions;
using API_Layer.Filters;
using Application.Services_Interfaces;
using Domain.Common.Const;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API_Layer.Controllers;

[Route("api/[controller]")]
[ApiController]
[HasPermission(Permissions.Results)]
public class ResultsController(IResultService resultService) : ControllerBase
{
    [HttpGet("row-data")]
    public async Task<IActionResult> PollVotes([FromRoute] int pollId, CancellationToken cancellationToken)
    {
        var result = await resultService.GetPollVotesAsync(pollId, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.Problem();
    }

    [HttpGet("votes-per-day")]
    public async Task<IActionResult> VotesPerDay([FromRoute] int pollId, CancellationToken cancellationToken)
    {
        var result = await resultService.GetVotesPerDayAsync(pollId, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.Problem();
    }

    [HttpGet("votes-per-question")]
    public async Task<IActionResult> VotesPerQuestion([FromRoute] int pollId, CancellationToken cancellationToken)
    {
        var result = await resultService.GetVotesPerQuestionAsync(pollId, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.Problem();
    }
}
