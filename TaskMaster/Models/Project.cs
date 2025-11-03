namespace TaskMaster.Models;

public class Project
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required DateTime Created { get; set; }
    public List<TaskItem>? Tasks { get; set; }
}
