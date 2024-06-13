using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Roadmap.Application.Interfaces.Services;

namespace Roadmap.Controllers;

[ApiController, Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet, Authorize(Policy = "AuthorizationPolicy")]
    public async Task<IActionResult> GetUsers([FromQuery] string username)
    {
        return Ok(await _userService.GetUsers(username));
    }
}