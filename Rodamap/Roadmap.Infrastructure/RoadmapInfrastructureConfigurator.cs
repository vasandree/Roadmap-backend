using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Roadmap.Infrastructure;

public static class RoadmapInfrastructureConfigurator
{
    public static void ConfigureRoadmapInfrastructure(this WebApplicationBuilder builder)
    {
        var connection = builder.Configuration.GetConnectionString("PostgresUser");
        builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connection));
    }

    public static void ConfigureRoadmapInfrastructure(this WebApplication application)
    {
        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();
            dbContext?.Database.Migrate();
            
        }
    }
}