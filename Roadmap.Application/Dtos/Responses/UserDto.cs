using System.ComponentModel.DataAnnotations;

namespace Roadmap.Application.Dtos.Responses;

public class UserDto
{
    
    [Required]
    public Guid Id { get; set; }
    
    [Required] 
    public string Email { get; set; }
    
    [Required] 
    public string Username { get; set; }   
}