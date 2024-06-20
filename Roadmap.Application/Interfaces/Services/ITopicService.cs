using Roadmap.Application.Dtos.Requests;
using Roadmap.Application.Dtos.Responses;

namespace Roadmap.Application.Interfaces.Services;

public interface ITopicService
{
    Task<TopicDto> GetTopic(Guid id, Guid userId);
    Task CreateTopic(TopicDto topicDto, Guid userId, Guid roadmapId);
    Task EditTopic(Guid id, EditTopicDto editTopicDto, Guid userId);
    Task DeleteTopic(Guid id, Guid userId);
}