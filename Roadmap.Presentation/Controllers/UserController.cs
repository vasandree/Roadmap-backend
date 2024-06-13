using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Roadmap.Application.Dtos.Requests;
using Roadmap.Application.Dtos.Responses;
using Roadmap.Application.Interfaces.Services;

namespace Roadmap.Controllers;

[ApiController, Route("api/user")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost, Route("login")]
    public async Task<IActionResult> LoginUser([FromBody] LoginDto loginDto)
    {
        return Ok(await _userService.LoginUser(loginDto));
    }

    [HttpPost, Route("register")]
    public async Task<IActionResult> RegisterUser([FromBody] RegisterDto registerDto)
    {
        return Ok(await _userService.RegisterUser(registerDto));
    }

    [HttpGet, Authorize(Policy = "AuthorizationPolicy")]
    public async Task<IActionResult> GetUserProfile()
    {
        return Ok(await _userService.GetProfile(Guid.Parse(User.FindFirst("UserId")!.Value!)));
    }

    [HttpPost, Authorize(Policy = "AuthorizationPolicy"), Route("logout")]
    public async Task<IActionResult> Logout(string refreshToken)
    {
        var accessToken = HttpContext.Request.Headers["Authorization"].First()?.Replace("Bearer ", "")!;
        await _userService.Logout(Guid.Parse(User.FindFirst("UserId")!.Value!), refreshToken, accessToken);
        return Ok();
    }

    [HttpPost, Authorize(Policy = "AuthorizationPolicy"), Route("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody]TokensDto tokensDto)
    {
        return Ok(await _userService.RefreshToken(tokensDto));
    }
}
