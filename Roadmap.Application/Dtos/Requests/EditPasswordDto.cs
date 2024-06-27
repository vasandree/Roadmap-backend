using System.ComponentModel.DataAnnotations;

namespace Roadmap.Application.Dtos.Requests;

public class EditPasswordDto
{
    [Required]
    [MinLength(6)]
    [MaxLength(30)]
    public string OldPassword { get; set; }
    
    [Required]
    [MinLength(6)]
    [MaxLength(30)]
    public string NewPassword { get; set; }
}