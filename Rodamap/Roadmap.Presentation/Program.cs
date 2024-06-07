using Roadmap.Application.Configurators;
using Roadmap.Infrastructure;
using Roadmap.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureRoadmapInfrastructure();

builder.ConfigureRoadmapPersistence();

builder.ConfigureRoadmapApplication();

builder.ConfigureAuth();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

var app = builder.Build();

app.ConfigureRoadmapInfrastructure();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.Run();
