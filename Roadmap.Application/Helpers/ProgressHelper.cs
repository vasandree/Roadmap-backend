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

    public List<Guid> GetTopicIds(JsonDocument jsonDocument)
    {
        var jsonElements = jsonDocument.RootElement.EnumerateArray();

        var nonEdgeNodes = jsonElements
            .Where(element => element.TryGetProperty("shape", out var shape) && shape.GetString() != "edge")
            .ToList();

        return nonEdgeNodes.Any()
            ? nonEdgeNodes.Select(element => Guid.Parse(element.GetProperty("id").GetString())).ToList()
            : new List<Guid>();
    }

    public async Task ChangeProgress(Guid userId, Guid roadmapId, List<Guid> deletedTopicIds, List<Guid> addedTopicIds,
        List<Guid> modifiedIds)
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

                    if (deletedTopicIds.Contains(itemId))
                    {
                        continue;
                    }

                    if (modifiedIds.Contains(itemId))
                    {
                        var modifiedItem = new
                        {
                            Id = itemId,
                            Status = ProgressStatus.ChangedByAuthor.ToString()
                        };
                        var jsonString = JsonSerializer.Serialize(modifiedItem);
                        updatedProgressItems.Add(JsonDocument.Parse(jsonString).RootElement);
                    }
                    else
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


    public List<Guid> GetModifiedTopics(List<Guid> commonTopicIds, JsonDocument oldContent, JsonDocument newContent)
    {
        var modifiedIds = new List<Guid>();

        foreach (var topicId in commonTopicIds)
        {
            var oldTopic = GetTopicDetails(oldContent, topicId);
            var newTopic = GetTopicDetails(newContent, topicId);

            if (oldTopic != null && newTopic != null)
            {
                if (oldTopic.Text != newTopic.Text || oldTopic.Data != newTopic.Data)
                {
                    modifiedIds.Add(topicId);
                }
            }
        }

        return modifiedIds;
    }

    private TopicDetails? GetTopicDetails(JsonDocument jsonDocument, Guid topicId)
    {
        var root = jsonDocument.RootElement;

        foreach (var element in root.EnumerateArray())
        {
            if (element.GetProperty("id").GetGuid() == topicId)
            {
                var text = element.GetProperty("attrs").GetProperty("text").GetProperty("text").GetString();
                var data = element.GetProperty("data").GetProperty("data").GetString();

                return new TopicDetails { Text = text, Data = data };
            }
        }

        return null;
    }

    private class TopicDetails
    {
        public string Text { get; set; }
        public string Data { get; set; }
    }
}