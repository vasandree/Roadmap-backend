using System.ComponentModel.DataAnnotations;

namespace Roadmap.Domain.Entities;

public class ExpiredToken : GenericEntity
{
    [Required]
    public string TokenString { get; set; }
    
    [Required]
    public DateTime ExpiryDate { get; set; }
}