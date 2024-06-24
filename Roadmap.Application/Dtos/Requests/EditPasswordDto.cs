using System.ComponentModel.DataAnnotations;

namespace Roadmap.Application.Dtos.Requests;

public class EditPasswordDto
{
    [Required]
    [MinLength(6)]
    public string OldPassword { get; set; }
    
    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; }
}