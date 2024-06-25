using System.ComponentModel.DataAnnotations;

namespace Roadmap.Application.Dtos.Requests;

public class EditProfileDto
{
    [Required] 
    [EmailAddress]
    public string Email { get; set; }
    
    [Required] 
    [MinLength(6)]
    [MaxLength(30)]
    public string Username { get; set; }   
}