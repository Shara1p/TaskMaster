namespace TaskMaster.Models;

public class TaskOperationResult
{
    public TaskItem? Task { get; set; }
    public bool Success { get; set; }
    public bool TaskNotFound { get; set; }
    public bool InvalidTransition { get; set; }
    public bool TaskExists {get; set; }
}
