using System.Text.Json;
using AutoMapper;
using Common.Exceptions;
using Roadmap.Application.Dtos.Requests;
using Roadmap.Application.Dtos.Responses;
using Roadmap.Application.Dtos.Responses.Paged;
using Roadmap.Application.Interfaces.Repositories;
using Roadmap.Application.Interfaces.Services;
using Roadmap.Domain.Entities;
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
            
            if (userId.HasValue && userId != roadmap.UserId)
            {
                if (_staredRoadmap.IsStared(userId.Value, roadmap.Id))
                {
                    dto.IsStared = true;
                }

                await AddRecentlyVisited(userId.Value, roadmap.Id);
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
            await AddRecentlyVisited(userId.Value, roadmap.Id);
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

    public async Task EditRoadmapContent(Guid roadmapId, JsonDocument jsonContent, Guid userId)
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

        roadmap.Content = jsonContent;

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

    public async Task<RoadmapsPagedDto> GetPrivateRoadmaps(Guid userId, int page)
    {
        if (!await _repository.CheckIfIdExists(userId))
            throw new NotFound($"User with id={userId} not found");

        var roadmaps = await _accessRepository.GetPrivateRoadmaps(userId);

        return GetPagedRoadmaps(roadmaps, page, userId);
    }

    public async Task<List<RoadmapPagedDto>> GetRecentRoadmaps(Guid userId)
    {
        if (!await _repository.CheckIfIdExists(userId))
            throw new NotFound($"User with id={userId} not found");
    
        var user = await _repository.GetById(userId);
        var roadmapsIds = user.RecentlyVisited;

        if (roadmapsIds == null || !roadmapsIds.Any())
            return new List<RoadmapPagedDto>();

        var roadmapsTasks = roadmapsIds.Select(async x => 
        {
            var roadmap = await _roadmapRepository.GetById(x);
            return _mapper.Map<RoadmapPagedDto>(roadmap);
        });

        var roadmaps = await Task.WhenAll(roadmapsTasks);

        return roadmaps.ToList();
    }


    public async Task StarRoadmap(Guid userId, Guid roadmapId)
    {
        if (!await _roadmapRepository.CheckIfIdExists(roadmapId))
            throw new NotFound($"Roadmap with id={roadmapId} not found");

        var roadmap = await _roadmapRepository.GetById(roadmapId);

        if (!await _repository.CheckIfIdExists(userId))
            throw new NotFound("User does not exist");

        var user = await _repository.GetById(userId);

        if (roadmap.UserId == user.Id)
            throw new BadRequest("You can't mark your own roadmap");

        if (roadmap.Status != Status.Public)
            throw new BadRequest("Roadmap is not published");

        if (_staredRoadmap.IsStared(user.Id, roadmap.Id))
        {
            await _staredRoadmap.DeleteAsync(await _staredRoadmap.GetByUserAndRoadmap(user.Id, roadmap.Id));
        }
        else
        {
            await _staredRoadmap.CreateAsync(new StaredRoadmap
            {
                Id = Guid.NewGuid(),
                RoadmapId = roadmapId,
                UserId = userId,
                User = user,
                Roadmap = roadmap
            });
        }
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

    private async Task AddRecentlyVisited(Guid userId, Guid roadmapId)
    {
        var user = await _repository.GetById(userId);

        if (user.RecentlyVisited == null)
        {
            user.RecentlyVisited = new List<Guid>();
        }

        user.RecentlyVisited.Remove(roadmapId);

        user.RecentlyVisited.Insert(0, roadmapId);

        if (user.RecentlyVisited.Count > 5)
        {
            user.RecentlyVisited = user.RecentlyVisited.Take(5).ToList();
        }

        await _repository.UpdateAsync(user);
    }
}