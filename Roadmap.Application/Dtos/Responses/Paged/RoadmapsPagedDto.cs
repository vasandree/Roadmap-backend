using System.ComponentModel.DataAnnotations;

namespace Roadmap.Application.Dtos.Responses.Paged;

public class RoadmapsPagedDto
{
    [Required]
    public List<RoadmapPagedDto> Roadmaps { get; set; }
    
    [Required]
    public Pagination Pagination { get; set; }
}