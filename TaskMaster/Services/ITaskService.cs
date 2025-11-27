using Microsoft.AspNetCore.Mvc;
using TaskMaster.Models;
using TaskMaster.Models.DTO;
using TaskMaster.Models.Enums;

namespace TaskMaster.Services;
public interface ITaskService
{
    Task<ActionResult<TaskItemResponse>> GetTaskByIdAsync(int id);
    Task<ActionResult<IEnumerable<TaskItemResponse>>> GetAllTasksAsync();
    Task<ActionResult<ProjectResponse>> GetProjectByTaskAsync(int id);
    Task<ActionResult<TaskItemResponse>> CreateTaskAsync(CreateTaskItemDto task);
    Task<Result> UpdateTaskStatusAsync(int taskId, TaskItemStatus newStatus);
    Task<Result> AssignTaskAsync(int taskId, int projectId);
    Task<Result> DeleteTaskAsync(int taskId);
}