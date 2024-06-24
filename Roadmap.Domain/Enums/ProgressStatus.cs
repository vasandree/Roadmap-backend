using System.Text.Json.Serialization;

namespace Roadmap.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]

public enum ProgressStatus
{
    Pending,
    InProgress,
    Closed,
    ChangedByAuthor
}