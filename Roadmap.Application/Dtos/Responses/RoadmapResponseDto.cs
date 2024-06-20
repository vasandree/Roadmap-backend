using System.ComponentModel.DataAnnotations;
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
    public string Content { get; set; }
    
    [Required]
    public bool IsStared { get; set; }
}