using System.Text.Json;
using System.Text.Json.Nodes;
using Common.Exceptions;
using Roadmap.Application.Helpers;
using Roadmap.Application.Interfaces.Repositories;
using Roadmap.Application.Interfaces.Services;
using Roadmap.Domain.Enums;

namespace Roadmap.Application.Services;

public class ProgressService : IProgressService
{
    private readonly IUserRepository _repository;
    private readonly ProgressHelper _progressHelper;
    private readonly IRoadmapRepository _roadmapRepository;
    private readonly IProgressRepository _progressRepository;
    private readonly IPrivateAccessRepository _accessRepository;


    public ProgressService(IUserRepository repository, IRoadmapRepository roadmapRepository,
        IProgressRepository progressRepository, IPrivateAccessRepository accessRepository,
        ProgressHelper progressHelper)
    {
        _repository = repository;
        _roadmapRepository = roadmapRepository;
        _progressRepository = progressRepository;
        _accessRepository = accessRepository;
        _progressHelper = progressHelper;
    }

    public async Task ChangeProgress(Guid userId, Guid roadmapId, Guid topicId, ProgressStatus progressStatus)
    {
        if (progressStatus == ProgressStatus.ChangedByAuthor)
            throw new NotFound("You can't change topic status to ChangedByAuthor");

        if (!await _repository.CheckIfIdExists(userId))
            throw new NotFound("User does not exist");

        if (!await _roadmapRepository.CheckIfIdExists(roadmapId))
            throw new NotFound($"Roadmap with id={roadmapId} not found");

        var roadmap = await _roadmapRepository.GetById(roadmapId);

        if (roadmap.Status != Status.Public || roadmap.UserId != userId)
        {
            if (!await _accessRepository.CheckIfUserHasAccess(roadmapId, userId))
                throw new Forbidden("You do not have access to this roadmap");
        }

        var topicIds = _progressHelper.GetTopics(roadmap.Content);

        if (!topicIds.Contains(topicId))
            throw new NotFound($"Topic with id={topicId} not found in the roadmap");

        var userProgress = await _progressRepository.GetByUserAndRoadmap(userId, roadmapId);

        if (userProgress == null)
            throw new NotFound("User progress not found for the specified roadmap");

        var progressItems = userProgress.UsersProgress.RootElement.EnumerateArray().ToList();

        var updatedProgressItems = new List<JsonElement>();

        foreach (var item in progressItems)
        {
            var itemId = item.GetProperty("Id").GetGuid();
            if (itemId == topicId)
            {
                var jsonObject = item.EnumerateObject().ToDictionary(p => p.Name, p => p.Value);

                jsonObject["Status"] = JsonDocument.Parse($"\"{progressStatus.ToString()}\"").RootElement;

                var jsonString = JsonSerializer.Serialize(jsonObject);
                updatedProgressItems.Add(JsonDocument.Parse(jsonString).RootElement);
            }
            else
            {
                updatedProgressItems.Add(item);
            }
        }

        var updatedJson = JsonSerializer.Serialize(updatedProgressItems);
        userProgress.UsersProgress = JsonDocument.Parse(updatedJson);

        await _progressRepository.UpdateAsync(userProgress);
    }
}