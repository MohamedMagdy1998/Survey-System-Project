using API_Layer.Extentions;
using API_Layer.Filters;
using Application.DTOs.Requests.Polls;
using Application.DTOs.Responses.Polls;
using Application.Services_Interfaces;
using Domain.Common.Const;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

[Route("api/[controller]")]
[ApiController]
public class PollsController(IPollService pollService, ILogger<PollsController> logger) : ControllerBase
{
    private readonly IPollService _pollService = pollService;
    private readonly ILogger<PollsController> _logger = logger;

    [HttpGet("")]
    [HasPermission(Permissions.GetPolls)]

    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all polls");
        var response = await _pollService.GetAllAsync(cancellationToken);

        return response.IsSuccess ? Ok(response.Value) : response.Problem();    
    }

    [HttpGet("current")]
    [Authorize(Roles = DefaultRoles.Member)]
    [EnableRateLimiting(RateLimiters.UserLimiter)]

    public async Task<IActionResult> GetCurrent(CancellationToken cancellationToken)
    {
     
        var response = await _pollService.GetCurrentAsync(cancellationToken);

        return response.IsSuccess ? Ok(response.Value) : response.Problem();
    }

    [HttpGet("{id:int}")]
    [HasPermission(Permissions.GetPolls)]

    public async Task<IActionResult> Get([FromRoute] int id, CancellationToken cancellationToken)
    {
        var poll = await _pollService.GetAsync(id, cancellationToken);
       
        return poll.IsSuccess ? Ok(poll.Value) : poll.Problem();
    }

    [HttpPost("")]
    [HasPermission(Permissions.AddPolls)]
    public async Task<IActionResult> Add([FromBody] PollRequest request,CancellationToken cancellationToken)
    {
        var newPoll = await _pollService.AddAsync(request, cancellationToken);

        return newPoll.IsSuccess ? CreatedAtAction(nameof(Get), new { id = newPoll.Value.Id }, newPoll.Value) : newPoll.Problem();
    }

    [HttpPut("{id:int}")]
    [HasPermission(Permissions.UpdatePolls)]
    public async Task<IActionResult> Update([FromRoute] int id,[FromBody] PollRequest request,CancellationToken cancellationToken)
    {
        var isUpdated = await _pollService.UpdateAsync(id, request, cancellationToken);
      return isUpdated.IsSuccess ? NoContent() : isUpdated.Problem();
    }

    [HttpDelete("{id:int}")]
    [HasPermission(Permissions.DeletePolls)]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
    {
        var isDeleted = await _pollService.DeleteAsync(id, cancellationToken);
       
        return isDeleted.IsSuccess ? NoContent() : isDeleted.Problem();
    }

    [HttpPut("{id:int}/togglePublish")]
    [HasPermission(Permissions.UpdatePolls)]
    public async Task<IActionResult> TogglePublish([FromRoute] int id, CancellationToken cancellationToken)
    {
        var isUpdated = await _pollService.TogglePublishStatusAsync(id, cancellationToken);
       
        return isUpdated.IsSuccess ? NoContent() : isUpdated.Problem();
    }
}