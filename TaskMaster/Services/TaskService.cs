using Microsoft.EntityFrameworkCore;
using TaskMaster.Data;
using TaskMaster.Models;
using TaskMaster.Models.Enums;

namespace TaskMaster.Services;

public class TaskService : ITaskService
{
    private readonly ApplicationDbContext _dbContext;

    public TaskService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TaskOperationResult> GetTaskByIdAsync(int id)
    {
        var task = await _dbContext.Tasks.FindAsync(id);
        if (task == null) return new TaskOperationResult { TaskNotFound = true, Success = false };

        return new TaskOperationResult { Task = task, TaskNotFound = false, Success = true };
    }

    public async Task<IEnumerable<TaskItem>?> GetAllTasksAsync()
    {
        var taskEntities = await _dbContext.Tasks
           .AsNoTracking()
           .ToListAsync();

        if (!taskEntities.Any()) return null;

        return taskEntities;
    }

    public async Task<Project?> GetProjectByTaskAsync(int id)
    {
        var task = await _dbContext.Tasks
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (task == null) return null;

        if (task.Project == null) return null;

        return task.Project;
    }

    public async Task<TaskOperationResult> CreateTaskAsync(TaskItem task)
    {
        if (task == null) return new TaskOperationResult { Task = null, Success = false, TaskNotFound = false };

        bool taskExists = await _dbContext.Tasks
            .AnyAsync(t => t.ProjectId == task.ProjectId &&
                           t.Title.ToLower() == task.Title.ToLower());

        if (taskExists) return new TaskOperationResult { Task = null, Success = false, TaskExists = true };

        if (!await _dbContext.Projects.AnyAsync(p => p.Id == task.ProjectId))
            return new TaskOperationResult { Task = null, Success = false, TaskNotFound = true };

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

        return new TaskOperationResult { Task = newTask, Success = true };
    }

    public async Task<TaskOperationResult> UpdateTaskStatusAsync(int taskId, TaskItemStatus newStatus)
    {
        var task = await _dbContext.Tasks.FindAsync(taskId);
        if (task == null)
            return new TaskOperationResult { Task = null, Success = false, TaskNotFound = true };

        if (!IsValidStatusTransition(task.Status, newStatus))
            return new TaskOperationResult { Task = null, Success = false, InvalidTransition = true };

        task.Status = newStatus;
        await _dbContext.SaveChangesAsync();
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
            return new TaskOperationResult { Task = null, Success = false, TaskNotFound = true };

        task!.ProjectId = projectId;
        await _dbContext.SaveChangesAsync();
        return new TaskOperationResult { Task = task, Success = true };
    }

    public async Task<TaskOperationResult> DeleteTaskAsync(int taskId)
    {
        var (task, failure) = await FindTaskAsync(taskId);
        if (failure) 
            return new TaskOperationResult { Task = null, Success = false, TaskNotFound = true };

        _dbContext.Tasks.Remove(task!);
        await _dbContext.SaveChangesAsync();
        return new TaskOperationResult { Task = task, Success = true };
    }
    private async Task<(TaskItem? Task, bool Failure)> FindTaskAsync(int taskId)
    {
        var task = await _dbContext.Tasks.FindAsync(taskId);
        if (task == null) return (default, true);
        return (task, false);
    }
}