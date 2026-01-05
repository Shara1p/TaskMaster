using TaskMaster.Models;

public class ProjectOperationResult
{
    public Project? Project { get; set; }
    public bool Success { get; set; }
    public bool ProjectNotFound { get; set; }
    public bool HasTasks { get; set; }
    public bool ProjectExists { get; set; }
}
