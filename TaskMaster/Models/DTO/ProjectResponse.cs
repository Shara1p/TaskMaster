namespace TaskMaster.Models.DTO;

public class ProjectResponse
{
    public required string Name { get; set; }
    
    public string? Description { get; set; }
    
    public DateTime? Created { get; set; }
    
    public List<TaskItem>? Tasks { get; set; }
}
