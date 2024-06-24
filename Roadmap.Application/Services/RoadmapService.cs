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
    private readonly IPrivateAccessRepository _accessRepository;


    public RoadmapService(IRoadmapRepository roadmapRepository, IMapper mapper,
        IPrivateAccessRepository accessRepository, IUserRepository repository)
    {
        _roadmapRepository = roadmapRepository;
        _mapper = mapper;
        _accessRepository = accessRepository;
        _repository = repository;
    }


    public async Task<RoadmapResponseDto> GetRoadmap(Guid roadmapId, Guid? userId)
    {
        if (!await _roadmapRepository.CheckIfIdExists(roadmapId))
            throw new NotFound($"Roadmap with id={roadmapId} not found");

        var roadmap = await _roadmapRepository.GetById(roadmapId);

        if (userId.HasValue)
        {
            if (!await _repository.CheckIfIdExists(userId.Value))
                throw new NotFound("User does not exist");

            var user = await _repository.GetById(userId.Value);

            if (roadmap.Status != Status.Public)
            {
                if (!await _accessRepository.CheckIfUserHasAccess(roadmapId, userId.Value))
                    throw new Forbidden($"User does not have access to roadmap with id={roadmapId}");
            }

            var dto = _mapper.Map<RoadmapResponseDto>(roadmap);

            if (user.Stared != null && user.Stared.Contains(roadmap.Id))
                dto.IsStared = true;

            await AddRecentlyVisited(user, roadmapId);

            return dto;
        }

        if (roadmap.Status == Status.Public)
        {
            return _mapper.Map<RoadmapResponseDto>(roadmap);
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
            Content = null,
            Status = Status.Private,
            StarsCount = 0,
            TopicsCount = 0,
            User = user,
            PrivateAccesses = new List<PrivateAccess>()
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

        if (jsonContent != roadmap.Content)
        {
            if (roadmap.Content != null)
            {
                var oldTopicsIds = GetNonEdges(roadmap.Content);
                var newTopicIds = GetNonEdges(jsonContent);

                //todo: progress delete

                roadmap.TopicsCount = newTopicIds.Count;
            }

            roadmap.Content = jsonContent;
            await _roadmapRepository.UpdateAsync(roadmap);
        }
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

        return await GetPagedRoadmaps(roadmaps, page, userId);
    }

    public async Task<RoadmapsPagedDto> GetMyRoadmaps(Guid userId, int page)
    {
        if (!await _repository.CheckIfIdExists(userId))
            throw new NotFound($"User with id={userId} not found");

        var roadmaps = await _roadmapRepository.GetUsersRoadmaps(userId);

        return await GetPagedRoadmaps(roadmaps, page, userId);
    }


    public async Task<RoadmapsPagedDto> GetStaredRoadmaps(Guid userId, int page)
    {
        if (!await _repository.CheckIfIdExists(userId))
            throw new NotFound($"User with id={userId} not found");

        var user = await _repository.GetById(userId);

        var roadmapIds = user.Stared?.ToList();

        if (roadmapIds == null || !roadmapIds.Any())
            return new RoadmapsPagedDto();

        var existingRoadmapIds = new List<Guid>();
        foreach (var roadmapId in roadmapIds)
        {
            if (await _roadmapRepository.CheckIfIdExists(roadmapId))
            {
                existingRoadmapIds.Add(roadmapId);
            }
        }

        if (!existingRoadmapIds.Any())
            return new RoadmapsPagedDto();

        var roadmaps = await _roadmapRepository.GetRoadmapsByIds(existingRoadmapIds);

        return await GetPagedRoadmaps(roadmaps, page, userId);
    }


    public async Task<RoadmapsPagedDto> GetPrivateRoadmaps(Guid userId, int page)
    {
        if (!await _repository.CheckIfIdExists(userId))
            throw new NotFound($"User with id={userId} not found");

        var roadmaps = await _accessRepository.GetPrivateRoadmaps(userId);

        return await GetPagedRoadmaps(roadmaps, page, userId);
    }

    public async Task<RoadmapsPagedDto> GetRecentRoadmaps(Guid userId)
    {
        if (!await _repository.CheckIfIdExists(userId))
            throw new NotFound($"User with id={userId} not found");

        var user = await _repository.GetById(userId);

        var roadmapIds = user.RecentlyVisited?.ToList();

        if (roadmapIds == null || !roadmapIds.Any())
            return new RoadmapsPagedDto();

        var existingRoadmapIds = new List<Guid>();
        foreach (var roadmapId in roadmapIds)
        {
            if (await _roadmapRepository.CheckIfIdExists(roadmapId))
            {
                existingRoadmapIds.Add(roadmapId);
            }
        }

        if (!existingRoadmapIds.Any())
            return new RoadmapsPagedDto();

        var roadmaps = await _roadmapRepository.GetRoadmapsByIds(existingRoadmapIds);

        return await GetPagedRoadmaps(roadmaps, 1, userId);
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

        if (user.Stared != null && user.Stared.Contains(roadmapId))
        {
            user.Stared.Remove(roadmapId);
        }
        else if (user.Stared != null)
        {
            user.Stared.Add(roadmapId);
        }
        else
        {
            user.Stared = new HashSet<Guid>();
            user.Stared.Add(roadmapId);
        }

        await _repository.UpdateAsync(user);
    }

    public async Task<RoadmapResponseDto> CopyRoadmap(Guid userId, Guid roadmapId)
    {
        
        if (!await _roadmapRepository.CheckIfIdExists(roadmapId))
            throw new NotFound($"Roadmap with id={roadmapId} not found");

        var roadmap = await _roadmapRepository.GetById(roadmapId);

        if (!await _repository.CheckIfIdExists(userId))
            throw new NotFound("User does not exist");

        var user = await _repository.GetById(userId);
        
        if (roadmap.Status != Status.Public || roadmap.UserId != userId)
            throw new BadRequest("Roadmap is not published");

        
        var newRoadmap = new Domain.Entities.Roadmap
        {
            Id = Guid.NewGuid(),
            UserId = roadmap.UserId,
            Name = $"{roadmap.Name}(копия)",
            Description = roadmap.Description,
            Content = null,
            Status = Status.Private,
            StarsCount = 0,
            TopicsCount = 0,
            User = user,
            PrivateAccesses = new List<PrivateAccess>()
        };

        await _roadmapRepository.CreateAsync(newRoadmap);
        
        return _mapper.Map<RoadmapResponseDto>(newRoadmap);
    }

    public async Task<RoadmapsPagedDto> GetUsersRoadmaps(Guid userId, Guid roadmapUserId, int page)
    {
        if (!await _repository.CheckIfIdExists(userId))
            throw new NotFound("User does not exist");
        
        if (!await _repository.CheckIfIdExists(roadmapUserId))
            throw new NotFound("User does not exist");

        var roadmapUser = await _repository.GetById(userId);

        if (roadmapUser.CreatedRoadmaps != null)
            return await GetPagedRoadmaps(roadmapUser.CreatedRoadmaps.ToList(), page, userId);

        return new RoadmapsPagedDto();
    }

    private async Task<RoadmapsPagedDto> GetPagedRoadmaps(List<Domain.Entities.Roadmap> roadmaps, int page,
        Guid? userId)
    {
        int totalRoadmapsCount = roadmaps.Count;
        var pagedRoadmaps = roadmaps
            .OrderByDescending(x => x.StarsCount)
            .Skip((page - 1) * 10)
            .Take(10)
            .ToList();

        int totalPages = (int)Math.Ceiling((double)totalRoadmapsCount / 10);

        var dto = new List<RoadmapPagedDto>();

        foreach (var roadmap in pagedRoadmaps)
        {
            var roadmapDto = _mapper.Map<RoadmapPagedDto>(roadmap);


            if (userId.HasValue)
            {
                if (!await _repository.CheckIfIdExists(userId.Value))
                    throw new NotFound("User does not exist");

                var user = await _repository.GetById(userId.Value);

                if (user.Stared != null && user.Stared.Contains(roadmap.Id))
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

    private async Task AddRecentlyVisited(User user, Guid roadmapId)
    {
        if (user.RecentlyVisited == null)
        {
            user.RecentlyVisited = new LinkedList<Guid>();
        }

        if (user.RecentlyVisited.Contains(roadmapId))
        {
            user.RecentlyVisited.Remove(roadmapId);
        }

        user.RecentlyVisited.AddFirst(roadmapId);

        if (user.RecentlyVisited.Count > 5)
        {
            user.RecentlyVisited.RemoveLast();
        }

        await _repository.UpdateAsync(user);
    }

    private List<Guid> GetNonEdges(JsonDocument jsonDocument)
    {
        var jsonElements = jsonDocument.RootElement.EnumerateArray();

        var nonEdgeNodes = jsonElements
            .Where(element => element.TryGetProperty("shape", out var shape) && shape.GetString() != "edge")
            .ToList();

        return nonEdgeNodes.Any()
            ? nonEdgeNodes.Select(element => Guid.Parse(element.GetProperty("id").GetString())).ToList()
            : new List<Guid>();
    }
}