using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Roadmap.Infrastructure;

namespace Roadmap.Application.BackgroundServices;

public class CleanExpiredRefreshTokens : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public CleanExpiredRefreshTokens(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await CleanTokens(dbContext);
            }
            
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }

    private async Task CleanTokens(ApplicationDbContext dbContext)
    {
        var currentTime = DateTime.UtcNow;
        var expiredTokens = await dbContext.RefreshTokens
            .Where(rt => rt.ExpiryDate < currentTime)
            .ToListAsync();

        if (expiredTokens.Any())
        {
            dbContext.RefreshTokens.RemoveRange(expiredTokens);
            await dbContext.SaveChangesAsync();
        }
    }
}