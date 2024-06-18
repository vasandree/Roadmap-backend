using System.ComponentModel.DataAnnotations;

namespace Roadmap.Application.Dtos.Requests;

public class RoadmapRequestDto
{
    [Required]
    public string Name { get; set; }
    
    [Required]
    public string Description { get; set; }
}