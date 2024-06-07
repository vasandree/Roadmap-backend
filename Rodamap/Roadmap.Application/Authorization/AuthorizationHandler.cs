using Common.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Roadmap.Infrastructure;

namespace Roadmap.Application.Authorization;

public class AuthorizationHandler : AuthorizationHandler<AuthorizationRequirements>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public AuthorizationHandler(IHttpContextAccessor httpContextAccessor,
        IServiceScopeFactory serviceScopeFactory)
    {
        _httpContextAccessor = httpContextAccessor;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        AuthorizationRequirements requirement)
    {
        if (_httpContextAccessor.HttpContext != null)
        {
            string? authorizationString = _httpContextAccessor.HttpContext.Request.Headers[HeaderNames.Authorization];
            if (authorizationString == null) throw new Unauthorized();
            
            var token = authorizationString.Replace("Bearer ", "");

            using var scope = _serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var tokenEntity = await dbContext.ExpiredTokens
                .Where(x => x.TokenString == token)
                .FirstOrDefaultAsync();
            
            if (tokenEntity != null) throw new Unauthorized();
            
            context.Succeed(requirement);
        }
        else
        {
            throw new BadRequest("");
        }
    }
}