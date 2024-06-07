using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Roadmap.Infrastructure;

namespace Roadmap.Application.BackgroundServices;

public class CleanExpiredAccessTokens : BackgroundService
{
    private readonly TimeSpan _cleanMinutes;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public CleanExpiredAccessTokens(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
    {
        _cleanMinutes = TimeSpan.FromMinutes(5);
        _serviceScopeFactory = serviceScopeFactory;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await CleanExpiredTokensAsync(stoppingToken);
            await Task.Delay(_cleanMinutes, stoppingToken);
        }
    }

    private async Task CleanExpiredTokensAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var expiredTokens = await context.ExpiredTokens
            .ToListAsync(cancellationToken: stoppingToken);

        foreach (var token in expiredTokens)
        {
            context.ExpiredTokens.Remove(token);
        }
        await context.SaveChangesAsync(stoppingToken);
    }
}