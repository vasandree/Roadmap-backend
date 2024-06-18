using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Roadmap.Application.Interfaces.Repositories;
using Roadmap.Application.Interfaces.Services;
using Roadmap.Domain.Entities;
using Roadmap.Persistence.Repositories;

namespace Roadmap.Persistence;

public static class RoadmapPersistenceConfigurator
{
    public static void ConfigureRoadmapPersistence(this WebApplicationBuilder builder)
    {
        builder.Services.AddTransient(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        builder.Services.AddTransient<IUserRepository, UserRepository>();
        builder.Services.AddTransient<IExpiredToken, ExpiredTokenRepository>();
        builder.Services.AddTransient<IRefreshToken, RefreshTokenRepository>();
        builder.Services.AddTransient<IRoadmapRepository, RoadmapRepository>();
        builder.Services.AddTransient<IPrivateAccessRepository, PrivateAccessRepository>();
        builder.Services.AddTransient<IStaredRoadmapRepository, StaredRoadmapRepository>();
    }
}