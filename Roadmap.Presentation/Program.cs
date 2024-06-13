using Common.Middleware;
using Roadmap.Application.Configurators;
using Roadmap.Infrastructure;
using Roadmap.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureRoadmapInfrastructure();

builder.ConfigureRoadmapPersistence();

builder.ConfigureRoadmapApplication();

builder.ConfigureAuth();

builder.ConfigureSwagger();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

var app = builder.Build();

app.ConfigureRoadmapInfrastructure();

app.UseMiddleware();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
