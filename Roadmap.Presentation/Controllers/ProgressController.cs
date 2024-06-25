using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Roadmap.Application.Interfaces.Repositories;
using Roadmap.Application.Interfaces.Services;
using Roadmap.Domain.Enums;

namespace Roadmap.Controllers;

[ApiController, Route("api")]
public class ProgressController : ControllerBase
{
    private readonly IProgressService _progressService;

    public ProgressController(IProgressService progressService)
    {
        _progressService = progressService;
    }

    [HttpPut, Authorize("AuthorizationPolicy"), Route("roadmaps/{roadmapId}/topics/{topicId}/progress")]
    public async Task<IActionResult> ChangeProgress(Guid roadmapId, Guid topicId, ProgressStatus progressStatus)
    {
        await _progressService.ChangeProgress(Guid.Parse(User.FindFirst("UserId")!.Value!), roadmapId, topicId,
            progressStatus);
        return Ok();
    }
}