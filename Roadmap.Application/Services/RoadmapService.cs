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


    public async Task<RoadmapResponseDto> GetRoadmap(Guid roadmapId, Guid userId)
    {
        if (!await _roadmapRepository.CheckIfIdExists(roadmapId))
            throw new NotFound($"Roadmap with id={roadmapId} not found");

        var roadmap = await _roadmapRepository.GetById(roadmapId);

        if (roadmap.Status == Status.Public || roadmap.UserId == userId)
        {
            return _mapper.Map<RoadmapResponseDto>(roadmap);
        }

        if (!await _accessRepository.CheckIfUserHasAccess(roadmapId, userId))
            throw new Forbidden($"User does not have access to roadmap with id={roadmapId}");

        return _mapper.Map<RoadmapResponseDto>(roadmap);
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
            Content = String.Empty,
            Status = Status.Private,
            User = user,
        });
    }

    public async Task EditRoadmap(Guid roadmapId, RoadmapRequestDto roadmapRequestDto, Guid userId)
    {
        if (!await _roadmapRepository.CheckIfIdExists(roadmapId))
            throw new NotFound($"Roadmap with id={roadmapId} not found");

        var roadmap = await _roadmapRepository.GetById(roadmapId);

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

    public async Task<RoadmapsPagedDto> GetRoadmaps(string name, int page)
    {

        var roadmaps = await _roadmapRepository.GetPublishedRoadmaps(name);
        
        int totalRoadmapsCount = roadmaps.Count;
        var pagedRoadmaps = roadmaps.OrderByDescending(x=>x.Stared?.Count()).Skip((page - 1) * 10).Take(10).ToList();
        int totalPages = (int)Math.Ceiling((double)totalRoadmapsCount / 10);
    
        return new RoadmapsPagedDto
        {
            Roadmaps = pagedRoadmaps.Select(u => _mapper.Map<RoadmapPagedDto>(u)).ToList(),
            Pagination = new Pagination(10, totalPages, page)
        };
    }

    public async Task<RoadmapsPagedDto> GetMyRoadmaps(Guid userId, int page)
    {
        if (!await _repository.CheckIfIdExists(userId))
            throw new NotFound($"User with id={userId} not found");

        var roadmaps = await _roadmapRepository.Find(x => x.UserId == userId);
    
        int totalRoadmapsCount = roadmaps.Count;
        var pagedRoadmaps = roadmaps.OrderByDescending(x=>x.Stared?.Count()).Skip((page - 1) * 10).Take(10).ToList();

        int totalPages = (int)Math.Ceiling((double)totalRoadmapsCount / 10);
    
        return new RoadmapsPagedDto
        {
            Roadmaps = pagedRoadmaps.Select(u => _mapper.Map<RoadmapPagedDto>(u)).ToList(),
            Pagination = new Pagination(10, totalPages, page)
        };
    }


    public async Task<RoadmapsPagedDto> GetStaredRoadmaps(Guid userId, int page)
    {
        if (!await _repository.CheckIfIdExists(userId))
            throw new NotFound($"User with id={userId} not found");
        
        var roadmaps = await _staredRoadmap.GetStaredRoadmaps(userId);
    
        int totalRoadmapsCount = roadmaps.Count;
        var pagedRoadmaps = roadmaps.OrderByDescending(x=>x.Stared?.Count()).Skip((page - 1) * 10).Take(10).ToList();

        int totalPages = (int)Math.Ceiling((double)totalRoadmapsCount / 10);
    
        return new RoadmapsPagedDto
        {
            Roadmaps = pagedRoadmaps.Select(u => _mapper.Map<RoadmapPagedDto>(u)).ToList(),
            Pagination = new Pagination(10, totalPages, page)
        };
    }
}