using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Roadmap.Application.Dtos.Requests;
using Roadmap.Application.Dtos.Responses;
using Roadmap.Application.Interfaces.Services;

namespace Rodamap.Controllers;

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

    [HttpPut, Authorize(Policy = "AuthorizationPolicy")]
    public async Task<IActionResult> EditProfile(EditProfileDto editProfileDto)
    {
        await _userService.EditProfile(Guid.Parse(User.FindFirst("UserId")!.Value!), editProfileDto);
        return Ok();
    }

    [HttpPut, Authorize(Policy = "AuthorizationPolicy")]
    public async Task<IActionResult> ChangePassword(EditPasswordDto editPasswordDto)
    {
        await _userService.ChangePassword(Guid.Parse(User.FindFirst("UserId")!.Value!), editPasswordDto);
        return Ok();
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