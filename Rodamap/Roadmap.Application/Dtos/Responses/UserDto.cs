using System.ComponentModel.DataAnnotations;

namespace Roadmap.Application.Dtos.Responses;

public class UserDto
{
    [Required] 
    public string Email { get; set; }
    
    [Required] 
    public string Username { get; set; }   
}