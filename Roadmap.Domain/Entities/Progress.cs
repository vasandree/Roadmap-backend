using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Roadmap.Domain.Entities;

public class Progress : GenericEntity
{
    [Required]
    [ForeignKey("Roadmap")] 
    public Guid RoadmapId { get; set; }
    
    [Required]
    [ForeignKey("User")] 
    public Guid UserId { get; set; }
    
    [Required]
    public JsonDocument UsersProgress { get; set; }
    
    [Required]
    public User User { get; set; }
    
    [Required]
    public Roadmap Roadmap { get; set; }
}