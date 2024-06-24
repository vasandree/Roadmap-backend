using System.Text.Json;

namespace Roadmap.Application.Helpers;

public class ProgressHelper
{
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
}