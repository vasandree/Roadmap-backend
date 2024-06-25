using System.ComponentModel.DataAnnotations;

namespace Roadmap.Application.Dtos.Requests;

public class RoadmapRequestDto
{
    [Required]
    [MinLength(1)]
    [MaxLength(50)]
    public string Name { get; set; }
    
    [MinLength(0)]
    [MaxLength(80)]
    public string? Description { get; set; }
}