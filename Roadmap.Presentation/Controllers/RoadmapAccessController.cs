using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Roadmap.Application.Interfaces.Services;

namespace Roadmap.Controllers;

[ApiController, Route("api")]
public class RoadmapAccessController : ControllerBase
{
    private readonly IRoadmapAccessService _roadmapAccessService;

    public RoadmapAccessController(IRoadmapAccessService roadmapAccessService)
    {
        _roadmapAccessService = roadmapAccessService;
    }

    [HttpPost, Authorize("AuthorizationPolicy"), Route("roadmap/{id}/publish")]
    public async Task<IActionResult> PublishRoadmap(Guid id)
    {
        await _roadmapAccessService.PublishRoadmap(Guid.Parse(User.FindFirst("UserId")!.Value!), id);
        return Ok();
    }

    [HttpPost, Authorize("AuthorizationPolicy"), Route("roadmap/{id}/access/add")]
    public async Task<IActionResult> AddPrivateAccess(Guid id, [FromBody] Guid[] usersIds)
    {
        await _roadmapAccessService.AddPrivateAccess(Guid.Parse(User.FindFirst("UserId")!.Value!), usersIds, id);
        return Ok();
    }

    [HttpPost, Authorize("AuthorizationPolicy"), Route("roadmap/{id}/access/remove")]
    public async Task<IActionResult> RemovePrivateAccess(Guid id, [FromBody] Guid[] usersIds)
    {
        await _roadmapAccessService.RemovePrivateAccess(Guid.Parse(User.FindFirst("UserId")!.Value!), usersIds,
            id);
        return Ok();
    }
    
    [HttpGet, Authorize(Policy = "AuthorizationPolicy"), Route("{id}/users")]
    public async Task<IActionResult> GetUsersForRoadmap(Guid id, string? username)
    {
        return Ok(await _roadmapAccessService.GetPrivateUsers(Guid.Parse(User.FindFirst("UserId")!.Value!), id, username));
    }
}