using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Roadmap.Application.Interfaces.Services;

namespace Roadmap.Controllers;

[ApiController, Route("api")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet, Authorize(Policy = "AuthorizationPolicy"), Route("users"), AllowAnonymous]
    public async Task<IActionResult> GetUsers([FromQuery] string username)
    {
        return Ok(await _userService.GetUsers(Guid.Parse(User.FindFirst("UserId")!.Value!),username));
    }
}