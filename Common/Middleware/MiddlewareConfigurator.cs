using Microsoft.AspNetCore.Builder;

namespace Common.Middleware;

public static class MiddlewareConfigurator
{
    public static void UseMiddleware(this WebApplication app)
    {
        app.UseMiddleware<MiddlewareService>();
    }
}