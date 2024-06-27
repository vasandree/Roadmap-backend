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
        IPrivateAccessRepository accessRepository, IUserRepository repository, IProgressRepository progressRepository,
        ProgressHelper progressHelper)
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

            if (roadmap.Status != Status.Public && roadmap.UserId != userId.Value)
            {
                if (!await _accessRepository.CheckIfUserHasAccess(roadmapId, userId.Value))
                    throw new Forbidden($"User does not have access to roadmap with id={roadmapId}");
            }

            var dto = _mapper.Map<RoadmapResponseDto>(roadmap);

            if (user.Stared != null && user.Stared.Contains(roadmap.Id))
                dto.IsStared = true;

            await AddRecentlyVisited(user, roadmapId);

            if (roadmap.Content == null) return dto;
            if (!await _progressRepository.CheckIfExists(user.Id, roadmapId))
                await CreateProgress(user, roadmap);

            var progress = await _progressRepository.GetByUserAndRoadmap(user.Id, roadmap.Id);
            
            await _progressRepository.UpdateAsync(progress);
            
            dto.Progress = progress.UsersProgress;
            dto.TopicsClosed = _progressHelper.CountClosed(progress.UsersProgress);
            
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
            var topicsIds = _progressHelper.GetTopicIds(roadmap.Content);

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

    public async Task<Guid> CreateRoadMap(RoadmapRequestDto roadmapRequestDto, Guid userId)
    {
        if (!await _repository.CheckIfIdExists(userId))
            throw new NotFound("User does not exist");

        var user = await _repository.GetById(userId);

        var newRoadmap = new Domain.Entities.Roadmap
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = roadmapRequestDto.Name,
            Description = roadmapRequestDto.Description,
            Content = { },
            Status = Status.Private,
            StarsCount = 0,
            TopicsCount = 0,
            User = user,
            CreationTime = DateTime.UtcNow,
            PrivateAccesses = new List<PrivateAccess>()
        };
        
        await _roadmapRepository.CreateAsync(newRoadmap);
        
        return newRoadmap.Id;
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
            var newTopicIds = _progressHelper.GetTopicIds(jsonContent);

            if (roadmap.Content != null)
            {
                var oldTopicsIds = _progressHelper.GetTopicIds(roadmap.Content);
                var commonTopicIds = oldTopicsIds.Intersect(newTopicIds).ToList();

                var modifiedIds = _progressHelper.GetModifiedTopics(commonTopicIds, roadmap.Content, jsonContent);

                var deletedTopicIds = oldTopicsIds.Except(newTopicIds).ToList();
                var addedTopicIds = newTopicIds.Except(oldTopicsIds).ToList();

                await _progressHelper.ChangeProgress(userId, roadmapId, deletedTopicIds, addedTopicIds, modifiedIds);
            }

            roadmap.TopicsCount = newTopicIds.Count;
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

        var roadmapIds = user.Stared?.ToHashSet();

        var existingRoadmaps = new List<Guid>();

        if (roadmapIds == null || !roadmapIds.Any())
            return new RoadmapsPagedDto();

        foreach (var id in roadmapIds)
        {
            if (await _roadmapRepository.CheckIfIdExists(id))
                existingRoadmaps.Add(id);
        }

        user.Stared = existingRoadmaps;
        await _repository.UpdateAsync(user);


        var roadmaps = await _roadmapRepository.GetRoadmapsByIds(existingRoadmaps.ToList());


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
        var roadmapsIds = user.RecentlyVisited;

        if (roadmapsIds == null || !roadmapsIds.Any())
            return new RoadmapsPagedDto();

        var existingRoadmaps = new List<Guid>();

        foreach (var id in roadmapsIds)
        {
            if (await _roadmapRepository.CheckIfIdExists(id))
                existingRoadmaps.Add(id);
        }

        user.RecentlyVisited = existingRoadmaps;
        await _repository.UpdateAsync(user);

        var roadmaps = await _roadmapRepository.GetRoadmapsByIds(existingRoadmaps);
        
        return await GetPagedRoadmaps(roadmaps, 1, user.Id);
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
            roadmap.StarsCount--;
        }
        else if (user.Stared != null)
        {
            roadmap.StarsCount++;
            user.Stared.Add(roadmapId);
        }

        await _roadmapRepository.UpdateAsync(roadmap);
        await _repository.UpdateAsync(user);
    }

    public async Task<Guid> CopyRoadmap(Guid userId, Guid roadmapId)
    {
        if (!await _roadmapRepository.CheckIfIdExists(roadmapId))
            throw new NotFound($"Roadmap with id={roadmapId} not found");

        var roadmap = await _roadmapRepository.GetById(roadmapId);

        if (!await _repository.CheckIfIdExists(userId))
            throw new NotFound("User does not exist");

        var user = await _repository.GetById(userId);

        if (roadmap.Status != Status.Public && roadmap.UserId != userId)
            throw new BadRequest("Roadmap is not published");


        var newRoadmap = new Domain.Entities.Roadmap
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = $"{roadmap.Name} (копия)",
            Description = roadmap.Description,
            Content = roadmap.Content,
            Status = Status.Private,
            StarsCount = 0,
            TopicsCount = roadmap.TopicsCount,
            User = user,
            CreationTime = DateTime.UtcNow,
            PrivateAccesses = new List<PrivateAccess>()
        };

        await _roadmapRepository.CreateAsync(newRoadmap);

        return newRoadmap.Id;
    }

    public async Task<RoadmapsPagedDto> GetUsersRoadmaps(Guid? userId, Guid roadmapUserId, int page)
    {
        if (userId.HasValue)
        {
            if (!await _repository.CheckIfIdExists(userId.Value))
                throw new NotFound("User does not exist");

            if (userId.Value == roadmapUserId)
                throw new BadRequest("You cannot view your own roadmaps.");
        }
    
        if (!await _repository.CheckIfIdExists(roadmapUserId))
            throw new NotFound("User does not exist");

        var roadmapUser = await _repository.GetById(roadmapUserId);

        if (roadmapUser.CreatedRoadmaps != null)
        {
            var publicRoadmaps = roadmapUser.CreatedRoadmaps
                .Where(x => x.Status == Status.Public)
                .ToList();

            return await GetPagedRoadmaps(publicRoadmaps, page, userId);
        }

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

                if (await _progressRepository.CheckIfExists(user.Id, roadmap.Id))
                {
                    var progress = await _progressRepository.GetByUserAndRoadmap(user.Id, roadmap.Id);

                    roadmapDto.TopicsClosed = _progressHelper.CountClosed(progress.UsersProgress);
                }
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
            user.RecentlyVisited = new List<Guid>();
        }

        if (user.RecentlyVisited.Contains(roadmapId))
        {
            user.RecentlyVisited.Remove(roadmapId);
        }

        user.RecentlyVisited.Insert(0, roadmapId);

        if (user.RecentlyVisited.Count > 5)
        {
            user.RecentlyVisited.RemoveAt(user.RecentlyVisited.Count - 1);
        }

        await _repository.UpdateAsync(user);
    }
}