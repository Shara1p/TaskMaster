using Microsoft.AspNetCore.Mvc;
using Mapster;
using Microsoft.EntityFrameworkCore;
using TaskMaster.Data;
using TaskMaster.Models;
using TaskMaster.Models.DTO;
using TaskMaster.Models.Enums;

namespace TaskMaster.Services;

public class TaskService : ITaskService
{
    private readonly ApplicationDbContext _dbContext;

    public TaskService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ActionResult<TaskItemResponse>> GetTaskByIdAsync(int id)
    {
        var task = await _dbContext.Tasks.FindAsync(id);
        if (task == null)
        {
            return new NotFoundResult();
        }

        return new OkObjectResult(task.Adapt<TaskItemResponse>());
    }

    public async Task<ActionResult<IEnumerable<TaskItemResponse>>> GetAllTasksAsync()
    {
        var taskEntities = await _dbContext.Tasks.ToListAsync();
        return taskEntities.Adapt<List<TaskItemResponse>>();
    }

    public async Task<ActionResult<ProjectResponse>> GetProjectByTaskAsync(int id)
    {
        var task = await _dbContext.Tasks
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (task == null)
        {
            return new NotFoundResult();
        }

        if (task.Project == null)
        {
            return new NotFoundObjectResult("Project not found or task doesn't link to any project");
        }
        return task.Project.Adapt<ProjectResponse>();
    }

    public async Task<ActionResult<TaskItemResponse>> CreateTaskAsync(CreateTaskItemDto task)
    {
        if (task == null)
        {
            return new BadRequestObjectResult("Task data is required");
        }

        bool taskExists = await _dbContext.Tasks
            .AnyAsync(t => t.ProjectId == task.ProjectId &&
                           t.Title.ToLower() == task.Title.ToLower());

        if (taskExists)
        {
            return new BadRequestObjectResult($"Task '{task.Title}' already exists in this project");
        }

        if (!await _dbContext.Projects.AnyAsync(p => p.Id == task.ProjectId))
        {
            return new BadRequestObjectResult($"Project with ID {task.ProjectId} does not exist");
        }

        var newTask = new TaskItem
        {
            Title = task.Title,
            Description = task.Description,
            Created = DateTime.Now,
            DueDate = task.DueDate,
            ProjectId = task.ProjectId,
            Status = Enum.Parse<TaskItemStatus>(task.Status)
        };

        _dbContext.Add(newTask);
        await _dbContext.SaveChangesAsync();

        var response = newTask.Adapt<TaskItemResponse>();
        return new CreatedResult($"/api/tasks/{newTask.Id}", response);
    }

    public async Task<Result> UpdateTaskStatusAsync(int taskId, TaskItemStatus newStatus)
    {
        var task = await _dbContext.Tasks.FindAsync(taskId);
        if (task == null)
        {
            return Result.Failure($"Task with ID {taskId} not found");
        }
        if (!IsValidStatusTransition(task.Status, newStatus))
        {
            return Result.Failure($"Invalid status transition from {task.Status} to {newStatus}");
        }
        task.Status = newStatus;
        await _dbContext.SaveChangesAsync();
        return Result.Success();
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

    public async Task<Result> AssignTaskAsync(int taskId, int projectId)
    {
        var (task, failure) = await GetTaskOrFailureAsync(taskId);
        if (failure != null) return failure;
        task!.ProjectId = projectId;
        await _dbContext.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> DeleteTaskAsync(int taskId)
    {
        var (task, failure) = await GetTaskOrFailureAsync(taskId);
        if (failure != null) return failure;

        _dbContext.Tasks.Remove(task!);
        await _dbContext.SaveChangesAsync();
        return Result.Success();
    }
    private async Task<(TaskItem? Task, Result? Failure)> GetTaskOrFailureAsync(int taskId)
    {
        var task = await _dbContext.Tasks.FindAsync(taskId);
        if (task == null) return (default, Result.Failure($"Task with ID {taskId} not found"));
        return (task, null);
    }
}