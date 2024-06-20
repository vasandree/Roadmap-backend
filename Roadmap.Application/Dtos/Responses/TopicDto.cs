using System.ComponentModel.DataAnnotations;
using Roadmap.Domain.Enums;

namespace Roadmap.Application.Dtos.Responses;

public class TopicDto
{
    [Required]
    public Guid Id { get; set; }
    
    [Required]
    public string Name { get; set; }
    
    [Required]
    public string Content { get; set; }
    
}