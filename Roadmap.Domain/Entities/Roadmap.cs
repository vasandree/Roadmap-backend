using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Roadmap.Domain.Enums;

namespace Roadmap.Domain.Entities;

public class Roadmap : GenericEntity
{
    
    [Required] 
    [ForeignKey("User")] 
    public Guid UserId { get; set; }
    
    [Required]
    public string Name { get; set; }
    
    [Required]
    public string Description { get; set; }
    
    [Required]
    public JsonDocument? Content { get; set; }
    
    [Required]
    public Status Status { get; set; }
    
    [Required]
    public User User { get; set; }
    
    public IEnumerable<PrivateAccess>? PrivateAccesses { get; set; }
    public IEnumerable<StaredRoadmap>? Stared{ get; set; }
}