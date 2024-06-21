using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Roadmap.Application.Dtos.Requests;
using Roadmap.Application.Dtos.Responses;
using Roadmap.Application.Interfaces.Services;

namespace Roadmap.Controllers;

[ApiController, Route("api/topic")]
public class TopicController : ControllerBase
{
    private readonly ITopicService _topic;

    public TopicController(ITopicService topic)
    {
        _topic = topic;
    }

    [HttpGet, Route("{id}"), Authorize("AuthorizationPolicy"), AllowAnonymous]
    public async Task<IActionResult> GetTopic(Guid id)
    {
        return Ok(await _topic.GetTopic(id, Guid.Parse(User.FindFirst("UserId")!.Value!)));
    }

    [HttpPost, Authorize("AuthorizationPolicy")]
    public async Task<IActionResult> CreateTopic(Guid id, TopicDto topicDto)
    {
        await _topic.CreateTopic(topicDto, Guid.Parse(User.FindFirst("UserId")!.Value!), id);
        return Ok();
    }

    [HttpPut, Route("{id}"), Authorize("AuthorizationPolicy")]
    public async Task<IActionResult> EditTopic(Guid id, EditTopicDto topicDto)
    {
        await _topic.EditTopic(id, topicDto, Guid.Parse(User.FindFirst("UserId")!.Value!));
        return Ok();
    }

    [HttpDelete, Route("{id}"), Authorize("AuthorizationPolicy")]
    public async Task<IActionResult> DeleteTopic(Guid id)
    {
        await _topic.DeleteTopic(id, Guid.Parse(User.FindFirst("UserId")!.Value!));
        return Ok();
    }
}