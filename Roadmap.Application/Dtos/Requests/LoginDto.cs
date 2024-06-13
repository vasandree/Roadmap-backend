using System.ComponentModel.DataAnnotations;

namespace Roadmap.Application.Dtos.Requests;

public class LoginDto
{ 
    [Required] 
    public string Username { get; set; }

    [Required] 
    public string Password { get; set; }
}