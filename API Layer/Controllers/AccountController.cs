using API_Layer.Extentions;
using Application.DTOs.Requests.Users;
using Application.Services_Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API_Layer.Controllers;

[Route("me")]
[ApiController]
[Authorize]
public class AccountController(IUserService userService) : ControllerBase
{
    private readonly IUserService _userService = userService;

    [HttpGet("")]
    public async Task<IActionResult> Info()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var result = await _userService.GetProfileAsync(userId!);

        return Ok(result.Value);
    }

    [HttpPut("info")]
    public async Task<IActionResult> Info([FromBody] UpdateProfileRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _userService.UpdateProfileAsync(userId, request);

        return NoContent();
    }

    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _userService.ChangePasswordAsync(userId, request);

        return result.IsSuccess ? NoContent() : result.Problem();
    }
}
