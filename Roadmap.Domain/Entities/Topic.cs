using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Roadmap.Domain.Entities;

public class Topic : GenericEntity
{
    [Required] 
    [ForeignKey("Roadmap")] 
    public Guid RoadmapId { get; set; }
    
    [Required]
    public Guid CreatorId { get; set; }
    
    [Required]
    public string Name { get; set; }
    
    [Required]
    public string Content { get; set; }
    
    [Required]
    public Roadmap Roadmap { get; set; }

}