using System.Text.Json;
using Roadmap.Application.Dtos.Requests;
using Roadmap.Application.Dtos.Responses;
using Roadmap.Application.Dtos.Responses.Paged;

namespace Roadmap.Application.Interfaces.Services;

public interface IRoadmapService
{
    Task<RoadmapResponseDto> GetRoadmap(Guid roadmapId, Guid? userId);
    Task<Guid> CreateRoadMap(RoadmapRequestDto roadmapRequestDto, Guid userId);
    Task EditRoadmap(Guid roadmapId, RoadmapRequestDto roadmapRequestDto, Guid userId);
    Task EditRoadmapContent(Guid roadmapId, JsonDocument jsonContent, Guid userId);
    Task DeleteRoadmap(Guid roadmapId, Guid userId);
    Task<RoadmapsPagedDto> GetRoadmaps(Guid? userId, string? name, int page);
    Task<RoadmapsPagedDto> GetMyRoadmaps(Guid userId, int page);
    Task<RoadmapsPagedDto> GetStaredRoadmaps(Guid userId, int page);
    Task<RoadmapsPagedDto> GetPrivateRoadmaps(Guid userId, int page);
    Task<RoadmapsPagedDto> GetRecentRoadmaps(Guid userId);
    Task StarRoadmap(Guid userId, Guid roadmapId);
    Task<Guid> CopyRoadmap(Guid userId, Guid roadmapId);
    Task<RoadmapsPagedDto> GetUsersRoadmaps(Guid userId, Guid roadmapUserId, int page);
}