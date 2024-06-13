using System.ComponentModel.DataAnnotations;

namespace Roadmap.Application.Dtos.Requests;

public class RegisterDto
{
    [Required] 
    [EmailAddress]
    public string Email { get; set; }
    
    [Required] 
    public string Username { get; set; }

    [Required] 
    public string Password { get; set; }
}