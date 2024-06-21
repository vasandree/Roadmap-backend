using AutoMapper;
using Common.Exceptions;
using Roadmap.Application.Dtos.Requests;
using Roadmap.Application.Dtos.Responses;
using Roadmap.Application.Interfaces.Repositories;
using Roadmap.Application.Interfaces.Services;
using Roadmap.Domain.Entities;

namespace Roadmap.Application.Services;

public class TopicService : ITopicService
{
    private readonly IUserRepository _repository;
    private readonly ITopicRepository _topic;
    private readonly IRoadmapRepository _roadmap;
    private readonly IMapper _mapper;

    public TopicService(IUserRepository repository, ITopicRepository topic, IMapper mapper, IRoadmapRepository roadmap)
    {
        _repository = repository;
        _topic = topic;
        _mapper = mapper;
        _roadmap = roadmap;
    }


    public async Task<TopicDto> GetTopic(Guid id, Guid userId)
    {
        if (!await _repository.CheckIfIdExists(userId))
            throw new NotFound("User does not exist");

        var topic = await _topic.GetById(id);


        return _mapper.Map<TopicDto>(topic);
    }

    public async Task CreateTopic(TopicDto topicDto, Guid userId, Guid roadmapId)
    {
        if (!await _repository.CheckIfIdExists(userId))
            throw new NotFound("User does not exist");

        await _topic.CreateAsync(new Topic
        {
            Id = topicDto.Id,
            RoadmapId = roadmapId,
            CreatorId = userId,
            Name = topicDto.Name,
            Content = topicDto.Content,
            Roadmap = await _roadmap.GetById(roadmapId)
        });
    }

    public async Task EditTopic(Guid id, EditTopicDto editTopicDto, Guid userId)
    {
        if (!await _repository.CheckIfIdExists(userId))
            throw new NotFound("User does not exist");

        var topic = await _topic.GetById(id);

        if (topic.CreatorId != userId)
            throw new Forbidden("You are not a creator of this topic");

        topic.Content = editTopicDto.Content;
        topic.Name = editTopicDto.Name;

        await _topic.UpdateAsync(topic);
    }

    public async Task DeleteTopic(Guid id, Guid userId)
    {
        if (!await _repository.CheckIfIdExists(userId))
            throw new NotFound("User does not exist");

        var topic = await _topic.GetById(id);

        if (topic.CreatorId != userId)
            throw new Forbidden("You are not a creator of this topic");

        await _topic.DeleteAsync(topic);
    }
}