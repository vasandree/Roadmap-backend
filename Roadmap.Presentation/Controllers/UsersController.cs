using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Roadmap.Application.Interfaces.Services;

namespace Roadmap.Controllers;

[ApiController, Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IRoadmapService _roadmapService;

    public UsersController(IUserService userService, IRoadmapService roadmapService)
    {
        _userService = userService;
        _roadmapService = roadmapService;
    }

    [HttpGet, Authorize(Policy = "AuthorizationPolicy"),  AllowAnonymous]
    public async Task<IActionResult> GetUsers([FromQuery] string username)
    {
        var userIdClaim = User.FindFirst("UserId");
        Guid? userId = userIdClaim != null ? Guid.Parse(userIdClaim.Value) : null;
        
        return Ok(await _userService.GetUsers(userId, username));
    }

    [HttpGet, Authorize(Policy = "AuthorizationPolicy"), Route("{userId}/roadmaps"), AllowAnonymous]
    public async Task<IActionResult> GetUsersRoadmaps(Guid userIdRoadmaps, int page = 1)
    {
        var userIdClaim = User.FindFirst("UserId");
        Guid? userId = userIdClaim != null ? Guid.Parse(userIdClaim.Value) : null;
        
        return Ok(await _roadmapService.GetUsersRoadmaps(userId, userIdRoadmaps, page));
    }
}