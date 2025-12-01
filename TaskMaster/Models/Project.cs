namespace TaskMaster.Models;

public class Project
{
    public int Id { get; set; }
    
    public required string Name { get; init; }
    
    public string? Description { get; set; }
    
    public DateTime Created { get; set; } = DateTime.UtcNow;
    
    public List<TaskItem>? Tasks { get; set; }
}
