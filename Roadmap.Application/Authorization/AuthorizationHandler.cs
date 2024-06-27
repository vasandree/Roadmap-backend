using System.IdentityModel.Tokens.Jwt;
using Common.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Roadmap.Infrastructure;

namespace Roadmap.Application.Authorization
{
    public class AuthorizationHandler : AuthorizationHandler<AuthorizationRequirements>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public AuthorizationHandler(IHttpContextAccessor httpContextAccessor,
            IServiceScopeFactory serviceScopeFactory)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
            AuthorizationRequirements requirement)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext == null)
            {
                throw new BadRequest("HttpContext is null");
            }

            var headers = httpContext.Request.Headers;

            if (headers == null)
            {
                throw new BadRequest("Headers cannot be null");
            }

            if (!headers.TryGetValue(HeaderNames.Authorization, out var authorizationString))
            {
                throw new Unauthorized("Authorization header is missing or empty");
            }

            var token = authorizationString.ToString().Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);

            if (string.IsNullOrEmpty(token))
            {
                throw new Unauthorized("Token is missing");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            if (tokenHandler.CanReadToken(token))
            {
                var jwtToken = tokenHandler.ReadJwtToken(token);

                var expClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Exp);

                if (expClaim != null && long.TryParse(expClaim.Value, out var exp))
                {
                    var expirationDate = DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;
                    if (expirationDate <= DateTime.UtcNow)
                    {
                        throw new Unauthorized("Token is expired");
                    }
                }
            }

            using var scope = _serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var tokenEntity = await dbContext.ExpiredTokens
                .FirstOrDefaultAsync(x => x.TokenString == token);

            if (tokenEntity != null)
            {
                throw new Unauthorized("Token is expired");
            }

            context.Succeed(requirement);
        }
    }
}
