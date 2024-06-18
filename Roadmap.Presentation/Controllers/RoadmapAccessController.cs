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

    [HttpPost, Authorize("AuthorizationPolicy"), Route("roadmap/publish")]
    public async Task<IActionResult> PublishRoadmap(Guid roadmapId)
    {
        await _roadmapAccessService.PublishRoadmap(Guid.Parse(User.FindFirst("UserId")!.Value!), roadmapId);
        return Ok();
    }

    [HttpPost, Authorize("AuthorizationPolicy"), Route("roadmap/access/add")]
    public async Task<IActionResult> AddPrivateAccess([FromQuery] Guid roadmapId, [FromBody] Guid[] usersIds)
    {
        await _roadmapAccessService.AddPrivateAccess(Guid.Parse(User.FindFirst("UserId")!.Value!), usersIds, roadmapId);
        return Ok();
    }
    
    [HttpPost, Authorize("AuthorizationPolicy"), Route("roadmap/access/remove")]
    public async Task<IActionResult> RemovePrivateAccess([FromQuery] Guid roadmapId, [FromBody] Guid[] usersIds)
    {
        await _roadmapAccessService.RemovePrivateAccess(Guid.Parse(User.FindFirst("UserId")!.Value!), usersIds, roadmapId);
        return Ok();
    }
}