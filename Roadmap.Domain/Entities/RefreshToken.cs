using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Roadmap.Domain.Entities;

public class RefreshToken : GenericEntity
{
    [Required] 
    public string TokenString { get; set; }

    [Required] 
    [ForeignKey("User")] 
    public Guid UserId { get; set; }
    
    [Required]
    public DateTime ExpiryDate { get; set; }
    
    [Required]
    public User User { get; set; }
}