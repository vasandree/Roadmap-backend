using System.ComponentModel.DataAnnotations;

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
    public int StarsCount { get; set; }
    
    [Required]
    public string Content { get; set; }
    
    [Required]
    public bool IsStared { get; set; }
}