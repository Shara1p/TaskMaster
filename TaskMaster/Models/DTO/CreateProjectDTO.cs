using System.ComponentModel.DataAnnotations;

namespace TaskMaster.Models.DTO;

public class CreateProjectDto
{
    [Required]
    [StringLength(100, MinimumLength = 10, ErrorMessage = "Project name must be between 10 and 100 characters")]
    public required string Name { get; set; }
    
    [StringLength(100, ErrorMessage = "Project description must be not bigger than 100 characters")]
    public string? Description { get; set; }
}
