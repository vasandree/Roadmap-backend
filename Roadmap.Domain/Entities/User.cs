using System.ComponentModel.DataAnnotations;

namespace Roadmap.Domain.Entities;

public class User : GenericEntity
{
    [Required]
    public string Email { get; set; }
    
    [Required]
    public string Username { get; set; }
    
    [Required]
    public string Password { get; set; }

    public IEnumerable<RefreshToken>? RefreshTokens { get; set; }
    public IEnumerable<Roadmap>? CreatedRoadmaps { get; set; }
    public IEnumerable<PrivateAccess>? PrivateAccesses { get; set; }
    public IEnumerable<StaredRoadmap>? StaredRoadmaps { get; set; }
    
    public List<Guid>? RecentlyVisited { get; set; }
}