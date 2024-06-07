using System.ComponentModel.DataAnnotations;

namespace Roadmap.Domain.Entities;

public class User
{
    [Key]
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    public string Email { get; set; }
    
    [Required]
    public string Username { get; set; }
    
    [Required]
    public string Password { get; set; }

    public ICollection<RefreshToken?> RefreshTokens { get; set; }
}