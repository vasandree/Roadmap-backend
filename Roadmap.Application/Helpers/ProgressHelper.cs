using System.Text.Json;
using Roadmap.Application.Interfaces.Repositories;
using Roadmap.Domain.Enums;

namespace Roadmap.Application.Helpers;

public class ProgressHelper
{
    private readonly IProgressRepository _progressRepository;

    public ProgressHelper(IProgressRepository progressRepository)
    {
        _progressRepository = progressRepository;
    }

    public List<Guid> GetTopics(JsonDocument jsonDocument)
    {
        var jsonElements = jsonDocument.RootElement.EnumerateArray();

        var nonEdgeNodes = jsonElements
            .Where(element => element.TryGetProperty("shape", out var shape) && shape.GetString() != "edge")
            .ToList();

        return nonEdgeNodes.Any()
            ? nonEdgeNodes.Select(element => Guid.Parse(element.GetProperty("id").GetString())).ToList()
            : new List<Guid>();
    }

    public async Task ChangeProgress(Guid userId, Guid roadmapId, List<Guid> deletedTopicIds, List<Guid> addedTopicIds)
    {
        var progressesToChange = await _progressRepository.Find(x => x.UserId == userId && x.RoadmapId == roadmapId);
        var progresses = progressesToChange.ToList();

        foreach (var progress in progresses)
        {
            if (progress.UsersProgress != null)
            {
                var progressItems = progress.UsersProgress.RootElement.EnumerateArray().ToList();
                var updatedProgressItems = new List<JsonElement>();

                foreach (var item in progressItems)
                {
                    var itemId = item.GetProperty("Id").GetGuid();
                    if (!deletedTopicIds.Contains(itemId))
                    {
                        updatedProgressItems.Add(item);
                    }
                }

                foreach (var addedTopicId in addedTopicIds)
                {
                    var newProgressItem = new
                    {
                        Id = addedTopicId,
                        Status = ProgressStatus.Pending.ToString()
                    };
                    var jsonString = JsonSerializer.Serialize(newProgressItem);
                    updatedProgressItems.Add(JsonDocument.Parse(jsonString).RootElement);
                }

                var updatedJson = JsonSerializer.Serialize(updatedProgressItems);
                progress.UsersProgress = JsonDocument.Parse(updatedJson);

                await _progressRepository.UpdateAsync(progress);
            }
        }
    }

}