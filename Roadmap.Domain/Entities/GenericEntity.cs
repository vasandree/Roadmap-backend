using System.ComponentModel.DataAnnotations;

namespace Roadmap.Domain.Entities;

public class GenericEntity
{
    [Key]
    [Required]
    public Guid Id { get; set; }
}