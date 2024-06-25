using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Nodes;
using Roadmap.Domain.Enums;

namespace Roadmap.Application.Dtos.Responses;

public class RoadmapResponseDto
{
    [Required]
    public Guid Id { get; set; }
    
    [Required]
    public string Name { get; set; }
    
    [Required]
    public string Description { get; set; }
    
    [Required]
    public UserDto User { get; set; }
    
    [Required]
    public Status Status { get; set; } 
    
    [Required]
    public int StarsCount { get; set; }
    
    [Required]
    public int TopicsCount { get; set; }
    
    [Required]
    public int TopicsClosed { get; set; }
    
    [Required]
    public bool IsStared { get; set; }
    
    public JsonDocument? Progress { get; set; }
    
    public JsonDocument? Content { get; set; }
}