using System.ComponentModel.DataAnnotations;

namespace TaskMaster.Models.DTO;

public class CreateTaskItemDto
{
    
    [Required]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Task has to have a name")]
    public required string Title { get; set; }

    [StringLength(100)]
    public string? Description { get; set; }
    
    public DateTime? DueDate { get; set; }
    
    [Required]
    public int ProjectId { get; set; } 
}