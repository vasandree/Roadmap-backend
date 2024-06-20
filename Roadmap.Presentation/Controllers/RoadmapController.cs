using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Roadmap.Application.Dtos.Requests;
using Roadmap.Application.Interfaces.Services;

namespace Roadmap.Controllers;

[ApiController, Route("api")]
public class RoadmapController : ControllerBase
{
    private readonly IRoadmapService _roadmapService;

    public RoadmapController(IRoadmapService roadmapService)
    {
        _roadmapService = roadmapService;
    }


    [HttpGet, Route("roadmaps"), Authorize("AuthorizationPolicy"), AllowAnonymous]
    public async Task<IActionResult> GetRoadmaps(string? name, int page = 1)
    {
        var userIdClaim = User.FindFirst("UserId");
        Guid? userId = userIdClaim != null ? Guid.Parse(userIdClaim.Value) : null;

        return Ok(await _roadmapService.GetRoadmaps(userId, name, page));
    }

    [HttpGet, Authorize("AuthorizationPolicy"), Route("roadmaps/my")]
    public async Task<IActionResult> GetMyRoadmaps(int page = 1)
    {
        return Ok(await _roadmapService.GetMyRoadmaps(Guid.Parse(User.FindFirst("UserId")!.Value!), page));
    }

    [HttpGet, Authorize("AuthorizationPolicy"), Route("roadmaps/stared")]
    public async Task<IActionResult> GetStaredRoadmaps(int page = 1)
    {
        return Ok(await _roadmapService.GetStaredRoadmaps(Guid.Parse(User.FindFirst("UserId")!.Value!), page));
    }

    [HttpGet, Authorize("AuthorizationPolicy"), Route("roadmaps/private")]
    public async Task<IActionResult> GetPrivateRoadmaps(int page = 1)
    {
        return Ok(await _roadmapService.GetPrivateRoadmaps(Guid.Parse(User.FindFirst("UserId")!.Value!), page));
    }

    [HttpGet, Authorize("AuthorizationPolicy"), Route("roadmaps/recent")]
    public async Task<IActionResult> GetRecentRoadmaps()
    {
        return Ok(await _roadmapService.GetRecentRoadmaps(Guid.Parse(User.FindFirst("UserId")!.Value!)));
    }
    
    [HttpGet, Route("roadmap/{id}"), Authorize("AuthorizationPolicy"), AllowAnonymous]
    public async Task<IActionResult> GetRoadmap(Guid id)
    {
        var userIdClaim = User.FindFirst("UserId");
        Guid? userId = userIdClaim != null ? Guid.Parse(userIdClaim.Value) : null;


        return Ok(await _roadmapService.GetRoadmap(id, userId));
    }

    [HttpPost, Authorize("AuthorizationPolicy"), Route("roadmap")]
    public async Task<IActionResult> CreateRoadmap(RoadmapRequestDto roadmapRequestDto)
    {
        await _roadmapService.CreateRoadMap(roadmapRequestDto, Guid.Parse(User.FindFirst("UserId")!.Value!));
        return Ok();
    }

    [HttpPut, Authorize("AuthorizationPolicy"), Route("roadmap/{id}")]
    public async Task<IActionResult> EditRoadmap(Guid id, RoadmapRequestDto roadmapRequestDto)
    {
        await _roadmapService.EditRoadmap(id, roadmapRequestDto, Guid.Parse(User.FindFirst("UserId")!.Value!));
        return Ok();
    }

    [HttpDelete, Authorize("AuthorizationPolicy"), Route("roadmap/{id}")]
    public async Task<IActionResult> DeleteRoadmap(Guid id)
    {
        await _roadmapService.DeleteRoadmap(id, Guid.Parse(User.FindFirst("UserId")!.Value!));
        return Ok();
    }
    
    [HttpPost, Authorize("AuthorizationPolicy"), Route("roadmap/{id}/star")]
    public async Task<IActionResult> StarRoadmap(Guid id)
    {
        await _roadmapService.StarRoadmap(id, Guid.Parse(User.FindFirst("UserId")!.Value!));
        return Ok();
    }
}