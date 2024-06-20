using System.Text.Json;
using AutoMapper;
using Common.Exceptions;
using Roadmap.Application.Dtos.Requests;
using Roadmap.Application.Dtos.Responses;
using Roadmap.Application.Dtos.Responses.Paged;
using Roadmap.Application.Interfaces.Repositories;
using Roadmap.Application.Interfaces.Services;
using Roadmap.Domain.Enums;

namespace Roadmap.Application.Services;

public class RoadmapService : IRoadmapService
{
    private readonly IMapper _mapper;
    private readonly IUserRepository _repository;
    private readonly IRoadmapRepository _roadmapRepository;
    private readonly IStaredRoadmapRepository _staredRoadmap;
    private readonly IPrivateAccessRepository _accessRepository;


    public RoadmapService(IRoadmapRepository roadmapRepository, IMapper mapper,
        IPrivateAccessRepository accessRepository, IUserRepository repository, IStaredRoadmapRepository staredRoadmap)
    {
        _roadmapRepository = roadmapRepository;
        _mapper = mapper;
        _accessRepository = accessRepository;
        _repository = repository;
        _staredRoadmap = staredRoadmap;
    }


    public async Task<RoadmapResponseDto> GetRoadmap(Guid roadmapId, Guid? userId)
    {
        if (!await _roadmapRepository.CheckIfIdExists(roadmapId))
            throw new NotFound($"Roadmap with id={roadmapId} not found");

        var roadmap = await _roadmapRepository.GetById(roadmapId);

        if (roadmap.Status == Status.Public || roadmap.UserId == userId)
        {
            var dto = _mapper.Map<RoadmapResponseDto>(roadmap);
            
            if (userId.HasValue && _staredRoadmap.IsStared(userId.Value, roadmap.Id))
            {
                dto.IsStared = true;
            }

            return dto;
        }

        if (userId != null)
        {
            if (!await _accessRepository.CheckIfUserHasAccess(roadmapId, userId.Value))
                throw new Forbidden($"User does not have access to roadmap with id={roadmapId}");

            var dto = _mapper.Map<RoadmapResponseDto>(roadmap);
            
            if ( _staredRoadmap.IsStared(userId.Value, roadmap.Id))
            {
                dto.IsStared = true;
            }

            return dto;
        }

        throw new Forbidden($"User does not have access to roadmap with id={roadmapId}");
    }

    public async Task CreateRoadMap(RoadmapRequestDto roadmapRequestDto, Guid userId)
    {
        if (!await _repository.CheckIfIdExists(userId))
            throw new NotFound("User does not exist");

        var user = await _repository.GetById(userId);

        await _roadmapRepository.CreateAsync(new Domain.Entities.Roadmap
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = roadmapRequestDto.Name,
            Description = roadmapRequestDto.Description,
            Content = JsonDocument.Parse("{}"),
            Status = Status.Private,
            User = user,
        });
    }

    public async Task EditRoadmap(Guid roadmapId, RoadmapRequestDto roadmapRequestDto, Guid userId)
    {
        if (!await _roadmapRepository.CheckIfIdExists(roadmapId))
            throw new NotFound($"Roadmap with id={roadmapId} not found");

        var roadmap = await _roadmapRepository.GetById(roadmapId);

        if (roadmap.Status == Status.Public)
            throw new BadRequest("Roadmap is published. You can't edit it");
        
        if (!await _repository.CheckIfIdExists(userId))
            throw new NotFound("User does not exist");

        var user = await _repository.GetById(userId);

        if (roadmap.UserId != user.Id)
            throw new Forbidden($"User is not a creator of roadmap with id={roadmapId}");

        roadmap.Name = roadmapRequestDto.Name;
        roadmap.Description = roadmapRequestDto.Description;

        await _roadmapRepository.UpdateAsync(roadmap);
    }

    public async Task DeleteRoadmap(Guid roadmapId, Guid userId)
    {
        if (!await _roadmapRepository.CheckIfIdExists(roadmapId))
            throw new NotFound($"Roadmap with id={roadmapId} not found");

        var roadmap = await _roadmapRepository.GetById(roadmapId);

        if (!await _repository.CheckIfIdExists(userId))
            throw new NotFound("User does not exist");

        var user = await _repository.GetById(userId);

        if (roadmap.UserId != user.Id)
            throw new Forbidden($"User is not a creator of roadmap with id={roadmapId}");

        await _roadmapRepository.DeleteAsync(roadmap);
    }

    public async Task<RoadmapsPagedDto> GetRoadmaps(Guid? userId, string? name, int page)
    {
        var roadmaps = await _roadmapRepository.GetPublishedRoadmaps(name);

        return GetPagedRoadmaps(roadmaps, page, userId);
    }

    public async Task<RoadmapsPagedDto> GetMyRoadmaps(Guid userId, int page)
    {
        if (!await _repository.CheckIfIdExists(userId))
            throw new NotFound($"User with id={userId} not found");

        var roadmaps = await _roadmapRepository.GetUsersRoadmaps(userId);

        return GetPagedRoadmaps(roadmaps, page, userId);
    }


    public async Task<RoadmapsPagedDto> GetStaredRoadmaps(Guid userId, int page)
    {
        if (!await _repository.CheckIfIdExists(userId))
            throw new NotFound($"User with id={userId} not found");

        var roadmaps = await _staredRoadmap.GetStaredRoadmaps(userId);

        return GetPagedRoadmaps(roadmaps, page, userId);
    }

    private RoadmapsPagedDto GetPagedRoadmaps(List<Domain.Entities.Roadmap> roadmaps, int page, Guid? userId)
    {
        int totalRoadmapsCount = roadmaps.Count;
        var pagedRoadmaps = roadmaps
            .OrderByDescending(x => x.Stared?.Count() ?? 0)
            .Skip((page - 1) * 10)
            .Take(10)
            .ToList();

        int totalPages = (int)Math.Ceiling((double)totalRoadmapsCount / 10);

        var dto = new List<RoadmapPagedDto>();

        foreach (var roadmap in pagedRoadmaps)
        {
            var roadmapDto = _mapper.Map<RoadmapPagedDto>(roadmap);

            if (userId.HasValue && _staredRoadmap.IsStared(userId.Value, roadmap.Id))
            {
                roadmapDto.IsStared = true;
            }

            dto.Add(roadmapDto);
        }

        return new RoadmapsPagedDto
        {
            Roadmaps = dto,
            Pagination = new Pagination(10, totalPages, page)
        };
    }
}