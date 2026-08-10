using Application.DTOs.Requests.Polls;
using Application.Services_Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PollsController(IPollService pollService) : ControllerBase
{
    private readonly IPollService _pollService = pollService;

    [HttpGet("")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var response = await _pollService.GetAllAsync(cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get([FromRoute] int id, CancellationToken cancellationToken)
    {
        var poll = await _pollService.GetAsync(id, cancellationToken);
        if (poll is null)
            return NotFound();

        return Ok(poll);
    }

    [HttpPost("")]
    public async Task<IActionResult> Add([FromBody] PollRequest request,CancellationToken cancellationToken)
    {
        var newPoll = await _pollService.AddAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = newPoll.Id }, newPoll);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        [FromRoute] int id,
        [FromBody] PollRequest request,
        CancellationToken cancellationToken)
    {
        var isUpdated = await _pollService.UpdateAsync(id, request, cancellationToken);
        if (!isUpdated)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
    {
        var isDeleted = await _pollService.DeleteAsync(id, cancellationToken);
        if (!isDeleted)
            return NotFound();

        return NoContent();
    }

    [HttpPut("{id:int}/togglePublish")]
    public async Task<IActionResult> TogglePublish([FromRoute] int id, CancellationToken cancellationToken)
    {
        var isUpdated = await _pollService.TogglePublishStatusAsync(id, cancellationToken);
        if (!isUpdated)
            return NotFound();

        return NoContent();
    }
}