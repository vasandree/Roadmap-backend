using System.ComponentModel.DataAnnotations;

namespace Roadmap.Application.Dtos.Requests;

public class EditTopicDto
{
    [Required]
    public string Name { get; set; }
    
    [Required]
    public string Content { get; set; }
}