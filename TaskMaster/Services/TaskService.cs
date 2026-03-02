using Microsoft.EntityFrameworkCore;
using TaskMaster.Data;
using TaskMaster.Models;
using TaskMaster.Models.Enums;

namespace TaskMaster.Services;

public class TaskService : ITaskService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<TaskService> _logger;

    public TaskService(ApplicationDbContext dbContext, ILogger<TaskService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<TaskOperationResult> GetTaskByIdAsync(int id)
    {
        var task = await _dbContext.Tasks.FindAsync(id);
        if (task == null)
        {
            _logger.LogWarning("Task with ID {TaskId} not found", id);
            return new TaskOperationResult { TaskNotFound = true, Success = false };
        }

        _logger.LogInformation("Retrieved task with ID {TaskId}", id);
        return new TaskOperationResult { Task = task, TaskNotFound = false, Success = true };
    }

    public async Task<IEnumerable<TaskItem>?> GetAllTasksAsync()
    {
        var taskEntities = await _dbContext.Tasks
           .AsNoTracking()
           .ToListAsync();

        if (!taskEntities.Any())
        {
            _logger.LogInformation("No tasks found");
            return null;
        }

        _logger.LogInformation("Retrieved {TaskCount} tasks", taskEntities.Count);
        return taskEntities;
    }

    public async Task<Project?> GetProjectByTaskAsync(int id)
    {
        var task = await _dbContext.Tasks
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (task == null)
        {
            _logger.LogWarning("Task with ID {TaskId} not found when retrieving its project", id);
            return null;
        }

        if (task.Project == null)
        {
            _logger.LogWarning("Task with ID {TaskId} has no associated project", id);
            return null;
        }

        _logger.LogInformation("Retrieved project with ID {ProjectId} for task with ID {TaskId}", task.Project.Id, id);
        return task.Project;
    }

    public async Task<TaskOperationResult> CreateTaskAsync(TaskItem task)
    {
        if (task == null)
        {
            _logger.LogWarning("Attempted to create a task with null data");
            return new TaskOperationResult { Task = null, Success = false, TaskNotFound = false };
        }

        bool taskExists = await _dbContext.Tasks
            .AnyAsync(t => t.ProjectId == task.ProjectId &&
                           t.Title.ToLower() == task.Title.ToLower());

        if (taskExists)
        {
            _logger.LogWarning("A task with the title '{TaskTitle}' already exists in project {ProjectId}", task.Title, task.ProjectId);
            return new TaskOperationResult { Task = null, Success = false, TaskExists = true };
        }

        if (!await _dbContext.Projects.AnyAsync(p => p.Id == task.ProjectId))
        {
            _logger.LogWarning("Project with ID {ProjectId} not found when creating task", task.ProjectId);
            return new TaskOperationResult { Task = null, Success = false, TaskNotFound = true };
        }

        var newTask = new TaskItem
        {
            Title = task.Title,
            Description = task.Description,
            Created = DateTime.UtcNow,
            DueDate = task.DueDate,
            ProjectId = task.ProjectId,
            Status = task.Status
        };

        _dbContext.Add(newTask);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Successfully created task with ID {TaskId} in project {ProjectId}", newTask.Id, newTask.ProjectId);
        return new TaskOperationResult { Task = newTask, Success = true };
    }

    public async Task<TaskOperationResult> UpdateTaskStatusAsync(int taskId, TaskItemStatus newStatus)
    {
        var task = await _dbContext.Tasks.FindAsync(taskId);
        if (task == null)
        {
            _logger.LogWarning("Task with ID {TaskId} not found when updating status", taskId);
            return new TaskOperationResult { Task = null, Success = false, TaskNotFound = true };
        }

        if (!IsValidStatusTransition(task.Status, newStatus))
        {
            _logger.LogWarning("Invalid status transition for task {TaskId}: {CurrentStatus} -> {NewStatus}", taskId, task.Status, newStatus);
            return new TaskOperationResult { Task = null, Success = false, InvalidTransition = true };
        }

        var previousStatus = task.Status;
        task.Status = newStatus;
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Updated task {TaskId} status from {PreviousStatus} to {NewStatus}", taskId, previousStatus, newStatus);
        return new TaskOperationResult { Task = task, Success = true };
    }

    private bool IsValidStatusTransition(TaskItemStatus currentStatus, TaskItemStatus newStatus)
    {
        if (currentStatus == newStatus)
            return true;

        return (currentStatus, newStatus) switch
        {
            (TaskItemStatus.Analysis, TaskItemStatus.WaitingForDev) => true,
            (TaskItemStatus.WaitingForDev, TaskItemStatus.InDev) => true,
            (TaskItemStatus.InDev, TaskItemStatus.WaitingForReview) => true,
            (TaskItemStatus.WaitingForReview, TaskItemStatus.InReview) => true,
            (TaskItemStatus.InReview, TaskItemStatus.WaitingForTesting) => true,
            (TaskItemStatus.WaitingForTesting, TaskItemStatus.InTesting) => true,
            (TaskItemStatus.InTesting, TaskItemStatus.Done) => true,
            _ => false
        };
    }

    public async Task<TaskOperationResult> AssignTaskAsync(int taskId, int projectId)
    {
        var (task, failure) = await FindTaskAsync(taskId);
        if (failure)
        {
            _logger.LogWarning("Task with ID {TaskId} not found when assigning to project {ProjectId}", taskId, projectId);
            return new TaskOperationResult { Task = null, Success = false, TaskNotFound = true };
        }

        var previousProjectId = task!.ProjectId;
        task!.ProjectId = projectId;
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Assigned task {TaskId} from project {PreviousProjectId} to project {ProjectId}", taskId, previousProjectId, projectId);
        return new TaskOperationResult { Task = task, Success = true };
    }

    public async Task<TaskOperationResult> DeleteTaskAsync(int taskId)
    {
        var (task, failure) = await FindTaskAsync(taskId);
        if (failure)
        {
            _logger.LogWarning("Task with ID {TaskId} not found when attempting deletion", taskId);
            return new TaskOperationResult { Task = null, Success = false, TaskNotFound = true };
        }

        _dbContext.Tasks.Remove(task!);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Deleted task with ID {TaskId}", taskId);
        return new TaskOperationResult { Task = task, Success = true };
    }
    private async Task<(TaskItem? Task, bool Failure)> FindTaskAsync(int taskId)
    {
        var task = await _dbContext.Tasks.FindAsync(taskId);
        if (task == null) return (default, true);
        return (task, false);
    }
}
