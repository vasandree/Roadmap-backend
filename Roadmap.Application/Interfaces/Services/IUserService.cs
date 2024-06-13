using Microsoft.AspNetCore.Http;
using Roadmap.Application.Dtos.Requests;
using Roadmap.Application.Dtos.Responses;

namespace Roadmap.Application.Interfaces.Services;

public interface IUserService
{
    Task<TokensDto> RegisterUser(RegisterDto registerDto);
    Task<TokensDto> LoginUser(LoginDto loginDto);
    Task<UserDto> GetProfile(Guid userId);
    Task Logout(Guid userId, string refreshToken, string accessToken);
    Task<TokensDto> RefreshToken(TokensDto tokensDto);
    Task<IReadOnlyList<UserDto>> GetUsers(string username);
}