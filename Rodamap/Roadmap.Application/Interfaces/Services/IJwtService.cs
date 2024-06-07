using System.Security.Claims;

namespace Roadmap.Application.Interfaces.Services;

public interface IJwtService
{
    string GenerateTokenString(Guid id, string email, string userName);
    ClaimsPrincipal? GetTokenPrincipal(string token);
    string GenerateRefreshTokenString();
}