using System.ComponentModel.DataAnnotations;

namespace Roadmap.Domain.Entities;

public class ExpiredToken
{
    [Key]
    [Required]
    public Guid Id { get; set; }
    
    [Required]
    public string TokenString { get; set; }
}