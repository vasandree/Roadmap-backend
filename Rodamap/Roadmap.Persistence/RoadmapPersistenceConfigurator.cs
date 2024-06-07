using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Roadmap.Application.Interfaces.Repositories;
using Roadmap.Persistence.Repositories;

namespace Roadmap.Persistence;

public static class RoadmapPersistenceConfigurator
{
    public static void ConfigureRoadmapPersistence(this WebApplicationBuilder builder)
    {
        builder.Services.AddTransient(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        builder.Services.AddTransient<IUserRepository, UserRepository>();
    }
}