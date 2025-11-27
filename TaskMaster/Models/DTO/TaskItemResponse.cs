namespace TaskMaster.Models.DTO;

public class TaskItemResponse
{
    public required string Title { get; set; }
    
    public string? Description { get; set; }
    
    public required DateTime Created { get; set; }
    
    public string Status { get; set; } = null!;
    public DateTime? DueDate { get; set; }
    
    public int ProjectId { get; set; }
}
