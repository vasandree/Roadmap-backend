using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Roadmap.Application.Authorization;
using Roadmap.Application.BackgroundServices;
using Roadmap.Application.Interfaces.Services;
using Roadmap.Application.Mapping;
using Roadmap.Application.Services;

namespace Roadmap.Application.Configurators;

public static class RoadmapApplicationConfigurator
{
    public static void ConfigureRoadmapApplication(this WebApplicationBuilder builder)
    {
        builder.Services.AddAutoMapper(typeof(MappingProfile));

        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IJwtService, JwtService>();
        builder.Services.AddScoped<IRoadmapService, RoadmapService>();
        builder.Services.AddScoped<IRoadmapAccessService, RoadmapAccessService>();

        builder.Services.AddHostedService<CleanExpiredRefreshTokens>();
        builder.Services.AddHostedService<CleanExpiredAccessTokens>();
        
        builder.Services.AddHttpContextAccessor();
    }
}