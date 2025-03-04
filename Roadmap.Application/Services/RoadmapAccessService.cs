using AutoMapper;
using Common.Exceptions;
using Roadmap.Application.Dtos.Responses;
using Roadmap.Application.Interfaces.Repositories;
using Roadmap.Application.Interfaces.Services;
using Roadmap.Domain.Entities;
using Roadmap.Domain.Enums;

namespace Roadmap.Application.Services;

public class RoadmapAccessService : IRoadmapAccessService
{
    private readonly IRoadmapRepository _roadmapRepository;
    private readonly IPrivateAccessRepository _accessRepository;
    private readonly IUserRepository _repository;
    private readonly IMapper _mapper;

    public RoadmapAccessService(IRoadmapRepository roadmapRepository, IUserRepository repository,
        IPrivateAccessRepository accessRepository, IMapper mapper)
    {
        _roadmapRepository = roadmapRepository;
        _repository = repository;
        _accessRepository = accessRepository;
        _mapper = mapper;
    }

    public async Task PublishRoadmap(Guid userId, Guid roadmapId)
    {
        if (!await _roadmapRepository.CheckIfIdExists(roadmapId))
            throw new NotFound($"Roadmap with id={roadmapId} not found");

        var roadmap = await _roadmapRepository.GetById(roadmapId);

        if (roadmap.Status == Status.Public)
            throw new BadRequest("Roadmap is already published");

        if (roadmap.Content == null)
            throw new BadRequest("You cannot publish roadmap with no content");
        
        if (!await _repository.CheckIfIdExists(userId))
            throw new NotFound("User does not exist");

        if (roadmap.UserId != userId)
            throw new Forbidden($"User is not a creator of roadmap with id={roadmapId}");

        roadmap.Status = Status.Public;
        if (roadmap.PrivateAccesses != null)
        {
            _roadmapRepository.RemovePrivateAccess(roadmap.PrivateAccesses);
        }
        
        await _roadmapRepository.UpdateAsync(roadmap);
    }

    public async Task AddPrivateAccess(Guid creatorId, Guid[] userIds, Guid roadmapId)
    {
        if (!await _roadmapRepository.CheckIfIdExists(roadmapId))
            throw new NotFound($"Roadmap with id={roadmapId} not found");

        var roadmap = await _roadmapRepository.GetById(roadmapId);

        if (roadmap.Content == null)
            throw new BadRequest("You cannot add users to roadmap with no content");
        
        if (roadmap.Status == Status.Public)
            throw new BadRequest("Roadmap is published. You can't edit it");
        
        if (!await _repository.CheckIfIdExists(creatorId))
            throw new NotFound("User does not exist");

        if (roadmap.UserId != creatorId)
            throw new Forbidden($"User is not a creator of roadmap with id={roadmapId}");

        foreach (var id in userIds)
        {
            
            if (!await _repository.CheckIfIdExists(id))
                throw new NotFound("User does not exist");

            if (await _accessRepository.CheckIfUserHasAccess(roadmapId, id))
                throw new BadRequest($"User with id={id} already has access to this roadmap");
            
            if (roadmap.UserId == id)
                throw new BadRequest("You cannot add yourself");
        }
        
        foreach (var id in userIds)
        {
            await _accessRepository.CreateAsync(new PrivateAccess
            {
                Id = Guid.NewGuid(),
                RoadmapId = roadmapId,
                UserId = id,
                User = await _repository.GetById(id),
                Roadmap = roadmap
            });
        }

        roadmap.Status = Status.PrivateSharing;
        await _roadmapRepository.UpdateAsync(roadmap);
    }

    public async Task RemovePrivateAccess(Guid creatorId, Guid[] userIds, Guid roadmapId)
    {
        if (!await _roadmapRepository.CheckIfIdExists(roadmapId))
            throw new NotFound($"Roadmap with id={roadmapId} not found");

        var roadmap = await _roadmapRepository.GetById(roadmapId);

        if (roadmap.Status == Status.Public)
            throw new BadRequest("Roadmap is published. You can't edit it");
        
        if (!await _repository.CheckIfIdExists(creatorId))
            throw new NotFound("User does not exist");

        if (roadmap.UserId != creatorId)
            throw new Forbidden($"User is not a creator of roadmap with id={roadmapId}");

        if (roadmap.PrivateAccesses == null)
        {
            throw new BadRequest("Roadmap does not have private access");
        }

        foreach (var id in userIds)
        {
            if (!await _repository.CheckIfIdExists(id))
                throw new NotFound("User does not exist");

            if (!await _accessRepository.CheckIfUserHasAccess(roadmapId, id))
                throw new BadRequest($"User with id={id} does not have private access");
        }

        foreach (var id in userIds)
        {
            await _accessRepository.DeleteAsync(await _accessRepository.GetByUserAndRoadmap(id, roadmapId));
        }

        if (!roadmap.PrivateAccesses.Any())
        {
            roadmap.Status = Status.Private;
            await _roadmapRepository.UpdateAsync(roadmap);
        }
    }

    public async Task<List<UserDto>> GetPrivateUsers(Guid userId, Guid roadmapId, string? name)
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

        var users = await _accessRepository.GetUsers(roadmapId, name);
        
        return  users.Take(10).Select(u => _mapper.Map<UserDto>(u)).ToList();
    }
}