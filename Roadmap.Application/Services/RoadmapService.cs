using System.Text.Json;
using AutoMapper;
using Common.Exceptions;
using Roadmap.Application.Dtos.Requests;
using Roadmap.Application.Dtos.Responses;
using Roadmap.Application.Dtos.Responses.Paged;
using Roadmap.Application.Helpers;
using Roadmap.Application.Interfaces.Repositories;
using Roadmap.Application.Interfaces.Services;
using Roadmap.Domain.Entities;
using Roadmap.Domain.Enums;
using Progress = Roadmap.Domain.Entities.Progress;

namespace Roadmap.Application.Services;

public class RoadmapService : IRoadmapService
{
    private readonly IMapper _mapper;
    private readonly IUserRepository _repository;
    private readonly ProgressHelper _progressHelper;
    private readonly IRoadmapRepository _roadmapRepository;
    private readonly IProgressRepository _progressRepository;
    private readonly IPrivateAccessRepository _accessRepository;


    public RoadmapService(IRoadmapRepository roadmapRepository, IMapper mapper,
        IPrivateAccessRepository accessRepository, IUserRepository repository, IProgressRepository progressRepository, ProgressHelper progressHelper)
    {
        _roadmapRepository = roadmapRepository;
        _mapper = mapper;
        _accessRepository = accessRepository;
        _repository = repository;
        _progressRepository = progressRepository;
        _progressHelper = progressHelper;
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
            
            if(!await _progressRepository.CheckIfExists(user.Id, roadmapId))
                await CreateProgress(user, roadmap);

            return dto;
        }

        if (roadmap.Status == Status.Public)
        {
            return _mapper.Map<RoadmapResponseDto>(roadmap);
        }

        throw new Forbidden($"User does not have access to roadmap with id={roadmapId}");
    }

    private async Task CreateProgress(User user, Domain.Entities.Roadmap roadmap)
    {
        if (roadmap.Content != null)
        {
            var topicsIds = _progressHelper.GetTopics(roadmap.Content);

            await _progressRepository.CreateAsync(new Progress
            {
                Id = Guid.NewGuid(),
                RoadmapId = roadmap.Id,
                UserId = user.Id,
                UsersProgress = CreateUsersProgressJson(topicsIds),
                User = user,
                Roadmap = roadmap
            });
        }
    }

    private JsonDocument CreateUsersProgressJson(List<Guid> topicIds)
    {
        var progressList = topicIds.Select(id => new
        {
            Id = id,
            Status = ProgressStatus.Pending.ToString()
        }).ToList();

        var jsonString = JsonSerializer.Serialize(progressList);
        var jsonDocument = JsonDocument.Parse(jsonString);
        return jsonDocument;
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
                //todo: compare topics content
                
                var oldTopicsIds = _progressHelper.GetTopics(roadmap.Content);
                var newTopicIds = _progressHelper.GetTopics(jsonContent);

                var deletedTopicIds = oldTopicsIds.Except(newTopicIds).ToList();
                var addedTopicIds = newTopicIds.Except(oldTopicsIds).ToList();
                
                await _progressHelper.ChangeProgress(userId, roadmapId, deletedTopicIds, addedTopicIds);
                
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

        //todo: delete from recent, delete from stared

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

        var roadmaps = await _roadmapRepository.GetRoadmapsByIds(roadmapIds);


        return await GetPagedRoadmaps(roadmaps, page, userId);
    }

    public async Task<RoadmapsPagedDto> GetPrivateRoadmaps(Guid userId, int page)
    {
        if (!await _repository.CheckIfIdExists(userId))
            throw new NotFound($"User with id={userId} not found");

        var roadmaps = await _accessRepository.GetPrivateRoadmaps(userId);

        return await GetPagedRoadmaps(roadmaps, page, userId);
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
            var roadmapDto = _mapper.Map<RoadmapPagedDto>(roadmap);

            if (user.Stared != null && user.Stared.Contains(roadmap.Id))
            {
                roadmapDto.IsStared = true;
            }

            return roadmapDto;
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

        if (user.Stared != null && user.Stared.Contains(roadmapId))
        {
            roadmap.StarsCount--;
            user.Stared.Remove(roadmapId);
        }
        else if (user.Stared != null)
        {
            roadmap.StarsCount++;
            user.Stared.Add(roadmapId);
        }

        await _repository.UpdateAsync(user);
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
    
}