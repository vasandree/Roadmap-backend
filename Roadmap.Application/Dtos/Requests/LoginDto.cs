using System.ComponentModel.DataAnnotations;

namespace Roadmap.Application.Dtos.Requests;

public class LoginDto
{ 
    [Required] 
    [MinLength(6)]
    [MaxLength(30)]
    public string Username { get; set; }

    [Required] 
    [MinLength(6)]
    [MaxLength(30)]
    public string Password { get; set; }
}